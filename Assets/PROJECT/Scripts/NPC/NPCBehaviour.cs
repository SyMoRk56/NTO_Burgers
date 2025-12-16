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

    [HideInInspector] public bool dialogueActive = false;

    // ДЛЯ СИСТЕМЫ СОХРАНЕНИЙ
    public int CurrentActionIndex { get; private set; } = 0;
    public string CurrentTargetName { get; private set; } = "";

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;

        StartCoroutine(ActionRoutine());
    }

    private void Update()
    {
        if (!dialogueActive)
            RotateTowardsMovementDirection();
    }

    public void Stop()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetBool(moveAnimParameter, false);
            animator.SetTrigger("isIdle");
        }
    }

    public void Resume()
    {
        if (agent != null)
            agent.isStopped = false;
    }

    // ============================================
    //   MAIN ROUTINE
    // ============================================
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

    // ============================================
    //   WALK ROUTINE
    // ============================================
    private IEnumerator WalkToTarget(Transform target, Vector2 waitRange)
    {
        if (target == null) yield break;

        CurrentTargetName = target.name;

        agent.isStopped = false;
        agent.SetDestination(target.position);

        if (animator != null)
            animator.SetBool(moveAnimParameter, true);

        while (agent.pathPending ||
               agent.remainingDistance > agent.stoppingDistance ||
               agent.velocity.sqrMagnitude > 0.01f)
        {
            if (dialogueActive)
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

    // ============================================
    //   INTERACT ROUTINE
    // ============================================
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
            if (dialogueActive)
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

    // ============================================
    //   RESTORE FROM SAVE
    // ============================================
    public void RestoreStateFromSave(NPCSaveData d)
    {
        StopAllCoroutines();

        CurrentActionIndex = d.currentActionIndex;
        CurrentTargetName = d.currentTargetName;

        // Ищем цель по имени
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

        StartCoroutine(ActionRoutineFromIndex());
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

    // ============================================
    //   ROTATION
    // ============================================
    private void RotateTowardsMovementDirection()
    {
        if (agent.velocity.sqrMagnitude < 0.01f) return;

        Vector3 dir = agent.velocity.normalized;
        dir.y = 0;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 8f
        );
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
