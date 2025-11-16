using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class SettingsSaveManager : MonoBehaviour
{
    public static SettingsSaveManager Instance;

    private string settingsFolder;
    private string settingsPath;

    void Awake()
    {
        if (Instance == null) Instance = this;

        settingsFolder = Path.Combine(Application.persistentDataPath, "Settings");
        settingsPath = Path.Combine(settingsFolder, "settings.json");

        Directory.CreateDirectory(settingsFolder);
        LoadSettings();
    }

    // ----------- SAVE SETTINGS -----------
    public void SaveSettings()
    {
        SettingsData data = SettingsSaveSystem.Instance.GetData();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(settingsPath, json);
        print(json);
        Debug.Log("Settings saved -> " + settingsPath);
    }

    // ----------- LOAD SETTINGS -----------
    public void LoadSettings()
    {
        if (!File.Exists(settingsPath))
        {
            Debug.LogWarning("No settings file found, creating default.");
            SaveSettings();
            return;
        }

        string json = File.ReadAllText(settingsPath);
        SettingsData data = JsonUtility.FromJson<SettingsData>(json);

        SettingsSaveSystem.Instance.LoadData(data);
        Debug.Log("Settings loaded");
    }
}
