using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float stopDistance = 4f; // Дистанция остановки от игрока
    public float wanderRadius = 10f; // Радиус блуждания

    private Transform player;
    private Vector3 spawnPosition; // Позиция где NPC появился
    private Vector3 targetPosition; // Текущая целевая позиция
    private NPCState currentState = NPCState.MovingToPlayer;
    private float stateTimer = 0f;

    // Состояния NPC
    private enum NPCState
    {
        MovingToPlayer,     // Движется к игроку
        WaitingForInteraction, // Ждет взаимодействия с игроком
        ReturningToSpawn,   // Возвращается к точке спавна
        MovingToRandom,     // Движется в случайную точку
        Disappearing        // Исчезает
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnPosition = transform.position; // Запоминаем точку появления
        Debug.Log("NPC создан в позиции: " + spawnPosition);
    }

    void Update()
    {
        // Обновляем таймер состояния
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case NPCState.MovingToPlayer:
                MoveToPlayer();
                break;

            case NPCState.WaitingForInteraction:
                // Ждем когда игрок нажмет E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartInteraction();
                }
                break;

            case NPCState.ReturningToSpawn:
                MoveToPosition(spawnPosition);

                // Если дошли до точки спавна или прошло больше 10 секунд
                if (Vector3.Distance(transform.position, spawnPosition) < 0.5f || stateTimer > 10f)
                {
                    GoToRandomPosition();
                }
                break;

            case NPCState.MovingToRandom:
                MoveToPosition(targetPosition);

                // Если дошли до цели или прошло больше 10 секунд
                if (Vector3.Distance(transform.position, targetPosition) < 0.5f || stateTimer > 10f)
                {
                    Disappear();
                }
                break;

            case NPCState.Disappearing:
                // Ничего не делаем, просто ждем уничтожения
                break;
        }
    }

    void MoveToPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Если дистанция больше stopDistance, двигаемся к игроку
        if (distanceToPlayer > stopDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Поворачиваем NPC к игроку
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }
        }
        else
        {
            // Достигли нужной дистанции
            currentState = NPCState.WaitingForInteraction;
            stateTimer = 0f;
            Debug.Log("NPC достиг игрока и ждет взаимодействия");
        }
    }

    void MoveToPosition(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Поворачиваем NPC к цели
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }
    }

    void StartInteraction()
    {
        Debug.Log("Начато взаимодействие с NPC");

        // Запускаем корутину ожидания и возврата к спавну
        StartCoroutine(WaitAndReturnToSpawn());
    }

    IEnumerator WaitAndReturnToSpawn()
    {
        Debug.Log("NPC ждет 5 секунд перед возвратом...");

        // Ждем 5 секунд
        yield return new WaitForSeconds(5f);

        // Начинаем возврат к точке спавна
        currentState = NPCState.ReturningToSpawn;
        stateTimer = 0f;
        Debug.Log("NPC возвращается к точке спавна: " + spawnPosition);
    }

    void GoToRandomPosition()
    {
        // Генерируем случайную позицию в радиусе wanderRadius от точки спавна
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPosition = spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Убедимся что позиция на земле
        RaycastHit hit;
        if (Physics.Raycast(targetPosition + Vector3.up * 10, Vector3.down, out hit, 20, LayerMask.GetMask("Ground")))
        {
            targetPosition = hit.point;
        }

        currentState = NPCState.MovingToRandom;
        stateTimer = 0f;
        Debug.Log("NPC идет в случайную точку: " + targetPosition);
    }

    void Disappear()
    {
        currentState = NPCState.Disappearing;
        Debug.Log("NPC исчезает");
        Destroy(gameObject);
    }

    // Визуальная отладка в редакторе
    void OnDrawGizmos()
    {
        // Показываем дистанцию остановки
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Показываем точку спавна
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnPosition, Vector3.one);
            Gizmos.DrawLine(transform.position, spawnPosition);
        }

        // Показываем текущую цель
        if (currentState == NPCState.MovingToRandom)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(targetPosition, Vector3.one);
            Gizmos.DrawLine(transform.position, targetPosition);
        }

        // Показываем радиус блуждания
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnPosition, wanderRadius);
    }
}