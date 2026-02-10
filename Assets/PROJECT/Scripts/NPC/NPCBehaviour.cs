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

    // ДЛЯ СИСТЕМЫ СОХРАНЕНИЙ
    public int CurrentActionIndex { get; private set; } = 0;
    public string CurrentTargetName { get; private set; } = "";

    private bool isNight = false;
    private Coroutine actionCoroutine;

    // ======================================================
    // UNITY
    // ======================================================
    private void OnEnable()
    {
        DayNightCycle.OnTimeOfDayChanged += OnTimeOfDayChanged;
    }

    private void OnDisable()
    {
        DayNightCycle.OnTimeOfDayChanged -= OnTimeOfDayChanged;
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;

        actionCoroutine = StartCoroutine(ActionRoutine());
    }

    private void Update()
    {
        if (!dialogueActive && !isNight)
            RotateTowardsMovementDirection();
    }

    // ======================================================
    // DAY / NIGHT REACTION
    // ======================================================
    private void OnTimeOfDayChanged(int timeIndex)
    {
        if (!reactsToDayNight) return;

        switch (timeIndex)
        {
            case 2: // 🌇 закат
                GoHomeAtSunset();
                break;

            case 3: // 🌙 ночь
                EnterNight();
                break;

            case 0: // 🌅 рассвет
                ExitNight();
                break;
        }
    }

    private void GoHomeAtSunset()
    {
        if (homePoint == null || isNight) return;

        StopAllCoroutines();
        Stop();

        agent.isStopped = false;
        agent.SetDestination(homePoint.position);

        animator.SetBool(moveAnimParameter, true);
    }

    private void EnterNight()
    {
        if (isNight) return;
        isNight = true;

        StopAllCoroutines();
        Stop();

        agent.isStopped = true;
        SetVisible(false);
    }

    private void ExitNight()
    {
        if (!isNight) return;
        isNight = false;

        SetVisible(true);
        agent.isStopped = false;

        actionCoroutine = StartCoroutine(ActionRoutineFromIndex());
    }

    // ======================================================
    // CONTROL
    // ======================================================
    public void Stop()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        animator.SetBool(moveAnimParameter, false);
        animator.SetTrigger("isIdle");
    }

    public void Resume()
    {
        if (agent != null)
            agent.isStopped = false;
    }

    // ======================================================
    // MAIN ROUTINE
    // ======================================================
    private IEnumerator ActionRoutine()
    {
        while (true)
        {
            while (dialogueActive)
                yield return null;

            for (CurrentActionIndex = 0; CurrentActionIndex < actions.Length; CurrentActionIndex++)
            {
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
        }
    }

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
                CurrentActionIndex = 0;
        }
    }

    // ======================================================
    // WALK
    // ======================================================
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
            if (dialogueActive || isNight)
            {
                Stop();
                yield break;
            }

            yield return null;
        }

        animator.SetBool(moveAnimParameter, false);

        float r = Random.Range(waitRange.x, waitRange.y);
        yield return new WaitForSeconds(r);
    }

    // ======================================================
    // INTERACT
    // ======================================================
    private IEnumerator InteractWithObject(NPCAction act)
    {
        if (act.interactObject == null) yield break;

        CurrentTargetName = act.interactObject.name;

        agent.isStopped = false;
        agent.SetDestination(act.interactObject.position);

        animator.SetBool(moveAnimParameter, true);

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

        agent.isStopped = true;
        animator.SetBool(moveAnimParameter, false);

        animator.SetTrigger(act.interactionTrigger);

        yield return new WaitForSeconds(act.interactionDuration);

        agent.isStopped = false;
    }

    // ======================================================
    // SAVE RESTORE
    // ======================================================
    public void RestoreStateFromSave(NPCSaveData d)
    {
        StopAllCoroutines();

        CurrentActionIndex = d.currentActionIndex;
        CurrentTargetName = d.currentTargetName;

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

    // ======================================================
    // ROTATION & VISIBILITY
    // ======================================================
    private void RotateTowardsMovementDirection()
    {
        if (agent.velocity.sqrMagnitude < 0.01f) return;

        Vector3 dir = agent.velocity.normalized;
        dir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 8f
        );
    }

    private void SetVisible(bool value)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = value;
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
    