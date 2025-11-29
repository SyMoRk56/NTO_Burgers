using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public SettingsData settingsData;
    public MailSaveData mailData;

    // ДОБАВЛЕНО: данные инвентаря игрока
    public InventorySaveData inventoryData;

    public string saveDate;
    public long timestamp;
    public float playtime;
}