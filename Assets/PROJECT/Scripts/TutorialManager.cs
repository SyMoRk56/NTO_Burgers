using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Все шаги туториала
/// </summary>
public enum TutorialStep
{
    None = 0,
    WaitForNPCSpawn = 1,
    WaitForNPCApproach = 2,
    WaitForInventoryOpen = 3,
    WaitForLetterRead = 4,
    WaitForDelivery = 5,
    Completed = 99
}

/// <summary>
/// Менеджер туториала
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Туториальный NPC")]
    public GameObject tutorialNPCPrefab;
    public Transform npcSpawnPoint;

    [Header("Hint UI")]
    public TutorialHintUI hintUI;

    [Header("Настройки письма")]
    public string tutorialMailId = "tutorial_letter_01";
    public string tutorialRecipientNpcId = "npc_grandma";

    [Header("Диалог")]
    public DialogueRunner dialogueRunner; // Runner для запуска диалогов

    public TutorialStep CurrentStep { get; private set; } = TutorialStep.None;

    private bool tutorialCompleted = false;
    private TutorialNPC spawnedNPC;

    private void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Сбрасываем туториал при загрузке главного меню
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "menu")
        {
            ResetTutorial();
        }
    }

    /// <summary>
    /// Полный сброс прогресса
    /// </summary>
    private void ResetTutorial()
    {
        Debug.Log("[Tutorial] Сброс туториала");

        tutorialCompleted = false;
        CurrentStep = TutorialStep.WaitForNPCSpawn;

        if (spawnedNPC != null)
        {
            Destroy(spawnedNPC.gameObject);
            spawnedNPC = null;
        }

        hintUI?.HideAll();
    }

    /// <summary>
    /// Запуск туториала для нового слота
    /// </summary>
    public void StartTutorialForNewSlot()
    {
        tutorialCompleted = false;

        SetStep(TutorialStep.WaitForNPCSpawn);

        // Показываем слайдшоу туториала
        if (TutorialSlideshowUI.Instance != null)
            TutorialSlideshowUI.Instance.ShowSlideshow();
        else
            Debug.LogWarning("[Tutorial] TutorialSlideshowUI не найден!");
    }

    public void OnSlideshowFinished()
    {
        Debug.Log("[Tutorial] Слайдшоу завершено");
    }

    /// <summary>
    /// Игрок вышел из дома — спавним NPC
    /// </summary>
    public void OnPlayerExitedHouse()
    {
        if (CurrentStep != TutorialStep.WaitForNPCSpawn) return;

        Debug.Log("[Tutorial] Игрок вышел из дома — спавним NPC");

        spawnedNPC = SpawnNPC();
        spawnedNPC?.OnPlayerExitedHouse();

        SetStep(TutorialStep.WaitForNPCApproach);
        SaveProgress();
    }

    /// <summary>
    /// NPC подошёл к игроку — показываем подсказку об инвентаре
    /// </summary>
    public void OnNPCReachedPlayer()
    {
        if (CurrentStep != TutorialStep.WaitForNPCApproach) return;

        SetStep(TutorialStep.WaitForInventoryOpen);
        hintUI?.ShowInventoryHint();

        SaveProgress();
    }

    /// <summary>
    /// Игрок открыл инвентарь
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
    /// Игрок прочитал письмо — запускаем диалог
    /// </summary>
    public void OnTutorialLetterRead()
    {
        if (CurrentStep != TutorialStep.WaitForLetterRead) return;

        Debug.Log("[Tutorial] Письмо прочитано");

        SetStep(TutorialStep.WaitForDelivery);
        hintUI?.ShowDeliveryHint(tutorialRecipientNpcId);

        // Запуск диалога с письмом
        if (dialogueRunner != null)
            StartCoroutine(StartDialogueWithDelay(true));

        SaveProgress();
    }

    /// <summary>
    /// Игрок доставил письмо — завершение туториала
    /// </summary>
    public void OnTutorialLetterDelivered()
    {
        if (CurrentStep != TutorialStep.WaitForDelivery) return;

        Debug.Log("[Tutorial] Письмо доставлено — туториал завершён!");
        CompleteTutorial();

        // Запуск финального диалога
        if (dialogueRunner != null)
            StartCoroutine(StartDialogueWithDelay(false));
    }

    /// <summary>
    /// Отложенный запуск диалога
    /// </summary>
    private IEnumerator StartDialogueWithDelay(bool isLetterDialogue)
    {
        yield return new WaitForSeconds(0.3f);

        dialogueRunner?.StartDialogue(isLetterDialogue);
    }

    private void CompleteTutorial()
    {
        tutorialCompleted = true;

        SetStep(TutorialStep.Completed);
        hintUI?.HideAll();

        SaveProgress();
    }

    /// <summary>
    /// Загружаем прогресс туториала
    /// </summary>
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
            case TutorialStep.WaitForNPCApproach:
            case TutorialStep.WaitForInventoryOpen:
            case TutorialStep.WaitForLetterRead:
            case TutorialStep.WaitForDelivery:
                break;
        }
    }

    /// <summary>
    /// Спавн NPC туториала
    /// </summary>
    private TutorialNPC SpawnNPC()
    {
        if (tutorialNPCPrefab == null || npcSpawnPoint == null) return null;

        GameObject obj = Instantiate(tutorialNPCPrefab, npcSpawnPoint.position, Quaternion.identity);
        spawnedNPC = obj.GetComponent<TutorialNPC>();
        return spawnedNPC;
    }

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

    public bool IsTutorialActive() =>
        !tutorialCompleted &&
        CurrentStep != TutorialStep.None &&
        CurrentStep != TutorialStep.Completed;

    public TutorialSaveData GetSaveData()
    {
        var data = new TutorialSaveData();
        data.currentStep = (int)CurrentStep;

        if (tutorialCompleted)
            data.completedTutorialSteps.Add("COMPLETED");

        return data;
    }
}