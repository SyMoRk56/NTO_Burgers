using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public MailSaveData mailData;
    public InventorySaveData inventoryData;
    public DictionaryData objectStates; // Состояния объектов (скамейка, дерево)
    public string saveDate;
    public long timestamp;
    public float playtime;

    public List<NPCSaveData> npcData = new List<NPCSaveData>();
    public TutorialSaveData tutorialData;
}

// Класс для сохранения данных туториала
[System.Serializable]
public class TutorialSaveData
{
    public List<string> completedTutorialSteps;

    public TutorialSaveData()
    {
        completedTutorialSteps = new List<string>();
    }
}