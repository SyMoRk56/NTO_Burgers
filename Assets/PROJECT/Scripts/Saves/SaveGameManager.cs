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
    public bool HasAutosave()
    {
        return File.Exists(autosavePath);
    }
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

    // ------------ INTERNAL LOGIC ------------
    private string CreateSaveJson()
    {
        GameSaveData data = new GameSaveData();

        try
        {
            data.playerData = PlayerSaveSystem.Instance.GetData();
        }
        catch
        {
            data.playerData = null;
        }



        return JsonUtility.ToJson(data, true);
    }

    private void LoadFromJson(string json)
    {
        Debug.Log(json);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        PlayerSaveSystem.Instance.LoadData(data.playerData);

        Debug.Log("Game save loaded successfully.");
    }

    private void SaveScreenshot(string filePath)
    {
        ScreenCapture.CaptureScreenshot(filePath);
    }

    private void ShowSaveIndicator()
    {
        
    }
}
