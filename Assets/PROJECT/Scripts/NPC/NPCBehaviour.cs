using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCBehaviour : MonoBehaviour
{
    [Header("Base Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Animation Parameters")]
    [SerializeField] private string moveAnimParameter = "isMoving";

    [Header("Action Sequence")]
    [SerializeField] private NPCAction[] actions;

    [Header("Day / Night")]
    [SerializeField] private Transform homePoint;
    [SerializeField] private bool reactsToDayNight = true;

    [HideInInspector] public bool dialogueActive = false;

    public int CurrentActionIndex { get; private set; } = 0;
    public string CurrentTargetName { get; private set; } = "";

    private bool isNight = false;
    private Coroutine actionCoroutine;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // отключаем авторотацию агента — крутим NPC сами через velocity, иначе будут дёрганья
        agent.updateRotation = false;

        actionCoroutine = StartCoroutine(ActionRoutine());
    }

    private void Update()
    {
        // во время диалога и ночью NPC стоит — крутить его не нужно
        if (!dialogueActive && !isNight)
            RotateTowardsMovementDirection();
    }

    private void OnTimeOfDayChanged(int timeIndex)
    {
        if (!reactsToDayNight) return;

        // индексы времени суток: 0 - рассвет, 1 - день, 2 - закат, 3 - ночь
        switch (timeIndex)
        {
            case 2: GoHomeAtSunset(); break;
            case 3: EnterNight(); break;
            case 0:
            case 1: ExitNight(); break;
        }
    }

    private void GoHomeAtSunset()
    {
        if (homePoint == null || isNight) return;

        // прерываем текущий маршрут и гоним NPC домой
        StopAllCoroutines();
        Stop();

        agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(homePoint.position);

        animator.SetBool(moveAnimParameter, true);

        StartCoroutine(CheckArrivalAtHome());
    }

    private IEnumerator CheckArrivalAtHome()
    {
        while (Vector3.Distance(transform.position, homePoint.position) > agent.stoppingDistance + 0.1f)
            yield return null;

        // дошли до дома — прячем NPC до утра
        agent.isStopped = true;
        SetInvisibleAndDisable();
    }

    private void EnterNight()
    {
        if (isNight) return;
        isNight = true;

        // если NPC не успел дойти домой — просто скрываем его на месте
        StopAllCoroutines();
        Stop();
        SetInvisibleAndDisable();
    }

    private void ExitNight()
    {
        if (!isNight) return;
        isNight = false;

        SetVisibleAndEnable();
        agent.isStopped = false;

        // продолжаем с того действия, на котором остановились перед ночью
        actionCoroutine = StartCoroutine(ActionRoutineFromIndex());
    }

    public void Stop()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // без этого агент ещё немного едет по инерции
        }

        animator.SetBool(moveAnimParameter, false);
        animator.SetTrigger("isIdle");
    }

    public void Resume()
    {
        if (agent != null)
            agent.isStopped = false;
    }

    private IEnumerator ActionRoutine()
    {
        while (true)
        {
            // ждём пока игрок не закроет диалог прежде чем начать маршрут
            while (dialogueActive)
                yield return null;

            for (CurrentActionIndex = 0; CurrentActionIndex < actions.Length; CurrentActionIndex++)
            {
                // диалог мог начаться пока мы переходили к следующему действию
                while (dialogueActive) yield return null;

                NPCAction act = actions[CurrentActionIndex];

                switch (act.actionType)
                {
                    case NPCActionType.Walk:
                        yield return WalkToTarget(act.walkTarget, act.waitAfterWalk);
                        break;

                    case NPCActionType.Interact:
                        yield return InteractWithObject(act);
                        break;
                }
            }
            // actions кончились — цикл while(true) начнёт их заново с нуля
        }
    }

    // та же рутина что ActionRoutine, но стартует не с нуля —
    // используется после загрузки сохранения или выхода из ночного режима
    private IEnumerator ActionRoutineFromIndex()
    {
        while (true)
        {
            NPCAction act = actions[CurrentActionIndex];

            switch (act.actionType)
            {
                case NPCActionType.Walk:
                    yield return WalkToTarget(act.walkTarget, act.waitAfterWalk);
                    break;

                case NPCActionType.Interact:
                    yield return InteractWithObject(act);
                    break;
            }

            CurrentActionIndex++;
            if (CurrentActionIndex >= actions.Length)
                CurrentActionIndex = 0; // зацикливаем маршрут
        }
    }

    private IEnumerator WalkToTarget(Transform target, Vector2 waitRange)
    {
        if (target == null) yield break;

        CurrentTargetName = target.name;

        agent.isStopped = false;
        agent.SetDestination(target.position);
        animator.SetBool(moveAnimParameter, true);

        while (agent.pathPending ||
               agent.remainingDistance > agent.stoppingDistance ||
               agent.velocity.sqrMagnitude > 0.01f)
        {
            // прибиваем Y таргета к высоте NPC — без этого на неровном рельефе
            // агент думает что не добрался до точки из-за разницы высот
            target.position = new Vector3(target.position.x, transform.position.y, target.position.z);

            if (dialogueActive || isNight)
            {
                Stop();
                yield break; // выходим, не дожидаясь точки назначения
            }

            yield return null;
        }

        animator.SetBool(moveAnimParameter, false);
        animator.SetTrigger("isIdle");

        // случайная пауза перед следующим действием, чтобы NPC не выглядел как робот
        float r = Random.Range(waitRange.x, waitRange.y);
        yield return new WaitForSeconds(r);
    }

    private IEnumerator InteractWithObject(NPCAction act)
    {
        if (act.interactObject == null) yield break;

        CurrentTargetName = act.interactObject.name;

        agent.isStopped = false;
        agent.SetDestination(act.interactObject.position);
        animator.SetBool(moveAnimParameter, true);

        // идём пока не войдём в радиус взаимодействия (Y игнорируем — объект может быть выше/ниже)
        while (Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(act.interactObject.position.x, 0, act.interactObject.position.z)
        ) > act.interactionDistance)
        {
            if (dialogueActive || isNight)
            {
                Stop();
                yield break;
            }

            yield return null;
        }

        // остановились — запускаем анимацию взаимодействия
        agent.isStopped = true;
        animator.SetBool(moveAnimParameter, false);
        animator.SetTrigger(act.interactionTrigger);

        yield return new WaitForSeconds(act.interactionDuration);

        agent.isStopped = false;
    }

    public void RestoreStateFromSave(NPCSaveData d)
    {
        StopAllCoroutines();

        CurrentActionIndex = d.currentActionIndex;
        CurrentTargetName = d.currentTargetName;

        // ищем трансформ по имени среди всех actions чтобы сразу отправить агента туда,
        // иначе NPC будет стоять на месте пока не начнётся следующее действие
        Transform target = null;
        foreach (var a in actions)
        {
            if (a.walkTarget != null && a.walkTarget.name == d.currentTargetName)
                target = a.walkTarget;

            if (a.interactObject != null && a.interactObject.name == d.currentTargetName)
                target = a.interactObject;
        }

        if (target != null)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }

        actionCoroutine = StartCoroutine(ActionRoutineFromIndex());
    }

    private void RotateTowardsMovementDirection()
    {
        // если агент почти не движется — не трогаем rotation, иначе NPC дёргается на месте
        if (agent.velocity.sqrMagnitude < 0.01f) return;

        Vector3 dir = agent.velocity.normalized;
        dir.y = 0; // убираем вертикальную составляющую — NPC не должен наклоняться

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 8f
        );
    }

    private void SetInvisibleAndDisable()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // отключаем агента полностью, иначе он продолжает считать пути в фоне и тратит CPU
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }

    private void SetVisibleAndEnable()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = true;

        if (agent != null)
            agent.enabled = true;
    }
}

public enum NPCActionType
{
    Walk,
    Interact
}

[System.Serializable]
public class NPCAction
{
    public NPCActionType actionType;

    public Transform walkTarget;
    public Vector2 waitAfterWalk = new Vector2(1f, 3f);

    public Transform interactObject;
    public string interactionTrigger = "Interact";
    public float interactionDistance = 1.5f;
    public float interactionDuration = 2f;
}