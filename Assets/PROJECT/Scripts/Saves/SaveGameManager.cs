using System.IO;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;

    private string saveFolder;
    private string autosavePath;
    public GameObject autosaveIndicator;

    void Awake()
    {
        Instance = this;

        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        autosavePath = Path.Combine(saveFolder, "autosave.json");

        Directory.CreateDirectory(saveFolder);
    }

    // ---------------- SAVE ----------------
    public void SaveAuto(bool showIndicator)
    {
        string json = CreateSaveJson();

        try
        {
            File.WriteAllText(autosavePath, json);
            if (showIndicator) ShowSaveIndicator();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Autosave failed: " + e.Message);
        }
    }

    public void SaveManual(string saveName)
    {
        string folder = Path.Combine(saveFolder, "manual");
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, saveName + ".json");

        string json = CreateSaveJson();
        File.WriteAllText(filePath, json);

        ScreenCapture.CaptureScreenshot(Path.Combine(folder, saveName + ".png"));

        ShowSaveIndicator();
        Debug.Log("Manual save created -> " + filePath);
    }

    // ---------------- LOAD ----------------
    public void LoadAuto()
    {
        if (!File.Exists(autosavePath))
        {
            Debug.LogWarning("No autosave found.");
            return;
        }

        LoadFromJson(File.ReadAllText(autosavePath));
    }

    public bool HasAutosave() => File.Exists(autosavePath);

    public bool HasManual(string name)
    {
        string folder = Path.Combine(saveFolder, "manual");
        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, name + ".json");
        return File.Exists(path);
    }

    public void LoadManual(string name)
    {
        string folder = Path.Combine(saveFolder, "manual");
        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, name + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found: " + path);
            return;
        }

        LoadFromJson(File.ReadAllText(path));
    }

    // ---------------- INTERNAL ----------------
    private string CreateSaveJson()
    {
        GameSaveData data = new GameSaveData();

        try
        {
            data.playerData = PlayerSaveSystem.Instance.GetData();
        }
        catch { data.playerData = null; }

        try
        {
            data.settingsData = SettingsSaveSystem.Instance.GetData();
        }
        catch { data.settingsData = null; }

        data.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();
        data.playtime = Time.time;

        return JsonUtility.ToJson(data, true);
    }

    private void LoadFromJson(string json)
    {
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        PlayerSaveSystem.Instance.LoadData(data.playerData);

        Debug.Log("Save loaded successfully");
    }

    private void ShowSaveIndicator()
    {
        if (autosaveIndicator == null) return;

        autosaveIndicator.SetActive(true);
        Invoke(nameof(DisableIndicator), 2.5f);
    }

    private void DisableIndicator()
    {
        if (autosaveIndicator != null)
            autosaveIndicator.SetActive(false);
    }
}
