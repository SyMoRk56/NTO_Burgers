using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public TutorialStep CurrentStep { get; private set; } = TutorialStep.None;

    private bool tutorialCompleted = false;
    private TutorialNPC spawnedNPC;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ✅ Ловим загрузку сцен
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "menu") // ⚠️ Укажи точное имя сцены
        {
            ResetTutorial();
        }
    }

    // ✅ Полный сброс
    private void ResetTutorial()
    {
        Debug.Log("[Tutorial] Сброс туториала (загрузка Menu)");

        tutorialCompleted = false;
        CurrentStep = TutorialStep.WaitForNPCSpawn;

        if (spawnedNPC != null)
        {
            Destroy(spawnedNPC.gameObject);
            spawnedNPC = null;
        }

        hintUI?.HideAll();
    }

    public void StartTutorialForNewSlot()
    {
        tutorialCompleted = false;

        Debug.Log("[Tutorial] Новый слот — запускаем слайдшоу");

        SetStep(TutorialStep.WaitForNPCSpawn);

        if (TutorialSlideshowUI.Instance != null)
            TutorialSlideshowUI.Instance.ShowSlideshow();
        else
            Debug.LogWarning("[Tutorial] TutorialSlideshowUI не найден!");
    }

    public void OnSlideshowFinished()
    {
        Debug.Log("[Tutorial] Слайдшоу завершено — ждём выхода из дома");
    }

    public void OnPlayerExitedHouse()
    {
        if (CurrentStep != TutorialStep.WaitForNPCSpawn) return;

        Debug.Log("[Tutorial] Игрок вышел из дома — спавним NPC");

        var npc = FindFirstObjectByType<TutorialNPC>();
        npc?.OnPlayerExitedHouse();

        SetStep(TutorialStep.WaitForNPCApproach);
        SaveProgress();
    }

    public void OnNPCReachedPlayer()
    {
        if (CurrentStep != TutorialStep.WaitForNPCApproach) return;

        SetStep(TutorialStep.WaitForInventoryOpen);
        hintUI?.ShowInventoryHint();

        SaveProgress();
    }

    public void OnInventoryOpened()
    {
        if (CurrentStep != TutorialStep.WaitForInventoryOpen) return;

        Debug.Log("[Tutorial] Инвентарь открыт");

        SetStep(TutorialStep.WaitForLetterRead);
        hintUI?.ShowLetterHint();

        SaveProgress();
    }

    public void OnTutorialLetterRead()
    {
        if (CurrentStep != TutorialStep.WaitForLetterRead) return;

        Debug.Log("[Tutorial] Письмо прочитано");

        SetStep(TutorialStep.WaitForDelivery);
        hintUI?.ShowDeliveryHint(tutorialRecipientNpcId);

        SaveProgress();
    }

    public void OnTutorialLetterDelivered()
    {
        if (CurrentStep != TutorialStep.WaitForDelivery) return;

        Debug.Log("[Tutorial] Письмо доставлено — туториал завершён!");
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        tutorialCompleted = true;

        SetStep(TutorialStep.Completed);
        hintUI?.HideAll();

        SaveProgress();
    }

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
                break;

            case TutorialStep.WaitForNPCApproach:
                break;

            case TutorialStep.WaitForInventoryOpen:
                break;

            case TutorialStep.WaitForLetterRead:
                break;

            case TutorialStep.WaitForDelivery:
                break;
        }
    }

    private TutorialNPC SpawnNPC()
    {
        if (tutorialNPCPrefab == null)
        {
            Debug.LogError("[Tutorial] tutorialNPCPrefab не назначен!");
            return null;
        }

        if (npcSpawnPoint == null)
        {
            Debug.LogError("[Tutorial] npcSpawnPoint не назначен!");
            return null;
        }

        GameObject obj = Instantiate(tutorialNPCPrefab, npcSpawnPoint.position, Quaternion.identity);
        spawnedNPC = obj.GetComponent<TutorialNPC>();

        if (spawnedNPC == null)
            Debug.LogError("[Tutorial] На NPC нет TutorialNPC!");

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