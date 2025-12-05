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

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Отключаем встроенный поворот агента
        agent.updateRotation = false;

        StartCoroutine(ActionRoutine());
    }

    private void Update()
    {
        RotateTowardsMovementDirection();
    }

    private IEnumerator ActionRoutine()
    {
        while (true)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                NPCAction act = actions[i];

                switch (act.actionType)
                {
                    case NPCActionType.Walk:
                        print("Walk to target " + name + act.walkTarget.name);
                        yield return WalkToTarget(act.walkTarget, act.waitAfterWalk);
                        break;

                    case NPCActionType.Interact:
                        yield return InteractWithObject(act);
                        break;
                }
            }
        }
    }

    // -------------------------------------------
    //   WALK ROUTINE
    // -------------------------------------------
    private IEnumerator WalkToTarget(Transform target, Vector2 waitRange)
    {
        if (target == null) yield break;

        agent.isStopped = false;
        agent.SetDestination(target.position);
        animator = GetComponentInChildren<Animator>();
        if(animator != null)
        animator.SetBool(moveAnimParameter, true);

        print("Move " + name);

        while (agent.pathPending ||
       agent.remainingDistance > agent.stoppingDistance ||
       agent.velocity.sqrMagnitude > 0.01f)
        {
            yield return null;
        }

        if(animator != null)
        animator.SetBool(moveAnimParameter, false);
        print("EndMoving");

        float r = Random.Range(waitRange.x, waitRange.y);
        print(r);
        yield return new WaitForSeconds(r);
    }

    // -------------------------------------------
    //   INTERACTION ROUTINE
    // -------------------------------------------
    private IEnumerator InteractWithObject(NPCAction act)
    {
        if (act.interactObject == null) yield break;

        agent.isStopped = false;
        agent.SetDestination(act.interactObject.position);
        animator.SetBool(moveAnimParameter, true);

        while (Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(act.interactObject.position.x, 0, act.interactObject.position.z)
        ) > act.interactionDistance)
        {
            yield return null;
        }

        agent.isStopped = true;
        animator.SetBool(moveAnimParameter, false);

        animator.SetTrigger(act.interactionTrigger);

        yield return new WaitForSeconds(act.interactionDuration);

        agent.isStopped = false;
    }

    // -------------------------------------------
    //   ROTATION TO MOVEMENT DIRECTION
    // -------------------------------------------
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
            Time.deltaTime * 8f   // скорость поворота, можешь менять
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

    [Header("Walk Settings")]
    public Transform walkTarget;
    public Vector2 waitAfterWalk = new Vector2(1f, 3f);

    [Header("Interaction Settings")]
    public Transform interactObject;
    public string interactionTrigger = "Interact";
    public float interactionDistance = 1.5f;
    public float interactionDuration = 2f;
}
