using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class AutoSaveSlot
{
    public string slotName;
}

[System.Serializable]
public class DictionaryData
{
    public List<string> keys = new List<string>();
    public List<bool> values = new List<bool>();

    public DictionaryData() { }

    public DictionaryData(Dictionary<string, bool> dict)
    {
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<string, bool> ToDictionary()
    {
        Dictionary<string, bool> dict = new Dictionary<string, bool>();
        for (int i = 0; i < keys.Count; i++)
        {
            if (i < values.Count)
            {
                dict[keys[i]] = values[i];
            }
        }
        return dict;
    }
}

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;

    private string saveFolder;
    private string autosavePath;
    public GameObject autosaveIndicator;

    // Флаги для контроля показа индикатора
    private bool isFirstSaveInGame = true;
    private string currentSceneName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        autosavePath = Path.Combine(saveFolder, "autosave.json");

        Directory.CreateDirectory(saveFolder);
        Directory.CreateDirectory(Path.Combine(saveFolder, "manual"));

        // Получаем текущую сцену
        currentSceneName = SceneManager.GetActiveScene().name;

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"SaveManager initialized on scene: {currentSceneName}");

        // Сразу отключаем индикатор
        if (autosaveIndicator != null)
            autosaveIndicator.SetActive(false);
    }

    void Start()
    {
        // Дополнительно отключаем индикатор при старте
        if (autosaveIndicator != null)
            autosaveIndicator.SetActive(false);
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log($"Scene loaded: {currentSceneName}");

        // При загрузке меню очищаем ObjectStateManager
        if (currentSceneName == "Menu" && ObjectStateManager.Instance != null)
        {
            ObjectStateManager.Instance.Clear();
            Debug.Log("ObjectStateManager очищен (загрузка меню)");
        }

        // При загрузке Game сцены проверяем, нужно ли очищать
        if (currentSceneName == "Game")
        {
            isFirstSaveInGame = true;
            Debug.Log("First save in Game scene will be hidden");
        }

        // Всегда отключаем индикатор при смене сцены
        if (autosaveIndicator != null)
            autosaveIndicator.SetActive(false);
    }

    public bool CheckSave(string saveName)
    {
        string folder = Path.Combine(saveFolder, "manual");
        string jsonPath = Path.Combine(folder, saveName + ".json");
        string hashPath = Path.Combine(folder, saveName + ".hash");

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
            string json = File.ReadAllText(jsonPath);
            string savedHash = File.ReadAllText(hashPath);
            string actualHash = ComputeHash(json);

            bool isValid = savedHash == actualHash;

            if (isValid)
            {
                Debug.Log($"Save integrity check passed for: {saveName}");
            }
            else
            {
                Debug.LogError($"Save integrity check failed for: {saveName}");
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

            return CheckSave(slot.slotName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during autosave check: {ex.Message}");
            return false;
        }
    }

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

        // НИКОГДА не показываем на сцене Menu
        if (currentSceneName == "Menu")
        {
            Debug.Log("Autosave on Menu scene - indicator hidden");
            return;
        }

        // На сцене Game показываем только если НЕ первое сохранение
        if (currentSceneName == "Game")
        {
            if (isFirstSaveInGame)
            {
                Debug.Log("First save in Game scene - indicator hidden");
                isFirstSaveInGame = false;
            }
            else if (showIndicator)
            {
                ShowSaveIndicator();
            }
        }

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

        string hash = ComputeHash(json);
        File.WriteAllText(hashPath, hash);

        ScreenCapture.CaptureScreenshot(Path.Combine(folder, saveName + ".png"));

        // НИКОГДА не показываем на сцене Menu
        if (currentSceneName == "Menu")
        {
            Debug.Log("Manual save on Menu scene - indicator hidden");
            return;
        }

        // На сцене Game показываем только если НЕ первое сохранение
        if (currentSceneName == "Game")
        {
            if (isFirstSaveInGame)
            {
                Debug.Log("First manual save in Game scene - indicator hidden");
                isFirstSaveInGame = false;
            }
            else if (showIndicator)
            {
                ShowSaveIndicator();
            }
        }
        Debug.Log("Manual save created -> " + jsonPath);
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

        try
        {
            data.playerData = PlayerSaveSystem.Instance.GetData();
        }
        catch
        {
            data.playerData = null;
        }

        try
        {
            data.mailData = MailManager.Instance.GetSaveData();
        }
        catch
        {
            data.mailData = null;
        }

        try
        {
            data.npcData = NPCSaveSystem.CollectNPCData();
        }
        catch
        {
            data.npcData = new List<NPCSaveData>();
        }

        try
        {
            data.inventoryData = PlayerMailInventory.Instance.GetSaveData();
        }
        catch
        {
            data.inventoryData = null;
        }

        // Сохраняем состояния объектов (скамейка, дерево)
        try
        {
            if (ObjectStateManager.Instance != null)
            {
                data.objectStates = new DictionaryData(ObjectStateManager.Instance.GetAllStates());
            }
        }
        catch
        {
            data.objectStates = new DictionaryData();
        }
        try
        {
            DayNightCycle cycle = FindObjectOfType<DayNightCycle>();
            if (cycle != null)
            {
                data.timeOfDayIndex = cycle.GetTimeIndex();
            }
        }
        catch
        {
            data.timeOfDayIndex = 0; // безопасный дефолт
        }


        data.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.timestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds();
        data.playtime = Time.time;

        return JsonUtility.ToJson(data, true);
    }

    private void LoadFromJson(string json)
    {
        Debug.Log("Loading from JSON");
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        // Сначала загружаем данные игрока
        PlayerSaveSystem.Instance.LoadData(data.playerData);

        // ЗАГРУЖАЕМ СОСТОЯНИЯ ОБЪЕКТОВ ДО ВСЕГО ОСТАЛЬНОГО
        if (ObjectStateManager.Instance != null && data.objectStates != null)
        {
            Debug.Log("Загружаем состояния объектов из сохранения...");
            ObjectStateManager.Instance.LoadStates(data.objectStates.ToDictionary());
            ObjectStateManager.Instance.DebugStates();
        }

        // ВАЖНО: Проверяем наличие сумки у игрока
        if (data.playerData != null && !data.playerData.hasBag)
        {
            Debug.LogWarning("У игрока нет сумки - очищаем все задания и письма!");

            // Очищаем ВСЕ письма
            if (MailManager.Instance != null && MailManager.Instance.catalog != null)
            {
                foreach (var mail in MailManager.Instance.catalog.mails)
                {
                    MailManager.Instance.SetDelivered(mail.id, false);
                }
                Debug.Log("Все письма сброшены (hasBag = false)");
            }

            // Очищаем инвентарь писем
            if (PlayerMailInventory.Instance != null)
            {
                PlayerMailInventory.Instance.ClearInventory();
                Debug.Log("Инвентарь писем очищен (hasBag = false)");
            }
        }
        else
        {
            // Если у игрока есть сумка - загружаем задания нормально
            Debug.Log("У игрока есть сумка - загружаем задания");

            if (data.mailData != null)
                MailManager.Instance.LoadSaveData(data.mailData);

            if (data.npcData != null)
                NPCSaveSystem.RestoreNPCData(data.npcData);

            if (data.inventoryData != null)
                PlayerMailInventory.Instance.LoadSaveData(data.inventoryData);
        }

        Debug.Log("Save loaded successfully");

        // Перемещаем игрока на сохраненную позицию
        var floatArray = PlayerSaveSystem.Instance.GetData().position;
        GameObject player = GameManager.Instance.GetPlayer();

        if (player != null && floatArray != null && floatArray.Length == 3)
        {
            player.GetComponent<Rigidbody>()
                  .MovePosition(new Vector3(floatArray[0], floatArray[1], floatArray[2]));
        }
        //  ЗАГРУЗКА ВРЕМЕНИ СУТОК
        DayNightCycle cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
        {
            cycle.SetTimeIndex(data.timeOfDayIndex);
            Debug.Log($"Загружено время суток: {data.timeOfDayIndex}");
        }
        else
        {
            Debug.LogWarning("DayNightCycle не найден при загрузке!");
        }

    }

    // ======================= UI =======================
    private void ShowSaveIndicator()
    {
        if (autosaveIndicator == null)
        {
            Debug.LogWarning("Autosave indicator is not assigned!");
            return;
        }

        Debug.Log("Showing save indicator");
        autosaveIndicator.SetActive(true);
        Invoke(nameof(DisableIndicator), 2.5f);
    }

    private void DisableIndicator()
    {
        if (autosaveIndicator != null)
        {
            autosaveIndicator.SetActive(false);
            Debug.Log("Save indicator hidden");
        }
    }

    // ======================= DELETE SAVE =======================
    public void DeleteSave(string saveName)
    {
        string folder = Path.Combine(saveFolder, "manual");
        string jsonPath = Path.Combine(folder, saveName + ".json");
        string hashPath = Path.Combine(folder, saveName + ".hash");
        string screenshotPath = Path.Combine(folder, saveName + ".png");

        try
        {
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
                Debug.Log($"Deleted save file: {jsonPath}");
            }

            if (File.Exists(hashPath))
            {
                File.Delete(hashPath);
                Debug.Log($"Deleted hash file: {hashPath}");
            }

            if (File.Exists(screenshotPath))
            {
                File.Delete(screenshotPath);
                Debug.Log($"Deleted screenshot: {screenshotPath}");
            }

            if (File.Exists(autosavePath))
            {
                AutoSaveSlot autoSave = JsonUtility.FromJson<AutoSaveSlot>(File.ReadAllText(autosavePath));
                if (autoSave != null && autoSave.slotName == saveName)
                {
                    File.Delete(autosavePath);
                    Debug.Log("Autosave deleted because its manual slot was removed");
                }
            }

            Debug.Log($"Save '{saveName}' successfully deleted");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error deleting save '{saveName}': {ex.Message}");
        }
    }
}