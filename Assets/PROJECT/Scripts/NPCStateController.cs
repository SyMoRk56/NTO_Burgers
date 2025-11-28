using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NPCStateController : MonoBehaviour
{
    // Определение состояний NPC
    public enum NPCState
    {
        SplinePatrol,       // 1. Прогулка по траектории
        InteractWithObject, // 2. Подойти к объекту и взаимодействовать
        PatrolWithStops     // 3. Прогулка с периодическими остановками
    }

    [Header("Настройки состояний")]
    public NPCState currentState = NPCState.SplinePatrol; // ДОБАВЛЕНО: объявление переменной
    public List<NPCState> availableStates = new List<NPCState> { NPCState.SplinePatrol, NPCState.PatrolWithStops, NPCState.InteractWithObject };
    public float minStateTime = 10f;
    public float maxStateTime = 25f;

    [Header("Настройки передвижения (Spline)")]
    public List<Transform> splineWaypoints;
    public float patrolSpeed = 3.5f;
    public float runSpeed = 5f;
    public float waitTimeAtStop = 3.0f;

    [Header("Настройки взаимодействия")]
    public List<Transform> interactTargets; // Несколько возможных целей для взаимодействия
    public float interactDistance = 2.0f;

    [Header("Анимации (Имена параметров)")]
    public string walkingBool = "IsWalking";
    public string runningBool = "IsRunning";
    public string interactTrigger = "Interact";
    public string idleTrigger = "Idle";

    // Внутренние переменные
    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private float stopTimer = 0f;
    private bool isWaiting = false;
    private bool hasInteracted = false;
    private float stateTimer = 0f;
    private float currentStateDuration = 0f;
    private Transform currentInteractTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Устанавливаем случайное начальное состояние
        SetRandomState();

        // Устанавливаем случайную длительность для первого состояния
        ResetStateTimer();
    }

    void Update()
    {
        // Обновляем таймер состояния
        stateTimer += Time.deltaTime;

        // Проверяем, не пора ли сменить состояние
        if (stateTimer >= currentStateDuration)
        {
            SetRandomState();
            ResetStateTimer();
        }

        // Машина состояний
        switch (currentState)
        {
            case NPCState.SplinePatrol:
                HandleSplinePatrol();
                break;

            case NPCState.InteractWithObject:
                HandleInteraction();
                break;

            case NPCState.PatrolWithStops:
                HandlePatrolWithStops();
                break;
        }

        // Обновление анимации движения
        UpdateMovementAnimation();
    }

    // --- ОСНОВНЫЕ МЕТОДЫ СОСТОЯНИЙ ---

    void HandleSplinePatrol()
    {
        // Иногда переключаемся на бег для разнообразия
        if (Random.Range(0f, 1f) < 0.3f && !animator.GetBool(runningBool))
        {
            agent.speed = runSpeed;
            animator.SetBool(runningBool, true);
        }
        else
        {
            agent.speed = patrolSpeed;
            animator.SetBool(runningBool, false);
        }

        if (splineWaypoints.Count == 0) return;

        MoveToNextWaypoint();
    }

    void HandleInteraction()
    {
        if (currentInteractTarget == null)
        {
            // Выбираем случайную цель для взаимодействия
            if (interactTargets.Count > 0)
            {
                currentInteractTarget = interactTargets[Random.Range(0, interactTargets.Count)];
            }
            else
            {
                // Если нет целей, переключаемся на другое состояние
                SetRandomState();
                return;
            }
        }

        if (!hasInteracted)
        {
            agent.SetDestination(currentInteractTarget.position);
            agent.speed = patrolSpeed;

            // Проверяем, дошли ли мы до объекта
            if (!agent.pathPending && agent.remainingDistance <= interactDistance)
            {
                // Останавливаем агента
                agent.isStopped = true;

                // Поворачиваемся к объекту
                Vector3 direction = (currentInteractTarget.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }

                // Проигрываем анимацию взаимодействия
                animator.SetTrigger(interactTrigger);
                hasInteracted = true;

                // Через некоторое время сбрасываем взаимодействие
                StartCoroutine(ResetInteractionAfterDelay(Random.Range(2f, 5f)));
            }
        }
    }

    void HandlePatrolWithStops()
    {
        agent.speed = patrolSpeed;

        if (isWaiting) return;

        MoveToNextWaypoint();

        // Логика таймера для случайной остановки
        stopTimer += Time.deltaTime;

        // Случайные остановки с вероятностью
        if (stopTimer > 5.0f && Random.Range(0f, 1f) < 0.4f)
        {
            StartCoroutine(PerformStop());
            stopTimer = 0f;
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    void MoveToNextWaypoint()
    {
        if (splineWaypoints.Count == 0) return;

        // Идем к текущей точке
        agent.SetDestination(splineWaypoints[currentWaypointIndex].position);

        // Если дошли до точки
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Случайно выбираем следующую точку для более естественного движения
            if (Random.Range(0f, 1f) < 0.7f)
            {
                // 70% chance - следующая по порядку точка
                currentWaypointIndex = (currentWaypointIndex + 1) % splineWaypoints.Count;
            }
            else
            {
                // 30% chance - случайная точка
                currentWaypointIndex = Random.Range(0, splineWaypoints.Count);
            }
        }
    }

    void UpdateMovementAnimation()
    {
        // Если агент двигается и у него есть остаточный путь
        bool isMoving = agent.velocity.sqrMagnitude > 0.1f && agent.remainingDistance > agent.stoppingDistance;
        animator.SetBool(walkingBool, isMoving);

        // Если стоим, сбрасываем бег
        if (!isMoving)
        {
            animator.SetBool(runningBool, false);
        }
    }

    void SetRandomState()
    {
        if (availableStates.Count == 0) return;

        NPCState newState = availableStates[Random.Range(0, availableStates.Count)];

        // Не переключаем на то же состояние
        if (newState == currentState && availableStates.Count > 1)
        {
            // Пробуем еще раз
            do
            {
                newState = availableStates[Random.Range(0, availableStates.Count)];
            } while (newState == currentState);
        }

        ChangeState(newState);
    }

    void ResetStateTimer()
    {
        currentStateDuration = Random.Range(minStateTime, maxStateTime);
        stateTimer = 0f;
    }

    IEnumerator PerformStop()
    {
        isWaiting = true;
        agent.isStopped = true;
        animator.SetTrigger(idleTrigger);

        // Случайное время остановки
        float stopDuration = Random.Range(waitTimeAtStop * 0.5f, waitTimeAtStop * 1.5f);
        yield return new WaitForSeconds(stopDuration);

        agent.isStopped = false;
        isWaiting = false;
    }

    IEnumerator ResetInteractionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hasInteracted = false;
        currentInteractTarget = null;
        agent.isStopped = false;
    }

    // Метод для переключения состояний
    public void ChangeState(NPCState newState)
    {
        currentState = newState;

        // Сброс флагов при смене состояния
        hasInteracted = false;
        isWaiting = false;
        agent.isStopped = false;
        stopTimer = 0f;
        animator.SetBool(runningBool, false);

        StopAllCoroutines();

        Debug.Log($"NPC переключился в состояние: {newState}");
    }

    // Метод для принудительной смены состояния извне
    public void ForceStateChange(NPCState newState)
    {
        ChangeState(newState);
        ResetStateTimer();
    }
}