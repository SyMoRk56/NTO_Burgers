using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        DontDestroyOnLoad(npcSpawnPoint.gameObject);
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void StartTutorialForNewSlot()
    {
        print("StartTutorialForNewSlot");
        if (tutorialCompleted) return;
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
        SetStep(TutorialStep.WaitForNPCApproach);
        SpawnAndApproach();
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
        spawnedNPC?.ShowDialogue(TutorialDialogueType.DeliverLetter);
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
        spawnedNPC?.OnTutorialComplete();
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
                // Просто ждём выхода из дома
                break;

            case TutorialStep.WaitForNPCApproach:
                SpawnAndApproach();
                break;

            case TutorialStep.WaitForInventoryOpen:
                // NPC уже говорил — просто спавним рядом и показываем подсказку
                SpawnAtPositionIfNeeded();
                hintUI?.ShowInventoryHint();
                break;

            case TutorialStep.WaitForLetterRead:
                SpawnAtPositionIfNeeded();
                hintUI?.ShowLetterHint();
                break;

            case TutorialStep.WaitForDelivery:
                // NPC уже говорил — просто спавним рядом и показываем подсказку
                SpawnAtPositionIfNeeded();
                hintUI?.ShowDeliveryHint(tutorialRecipientNpcId);
                break;
        }
    }

    private void SpawnAndApproach()
    {
        Debug.LogError("Spawn and approach");
        TutorialNPC npc = SpawnNPC();
        npc?.ApproachPlayer();
    }

    private void SpawnAtPositionIfNeeded()
    {
        if (spawnedNPC != null) return;
        SpawnNPC();
    }

    private TutorialNPC SpawnNPC()
    {
        if (tutorialNPCPrefab == null)
        {
            Debug.LogError("[Tutorial] tutorialNPCPrefab не назначен в TutorialManager!");
            return null;
        }

        Vector3 pos = npcSpawnPoint.position;

        GameObject obj = Instantiate(tutorialNPCPrefab, pos, Quaternion.identity);
        obj.SetActive(true);
        spawnedNPC = obj.GetComponent<TutorialNPC>();

        if (spawnedNPC == null)
            Debug.LogError("[Tutorial] На префабе NPC нет компонента TutorialNPC!");

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