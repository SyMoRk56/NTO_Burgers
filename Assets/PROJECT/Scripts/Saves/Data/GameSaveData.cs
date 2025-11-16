using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public SettingsData settingsData;
    public string saveDate;      
    public long timestamp;       
    public float playtime;
}
