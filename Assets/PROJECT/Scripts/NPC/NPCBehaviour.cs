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
        StartCoroutine(ActionRoutine());
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
                        print("Walk to target");
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
        animator.SetBool(moveAnimParameter, true);
        print("Move");
        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(target.position.x, 0, target.position.z)) > agent.stoppingDistance)
        {
            yield return null;
        }

        animator.SetBool(moveAnimParameter, false);
        print("EndMoving");
        var r = Random.Range(waitRange.x, waitRange.y);
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

        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(act.interactObject.position.x, 0, act.interactObject.position.z)) > act.interactionDistance)
        {
            yield return null;
        }

        agent.isStopped = true;
        animator.SetBool(moveAnimParameter, false);

        animator.SetTrigger(act.interactionTrigger);

        yield return new WaitForSeconds(act.interactionDuration);

        agent.isStopped = false;
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
