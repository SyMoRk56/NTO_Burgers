using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum TutorialDialogueType
{
    OpenInventory,  // "Открой инвентарь и посмотри письма"
    DeliverLetter,  // "Доставь письмо персонажу X"
    Farewell        // Прощание
}

/// <summary>
/// Туториальный NPC.
/// 
/// Настройка:
/// 1. Создай GameObject с NavMeshAgent, NPCBehaviour, DialogueRunner.
/// 2. Добавь этот компонент.
/// 3. Заполни три DialogueScriptableObject для каждой фразы туториала.
/// 4. Сделай из него префаб и назначь в TutorialManager.tutorialNPCPrefab.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DialogueRunner))]
public class TutorialNPC : MonoBehaviour
{
    [Header("Диалоги туториала")]
    [Tooltip("Диалог #1: 'Открой инвентарь и посмотри письма'")]
    public DialogueScriptableObject dialogueOpenInventory;

    [Tooltip("Диалог #2: 'Доставь письмо персонажу X'")]
    public DialogueScriptableObject dialogueDeliverLetter;

    [Tooltip("Диалог #3: Прощание после завершения туториала")]
    public DialogueScriptableObject dialogueFarewell;

    [Header("Движение")]
    [Tooltip("Дистанция от игрока, при которой NPC останавливается и начинает диалог")]
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
    }

    // ── Движение к игроку ──────────────────────────────────

    /// <summary>Начать движение к игроку. Вызывает TutorialManager.</summary>
    public void ApproachPlayer()
    {
        if (isApproaching) return;
        isApproaching = true;

        // Отключаем обычное патрулирование на время туториала
        if (npcBehaviour != null)
        {
            npcBehaviour.dialogueActive = true;
            npcBehaviour.Stop();
        }

        StartCoroutine(MoveToPlayerCoroutine());
    }

    private IEnumerator MoveToPlayerCoroutine()
    {
        GameObject player = GameManager.Instance.GetPlayer();
        if (player == null) yield break;

        agent.isStopped = false;

        while (true)
        {
            if (player == null) yield break;

            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist <= stopDistance)
            {
                agent.isStopped = true;

                // Повернуться к игроку
                Vector3 dir = (player.transform.position - transform.position).normalized;
                dir.y = 0f;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);

                yield return new WaitForSeconds(0.4f);

                // Показываем первый диалог туториала
                ShowDialogue(TutorialDialogueType.OpenInventory);
                TutorialManager.Instance?.OnNPCReachedPlayer();
                yield break;
            }

            agent.SetDestination(player.transform.position);
            yield return new WaitForSeconds(0.15f);
        }
    }

    // ── Диалоги ────────────────────────────────────────────

    /// <summary>Показать нужный туториальный диалог. Вызывает TutorialManager.</summary>
    public void ShowDialogue(TutorialDialogueType type)
    {
        DialogueScriptableObject dlg = type switch
        {
            TutorialDialogueType.OpenInventory => dialogueOpenInventory,
            TutorialDialogueType.DeliverLetter => dialogueDeliverLetter,
            TutorialDialogueType.Farewell => dialogueFarewell,
            _ => null
        };

        if (dlg == null)
        {
            Debug.LogWarning($"[TutorialNPC] Диалог {type} не назначен в инспекторе!");
            return;
        }

        // Подменяем defaultDialogues нужным туториальным диалогом
        dialogueRunner.defaultDialogues = new DialogueScriptableObject[] { dlg };

        // Блокируем игрока и запускаем диалог принудительно
        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var pm = player.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.ShowCursor(true);
                pm.CanMove = false;
            }
        }

        dialogueRunner.StartDialogue(false);
    }

    // ── Завершение туториала ───────────────────────────────

    public void OnTutorialComplete()
    {
        ShowDialogue(TutorialDialogueType.Farewell);
        StartCoroutine(DisappearAfterDelay(5f));
    }

    private IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Плавно исчезаем или просто деактивируем
        gameObject.SetActive(false);
    }
}