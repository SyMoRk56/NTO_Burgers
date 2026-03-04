using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum TutorialDialogueType
{
    OpenInventory,
    DeliverLetter,
    Farewell
}

/// <summary>
/// Туториальный NPC.
/// 
/// Настройка префаба — точно так же как обычный NPC:
/// 1. Возьми любой существующий NPC префаб как основу (скопируй)
/// 2. На DialogueRunner заполни defaultDialogues:
///    - Element 0 → диалог "Открой инвентарь и посмотри письма"
///    - Element 1 → диалог "Доставь письмо персонажу X"
///    - Element 2 → диалог прощания (опционально)
/// 3. Добавь этот скрипт TutorialNPC на тот же объект
/// 4. Убедись что DialogueUI назначен в DialogueRunner (как у обычных NPC)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DialogueRunner))]
public class TutorialNPC : MonoBehaviour
{
    [Header("Индексы диалогов в DialogueRunner.defaultDialogues")]
    [Tooltip("Element 0 в defaultDialogues — 'Открой инвентарь'")]
    public int dialogueIndexOpenInventory = 0;

    [Tooltip("Element 1 в defaultDialogues — 'Доставь письмо'")]
    public int dialogueIndexDeliverLetter = 1;

    [Tooltip("Element 2 в defaultDialogues — прощание")]
    public int dialogueIndexFarewell = 2;

    [Header("Движение")]
    public float stopDistance = 2.2f;

    private NavMeshAgent agent;
    private DialogueRunner dialogueRunner;
    private NPCBehaviour npcBehaviour;
    private bool isApproaching = false;

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
                Debug.LogError("[TutorialNPC] defaultDialogues пуст! Заполни диалоги в DialogueRunner на префабе TutorialNPC.");
        }
        else
        {
            Debug.LogError("[TutorialNPC] DialogueRunner не найден на объекте!");
        }
    }

    // ── Движение к игроку ──────────────────────────────────

    public void ApproachPlayer()
    {
        Debug.Log($"[TutorialNPC] ApproachPlayer. isApproaching={isApproaching}");
        if (isApproaching) return;

        isApproaching = true;

        // Останавливаем обычное патрулирование
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
            Debug.LogError("[TutorialNPC] NPC не на NavMesh! Проверь точку спавна.");
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

                // Повернуться к игроку
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

    // ── Диалоги ────────────────────────────────────────────

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
            Debug.LogError($"[TutorialNPC] Диалог с индексом {index} не существует! " +
                           $"defaultDialogues.Length={dialogueRunner.defaultDialogues?.Length ?? 0}. " +
                           $"Добавь нужные DialogueScriptableObject в DialogueRunner.");
            return;
        }

        Debug.Log($"[TutorialNPC] ShowDialogue({type}) → index={index}, " +
                  $"диалог='{dialogueRunner.defaultDialogues[index].name}'");

        // Если диалог уже запущен — принудительно закрываем
        if (dialogueRunner.IsDialogueActive)
        {
            Debug.Log("[TutorialNPC] Диалог уже активен — закрываем перед новым");
            dialogueRunner.ForceCloseDialogue();
        }

        // Устанавливаем стартовый индекс и запускаем
        // DialogueRunner.StartDialogue() начинает с currentDialogueIndex=0,
        // поэтому нам нужно запустить нужный блок напрямую через Choose
        dialogueRunner.StartDialogue(false);

        // Если нужен не нулевой индекс — перематываем через Choose
        if (index > 0)
        {
            // Форсируем переход к нужному блоку
            // StartDialogue всегда начинает с 0, поэтому если нам нужен index 1 или 2
            // — просто переставляем defaultDialogues так чтобы нужный был первым
            // Это безопаснее чем ломать DialogueRunner
            dialogueRunner.ForceCloseDialogue();

            // Временно переставляем нужный диалог на первое место
            var temp = dialogueRunner.defaultDialogues[0];
            dialogueRunner.defaultDialogues[0] = dialogueRunner.defaultDialogues[index];
            dialogueRunner.defaultDialogues[index] = temp;

            dialogueRunner.StartDialogue(false);

            // Возвращаем обратно после запуска (через кадр)
            StartCoroutine(RestoreDialogueOrder(index, temp));
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

    // ── Завершение туториала ───────────────────────────────

    public void OnTutorialComplete()
    {
        Debug.Log("[TutorialNPC] OnTutorialComplete");
        ShowDialogue(TutorialDialogueType.Farewell);
        StartCoroutine(DisappearAfterDelay(6f));
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("[TutorialNPC] NPC исчезает");
        gameObject.SetActive(false);
    }
}