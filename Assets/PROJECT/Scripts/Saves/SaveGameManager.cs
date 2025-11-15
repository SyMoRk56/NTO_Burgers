using System.Collections;
using System.IO;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;

    private string saveFolder;
    private string autosavePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        autosavePath = Path.Combine(saveFolder, "autosave.json");

        Directory.CreateDirectory(saveFolder);
    }

    // ------------ SAVE ------------
    public void SaveAuto()
    {
        string json = CreateSaveJson();
        File.WriteAllText(autosavePath, json);
        ShowSaveIndicator();
        Debug.Log("Autosave saved -> " + autosavePath);
    }

    public void SaveManual(string saveName)
    {
        string folder = Path.Combine(saveFolder, "manual");
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, saveName + ".json");

        string json = CreateSaveJson();
        File.WriteAllText(filePath, json);

        SaveScreenshot(Path.Combine(folder, saveName + ".png"));

        ShowSaveIndicator();
        Debug.Log("Manual save created -> " + filePath);
    }

    // ------------ LOAD ------------
    public void LoadAuto()
    {
        if (!File.Exists(autosavePath))
        {
            Debug.LogError("No autosave found.");
            return;
        }

        LoadFromJson(File.ReadAllText(autosavePath));
    }

    public void LoadManual(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found: " + path);
            return;
        }

        LoadFromJson(File.ReadAllText(path));
    }

    private string CreateSaveJson()
    {
        GameSaveData data = new GameSaveData();

        // сбор данных от разных систем
        data.playerData = PlayerSaveSystem.Instance.GetData();
        data.settingsData = SettingsSaveSystem.Instance.GetData();

        return JsonUtility.ToJson(data, true);
    }

    private void LoadFromJson(string json)
    {
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        PlayerSaveSystem.Instance.LoadData(data.playerData);
        SettingsSaveSystem.Instance.LoadData(data.settingsData);

        Debug.Log("Save loaded successfully.");
    }

    private void SaveScreenshot(string filePath)
    {
        ScreenCapture.CaptureScreenshot(filePath);
    }

    // ------------ FEEDBACK ------------
    private void ShowSaveIndicator()
    {
        
    }
    IEnumerator Start()
    {
        while (true)
        {

        }
    }
}
