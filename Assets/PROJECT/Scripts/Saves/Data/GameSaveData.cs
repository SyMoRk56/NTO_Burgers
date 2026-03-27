using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public MailSaveData mailData;
    public InventorySaveData inventoryData;
    public DictionaryData objectStates;
    public string saveDate;
    public long timestamp;
    public float playtime;
    public List<NPCSaveData> npcData = new List<NPCSaveData>();
    public TutorialSaveData tutorialData;
    public int timeOfDayIndex;
    public float timeOfDayValue;  // ✅ ДОБАВЛЕНО
}

[System.Serializable]
public class TutorialSaveData
{
    public int currentStep;
    public List<string> completedTutorialSteps;
    public bool completed;  // ✅ ДОБАВЛЕНО

    public TutorialSaveData()
    {
        currentStep = 0;
        completedTutorialSteps = new List<string>();
        completed = false;
    }
}