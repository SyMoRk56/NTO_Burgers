using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TutorialStep
{
    None = 0,
    WaitForNPCSpawn = 1,  // Слайдшоу показано, ждём выхода из дома
    WaitForNPCApproach = 2,  // NPC заспавнен, идёт к игроку
    WaitForInventoryOpen = 3,// NPC заговорил "открой инвентарь"
    WaitForLetterRead = 4,  // Инвентарь открыт, ждём прочтения письма
    WaitForDelivery = 5,  // Письмо прочитано, ждём доставки
    Completed = 99
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Туториальный NPC")]
    [Tooltip("Префаб туториального NPC")]
    public GameObject tutorialNPCPrefab;
    [Tooltip("Точка спавна — рядом с выходом из дома")]
    public Transform npcSpawnPoint;

    [Header("Hint UI")]
    public TutorialHintUI hintUI;

    [Header("Настройки письма")]
    public string tutorialMailId = "tutorial_letter_01";
    public string tutorialRecipientNpcId = "npc_grandma";

    public TutorialStep CurrentStep { get; private set; } = TutorialStep.None;

    private bool tutorialCompleted = false;
    private TutorialNPC spawnedNPC;

    // ── Singleton ──────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ── Запуск нового слота ────────────────────────────────

    /// <summary>
    /// Вызывается SaveGameManager при создании НОВОГО слота.
    /// Запускает слайдшоу; после него ждём выхода из дома.
    /// </summary>
    public void StartTutorialForNewSlot()
    {
        if (tutorialCompleted) return;
        Debug.Log("[Tutorial] Новый слот — запускаем слайдшоу");
        SetStep(TutorialStep.WaitForNPCSpawn);

        if (TutorialSlideshowUI.Instance != null)
            TutorialSlideshowUI.Instance.ShowSlideshow();
        else
            Debug.LogWarning("[Tutorial] TutorialSlideshowUI не найден!");
    }

    // ── Вызов из TutorialSlideshowUI ──────────────────────

    /// <summary>Слайдшоу закрыто — теперь ждём триггера двери.</summary>
    public void OnSlideshowFinished()
    {
        // Шаг уже WaitForNPCSpawn, просто логируем
        Debug.Log("[Tutorial] Слайдшоу завершено — ждём выхода из дома");
    }

    // ── Триггер двери дома ─────────────────────────────────

    /// <summary>
    /// Вызывается HouseExitTrigger когда игрок вышел из дома.
    /// </summary>
    public void OnPlayerExitedHouse()
    {
        if (CurrentStep != TutorialStep.WaitForNPCSpawn) return;

        Debug.Log("[Tutorial] Игрок вышел из дома — спавним NPC");
        SetStep(TutorialStep.WaitForNPCApproach);
        SpawnAndApproach();
        SaveProgress();
    }

    // ── События от игровых систем ──────────────────────────

    /// <summary>TutorialNPC вызывает, когда подошёл к игроку и начал диалог.</summary>
    public void OnNPCReachedPlayer()
    {
        if (CurrentStep != TutorialStep.WaitForNPCApproach) return;
        SetStep(TutorialStep.WaitForInventoryOpen);
        hintUI?.ShowInventoryHint();
        SaveProgress();
    }

    /// <summary>
    /// Добавь в метод открытия инвентаря:
    ///   TutorialManager.Instance?.OnInventoryOpened();
    /// </summary>
    public void OnInventoryOpened()
    {
        if (CurrentStep != TutorialStep.WaitForInventoryOpen) return;
        Debug.Log("[Tutorial] Инвентарь открыт");
        SetStep(TutorialStep.WaitForLetterRead);
        hintUI?.ShowLetterHint();
        SaveProgress();
    }

    /// <summary>
    /// Добавь в ShowLetter(mail):
    ///   if (mail.id == TutorialManager.Instance?.tutorialMailId)
    ///       TutorialManager.Instance.OnTutorialLetterRead();
    /// </summary>
    public void OnTutorialLetterRead()
    {
        if (CurrentStep != TutorialStep.WaitForLetterRead) return;
        Debug.Log("[Tutorial] Письмо прочитано");
        SetStep(TutorialStep.WaitForDelivery);
        spawnedNPC?.ShowDialogue(TutorialDialogueType.DeliverLetter);
        hintUI?.ShowDeliveryHint(tutorialRecipientNpcId);
        SaveProgress();
    }

    /// <summary>
    /// Добавь в MailManager.SetDelivered():
    ///   if (delivered && mailId == TutorialManager.Instance?.tutorialMailId)
    ///       TutorialManager.Instance.OnTutorialLetterDelivered();
    /// </summary>
    public void OnTutorialLetterDelivered()
    {
        if (CurrentStep != TutorialStep.WaitForDelivery) return;
        Debug.Log("[Tutorial] Письмо доставлено — туториал завершён!");
        CompleteTutorial();
    }

    // ── Завершение ─────────────────────────────────────────

    private void CompleteTutorial()
    {
        tutorialCompleted = true;
        SetStep(TutorialStep.Completed);
        hintUI?.HideAll();
        spawnedNPC?.OnTutorialComplete();
        SaveProgress();
    }

    // ── Загрузка сохранения ────────────────────────────────

    /// <summary>Вызывается SaveGameManager.LoadFromJson() в конце загрузки.</summary>
    public void LoadTutorialState(TutorialSaveData saveData)
    {
        if (saveData == null) return;

        if (saveData.completedTutorialSteps.Contains("COMPLETED"))
        {
            tutorialCompleted = true;
            CurrentStep = TutorialStep.Completed;
            Debug.Log("[Tutorial] Туториал уже пройден");
            return;
        }

        if (saveData.currentStep <= 0) return;

        TutorialStep restored = (TutorialStep)saveData.currentStep;
        Debug.Log($"[Tutorial] Восстанавливаем стадию: {restored}");
        // Откладываем на кадр — сцена должна полностью загрузиться
        StartCoroutine(ResumeNextFrame(restored));
    }

    private IEnumerator ResumeNextFrame(TutorialStep step)
    {
        yield return null;
        ResumeFromStep(step);
    }

    private void ResumeFromStep(TutorialStep step)
    {
        CurrentStep = step;

        switch (step)
        {
            case TutorialStep.WaitForNPCSpawn:
                // Слайдшоу уже было — просто ждём триггера двери
                break;

            case TutorialStep.WaitForNPCApproach:
                SpawnAndApproach();
                break;

            case TutorialStep.WaitForInventoryOpen:
                SpawnAtPositionIfNeeded();
                spawnedNPC?.ShowDialogue(TutorialDialogueType.OpenInventory);
                hintUI?.ShowInventoryHint();
                break;

            case TutorialStep.WaitForLetterRead:
                SpawnAtPositionIfNeeded();
                hintUI?.ShowLetterHint();
                break;

            case TutorialStep.WaitForDelivery:
                SpawnAtPositionIfNeeded();
                spawnedNPC?.ShowDialogue(TutorialDialogueType.DeliverLetter);
                hintUI?.ShowDeliveryHint(tutorialRecipientNpcId);
                break;
        }
    }

    // ── Спавн NPC ──────────────────────────────────────────

    private void SpawnAndApproach()
    {
        TutorialNPC npc = SpawnNPC();
        npc?.ApproachPlayer();
    }

    private void SpawnAtPositionIfNeeded()
    {
        if (spawnedNPC != null) return;
        SpawnNPC(); // NPC появляется рядом, не идёт к игроку
    }

    private TutorialNPC SpawnNPC()
    {
        if (tutorialNPCPrefab == null)
        {
            Debug.LogError("[Tutorial] tutorialNPCPrefab не назначен в TutorialManager!");
            return null;
        }

        Vector3 pos = npcSpawnPoint != null
            ? npcSpawnPoint.position
            : GameManager.Instance.GetPlayer().transform.position + Vector3.forward * 4f;

        GameObject obj = Instantiate(tutorialNPCPrefab, pos, Quaternion.identity);
        spawnedNPC = obj.GetComponent<TutorialNPC>();

        if (spawnedNPC == null)
            Debug.LogError("[Tutorial] На префабе NPC нет компонента TutorialNPC!");

        return spawnedNPC;
    }

    // ── Утилиты ────────────────────────────────────────────

    private void SetStep(TutorialStep step)
    {
        Debug.Log($"[Tutorial] {CurrentStep} → {step}");
        CurrentStep = step;
    }

    private void SaveProgress()
    {
        SaveGameManager.Instance?.SaveAuto(false);
    }

    public bool IsTutorialCompleted() => tutorialCompleted;
    public bool IsTutorialActive() => !tutorialCompleted
                                      && CurrentStep != TutorialStep.None
                                      && CurrentStep != TutorialStep.Completed;

    public TutorialSaveData GetSaveData()
    {
        var data = new TutorialSaveData();
        data.currentStep = (int)CurrentStep;
        if (tutorialCompleted)
            data.completedTutorialSteps.Add("COMPLETED");
        return data;
    }
}