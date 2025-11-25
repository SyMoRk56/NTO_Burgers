using System.IO;
using UnityEngine;

[System.Serializable]
public class AutoSaveSlot
{
    public string slotName;
}

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
        Directory.CreateDirectory(Path.Combine(saveFolder, "manual"));
    }

    // ------------------------ AUTOSAVE ------------------------
    public void SaveAuto(bool showIndicator)
    {
        string slot = GameManager.Instance.currentManualSlot;

        // Если нет активного слота — не автосейвим
        if (string.IsNullOrEmpty(slot))
        {
            Debug.LogWarning("No manual slot selected, autosave skipped!");
            return;
        }

        // 1. Сохраняем настоящее содержимое в мануальный слот
        SaveManual(slot, showIndicator: false);

        // 2. В autosave.json пишем только имя слота
        AutoSaveSlot data = new AutoSaveSlot() { slotName = slot };
        File.WriteAllText(autosavePath, JsonUtility.ToJson(data, true));

        if (showIndicator) ShowSaveIndicator();

        Debug.Log("Autosave saved slot name: " + slot);
    }

    // ------------------------ MANUAL SAVE ------------------------
    public void SaveManual(string saveName, bool showIndicator = true)
    {
        string folder = Path.Combine(saveFolder, "manual");
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, saveName + ".json");
        string json = CreateSaveJson();

        File.WriteAllText(filePath, json);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder, saveName + ".png"));

        if (showIndicator) ShowSaveIndicator();
        Debug.Log("Manual save created -> " + filePath);
    }

    // ------------------------ LOAD ------------------------
    public void LoadAuto()
    {
        if (!File.Exists(autosavePath))
        {
            Debug.LogWarning("No autosave found.");
            return;
        }

        AutoSaveSlot slot = JsonUtility.FromJson<AutoSaveSlot>(File.ReadAllText(autosavePath));

        if (string.IsNullOrEmpty(slot.slotName))
        {
            Debug.LogWarning("Autosave file empty or invalid.");
            return;
        }

        LoadManual(slot.slotName);
    }

    public bool HasAutosave() => File.Exists(autosavePath);

    public bool HasManual(string name)
    {
        string path = Path.Combine(saveFolder, "manual", name + ".json");
        return File.Exists(path);
    }

    public void LoadManual(string name)
    {
        string path = Path.Combine(saveFolder, "manual", name + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found: " + path);
            return;
        }

        // фиксируем активный слот
        GameManager.Instance.currentManualSlot = name;

        LoadFromJson(File.ReadAllText(path));
    }

    // ---------------- INTERNAL ----------------
    private string CreateSaveJson()
    {
        GameSaveData data = new GameSaveData();

        try { data.playerData = PlayerSaveSystem.Instance.GetData(); }
        catch { data.playerData = null; }

        try { data.settingsData = SettingsSaveSystem.Instance.GetData(); }
        catch { data.settingsData = null; }

        try { data.mailData = MailManager.Instance.GetSaveData(); }
        catch { data.mailData = null; }


        data.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();
        data.playtime = Time.time;

        return JsonUtility.ToJson(data, true);
    }

    private void LoadFromJson(string json)
    {
        Debug.LogWarning("LOAD FROM JSON " + json);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        PlayerSaveSystem.Instance.LoadData(data.playerData);
        if (data.mailData != null)
            MailManager.Instance.LoadSaveData(data.mailData);

        Debug.Log("Save loaded successfully");
        var floatArray = PlayerSaveSystem.Instance.GetData().position;
        GameManager.Instance.GetPlayer().GetComponent<Rigidbody>().MovePosition(new Vector3(floatArray[0], floatArray[1], floatArray[2]));
        print(GameManager.Instance.GetPlayer().transform.position);
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
