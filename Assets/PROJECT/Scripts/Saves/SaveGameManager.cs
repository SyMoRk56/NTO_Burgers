using System.IO;
using System.Security.Cryptography;
using System.Text;
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

    public bool CheckSave(string saveName)
    {
        string folder = Path.Combine(saveFolder, "manual");
        string jsonPath = Path.Combine(folder, saveName + ".json");
        string hashPath = Path.Combine(folder, saveName + ".hash");

        // Проверяем существование файлов
        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning($"Save file not found: {jsonPath}");
            return false;
        }

        if (!File.Exists(hashPath))
        {
            Debug.LogWarning($"Hash file not found: {hashPath}");
            return false;
        }

        try
        {
            // Читаем содержимое файлов
            string json = File.ReadAllText(jsonPath);
            string savedHash = File.ReadAllText(hashPath);

            // Вычисляем текущий хеш
            string actualHash = ComputeHash(json);

            // Сравниваем хеши
            bool isValid = savedHash == actualHash;

            if (isValid)
            {
                Debug.Log($"Save integrity check passed for: {saveName}");
            }
            else
            {
                Debug.LogError($"Save integrity check failed for: {saveName}");
                Debug.LogError($"Expected: {savedHash}");
                Debug.LogError($"Actual: {actualHash}");
            }

            return isValid;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during save integrity check for {saveName}: {ex.Message}");
            return false;
        }
    }
    public bool CheckAutoSave()
    {
        if (!File.Exists(autosavePath))
        {
            Debug.LogWarning("No autosave found to check.");
            return false;
        }

        try
        {
            AutoSaveSlot slot = JsonUtility.FromJson<AutoSaveSlot>(File.ReadAllText(autosavePath));

            if (string.IsNullOrEmpty(slot.slotName))
            {
                Debug.LogWarning("Autosave file empty or invalid.");
                return false;
            }

            // Проверяем соответствующее ручное сохранение
            return CheckSave(slot.slotName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during autosave check: {ex.Message}");
            return false;
        }
    }
    void Awake()
    {
        Instance = this;

        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        autosavePath = Path.Combine(saveFolder, "autosave.json");

        Directory.CreateDirectory(saveFolder);
        Directory.CreateDirectory(Path.Combine(saveFolder, "manual"));
    }

    // ======================= HASH =======================
    private string ComputeHash(string content)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            byte[] hash = sha.ComputeHash(bytes);
            return System.BitConverter.ToString(hash).Replace("-", "");
        }
    }

    // ======================= AUTOSAVE =======================
    public void SaveAuto(bool showIndicator)
    {
        string slot = GameManager.Instance.currentManualSlot;

        if (string.IsNullOrEmpty(slot))
        {
            Debug.LogWarning("No manual slot selected, autosave skipped!");
            return;
        }

        SaveManual(slot, showIndicator: false);

        AutoSaveSlot data = new AutoSaveSlot() { slotName = slot };
        File.WriteAllText(autosavePath, JsonUtility.ToJson(data, true));

        if (showIndicator) ShowSaveIndicator();

        Debug.Log("Autosave saved slot name: " + slot);
    }

    // ======================= MANUAL SAVE =======================
    public void SaveManual(string saveName, bool showIndicator = true)
    {
        string folder = Path.Combine(saveFolder, "manual");
        Directory.CreateDirectory(folder);

        string jsonPath = Path.Combine(folder, saveName + ".json");
        string hashPath = Path.Combine(folder, saveName + ".hash");

        string json = CreateSaveJson();

        File.WriteAllText(jsonPath, json);

        // ---- Create hash file ----
        string hash = ComputeHash(json);
        File.WriteAllText(hashPath, hash);

        // Screenshot
        ScreenCapture.CaptureScreenshot(Path.Combine(folder, saveName + ".png"));

        if (showIndicator) ShowSaveIndicator();
        Debug.Log("Manual save created -> " + jsonPath + " (HASH updated)");
    }

    // ======================= LOAD =======================
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
        string folder = Path.Combine(saveFolder, "manual");
        string jsonPath = Path.Combine(folder, name + ".json");
        string hashPath = Path.Combine(folder, name + ".hash");

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("Save file not found: " + jsonPath);
            return;
        }

        if (!File.Exists(hashPath))
        {
            Debug.LogError("Hash file missing! Save may be modified externally!");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        string savedHash = File.ReadAllText(hashPath);
        string actualHash = ComputeHash(json);

        // --- Validate hash ---
        if (savedHash != actualHash)
        {
            Debug.LogError("SAVE INTEGRITY ERROR! External modification detected! Slot: " + name);
            return;
        }

        Debug.Log("Save integrity OK.");

        GameManager.Instance.currentManualSlot = name;

        LoadFromJson(json);
    }

    // ======================= INTERNAL =======================
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
        GameObject player = GameManager.Instance.GetPlayer();

        if (player != null)
        {
            player.GetComponent<Rigidbody>()
                  .MovePosition(new Vector3(floatArray[0], floatArray[1], floatArray[2]));
        }
    }

    // ======================= UI =======================
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
