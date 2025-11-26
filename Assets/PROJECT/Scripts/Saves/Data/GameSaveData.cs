[System.Serializable]
public class GameSaveData
{
    public PlayerData playerData;
    public SettingsData settingsData;

    public MailSaveData mailData; 

    public string saveDate;
    public long timestamp;
    public float playtime;
}
