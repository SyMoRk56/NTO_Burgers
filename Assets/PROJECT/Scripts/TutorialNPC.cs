using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum TutorialDialogueType
{
    OpenInventory,
    DeliverLetter,
    Farewell
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DialogueRunner))]
public class TutorialNPC : MonoBehaviour
{
    [Header("Индексы диалогов в DialogueRunner.defaultDialogues")]
    public int dialogueIndexOpenInventory = 0;
    public int dialogueIndexDeliverLetter = 1;
    public int dialogueIndexFarewell = 2;

    [Header("Движение")]
    public float stopDistance = 2.2f;

    [Header("Уход после туториала")]
    [Tooltip("Точка куда NPC уйдёт после прощального диалога")]
    public Transform exitPoint;
    [Tooltip("Расстояние до exitPoint при котором NPC считается дошедшим")]
    public float exitStopDistance = 0.5f;

    private NavMeshAgent agent;
    private DialogueRunner dialogueRunner;
    private NPCBehaviour npcBehaviour;
    private bool isApproaching = false;

    void Start()
    {
        if (exitPoint == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("TutorialNPCExit");

            if (obj != null)
            {
                exitPoint = obj.transform;
            }
            else
            {
                Debug.LogError("TutorialExit point not found!");
            }
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        dialogueRunner = GetComponent<DialogueRunner>();
        npcBehaviour = GetComponent<NPCBehaviour>();

        Debug.Log($"[TutorialNPC] Awake. dialogueRunner={dialogueRunner != null}, npcBehaviour={npcBehaviour != null}");

        if (dialogueRunner != null)
        {
            int count = dialogueRunner.defaultDialogues != null ? dialogueRunner.defaultDialogues.Length : 0;
            Debug.Log($"[TutorialNPC] defaultDialogues.Length={count}");
            if (count == 0)
                Debug.LogError("[TutorialNPC] defaultDialogues пуст!");
        }
        else
        {
            Debug.LogError("[TutorialNPC] DialogueRunner не найден!");
        }
    }

    // ── Движение к игроку ─────────────────────────────────

    public void ApproachPlayer()
    {
        Debug.Log($"[TutorialNPC] ApproachPlayer. isApproaching={isApproaching}");
        if (isApproaching) return;
        isApproaching = true;

        if (npcBehaviour != null)
        {
            npcBehaviour.dialogueActive = true;
            npcBehaviour.Stop();
        }

        StartCoroutine(MoveToPlayerCoroutine());
    }

    private IEnumerator MoveToPlayerCoroutine()
    {
        Debug.Log("[TutorialNPC] Начинаем движение к игроку");

        GameObject player = GameManager.Instance.GetPlayer();
        if (player == null) { Debug.LogError("[TutorialNPC] GetPlayer() == NULL!"); yield break; }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[TutorialNPC] NPC не на NavMesh!");
            yield break;
        }

        agent.isStopped = false;
        int frame = 0;

        while (true)
        {
            if (player == null) yield break;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            frame++;
            if (frame % 60 == 0)
                Debug.Log($"[TutorialNPC] Дистанция до игрока: {dist:F1} / {stopDistance}");

            if (dist <= stopDistance)
            {
                agent.isStopped = true;

                Vector3 dir = (player.transform.position - transform.position).normalized;
                dir.y = 0f;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);

                Debug.Log("[TutorialNPC] Достигли игрока — запускаем диалог OpenInventory");
                yield return new WaitForSeconds(0.4f);

                ShowDialogue(TutorialDialogueType.OpenInventory);
                TutorialManager.Instance?.OnNPCReachedPlayer();
                yield break;
            }

            agent.SetDestination(player.transform.position);
            yield return new WaitForSeconds(0.15f);
        }
    }

    // ── Диалоги ───────────────────────────────────────────

    public void ShowDialogue(TutorialDialogueType type)
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("[TutorialNPC] dialogueRunner == NULL!");
            return;
        }

        int index = type switch
        {
            TutorialDialogueType.OpenInventory => dialogueIndexOpenInventory,
            TutorialDialogueType.DeliverLetter => dialogueIndexDeliverLetter,
            TutorialDialogueType.Farewell => dialogueIndexFarewell,
            _ => 0
        };

        if (dialogueRunner.defaultDialogues == null || index >= dialogueRunner.defaultDialogues.Length)
        {
            Debug.LogError($"[TutorialNPC] Диалог {index} не существует! Length={dialogueRunner.defaultDialogues?.Length ?? 0}");
            return;
        }

        Debug.Log($"[TutorialNPC] ShowDialogue({type}) → index={index}, диалог='{dialogueRunner.defaultDialogues[index].name}'");

        if (dialogueRunner.IsDialogueActive)
        {
            Debug.Log("[TutorialNPC] Диалог активен — закрываем");
            dialogueRunner.ForceCloseDialogue();
        }

        if (index > 0)
        {
            var temp = dialogueRunner.defaultDialogues[0];
            dialogueRunner.defaultDialogues[0] = dialogueRunner.defaultDialogues[index];
            dialogueRunner.defaultDialogues[index] = temp;

            dialogueRunner.StartDialogue(false);
            StartCoroutine(RestoreDialogueOrder(index, temp));
        }
        else
        {
            dialogueRunner.StartDialogue(false);
        }
    }

    private IEnumerator RestoreDialogueOrder(int index, DialogueScriptableObject original)
    {
        yield return null;
        if (dialogueRunner.defaultDialogues != null && index < dialogueRunner.defaultDialogues.Length)
        {
            var current = dialogueRunner.defaultDialogues[0];
            dialogueRunner.defaultDialogues[0] = original;
            dialogueRunner.defaultDialogues[index] = current;
        }
    }

    // ── Завершение туториала ──────────────────────────────

    public void OnTutorialComplete()
    {
        Debug.Log("[TutorialNPC] OnTutorialComplete");
        ShowDialogue(TutorialDialogueType.Farewell);
        StartCoroutine(WalkAwayAfterDialogue());
    }

    private IEnumerator WalkAwayAfterDialogue()
    {
        // Ждём пока диалог закончится
        yield return new WaitForSeconds(0.5f);
        while (dialogueRunner.IsDialogueActive)
            yield return new WaitForSeconds(0.2f);

        Debug.Log("[TutorialNPC] Диалог завершён — идём к exitPoint");

        if (exitPoint == null)
        {
            Debug.LogWarning("[TutorialNPC] exitPoint не назначен — просто исчезаем");
            gameObject.SetActive(false);
            yield break;
        }

        if (!agent.isOnNavMesh)
        {
            gameObject.SetActive(false);
            yield break;
        }

        agent.isStopped = false;
        agent.SetDestination(exitPoint.position);

        // Идём к точке
        while (true)
        {
            float dist = Vector3.Distance(transform.position, exitPoint.position);
            if (dist <= exitStopDistance)
                break;

            // Если NavMesh не может дойти
            if (!agent.pathPending && agent.remainingDistance < exitStopDistance)
                break;

            yield return new WaitForSeconds(0.15f);
        }

        agent.isStopped = true;
        Debug.Log("[TutorialNPC] Дошли до exitPoint — исчезаем");
        gameObject.SetActive(false);
    }
}