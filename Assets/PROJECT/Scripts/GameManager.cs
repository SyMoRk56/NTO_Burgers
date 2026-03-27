using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GameObject player;

    public bool isGameGoing = false;
    private Coroutine autosaveRoutine;

    public string pendingManualLoad = null;
    public string currentManualSlot = null;
    public bool loadAutoOnStart = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SettingsSaveManager.Instance.LoadSettings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
            StartCoroutine(DelayedGameStart());
    }

    IEnumerator DelayedGameStart()
    {
        print("GAME MANAGER 1");
        yield return null;
        while (GetPlayer() == null)
            yield return null;
        print("GAME MANAGER 2");

        while (PlayerSaveSystem.Instance == null)
            yield return null;
        print("GAME MANAGER 3");

        while (PlayerSaveSystem.Instance.GetData().position[1] == 0)
            yield return null;
        print("GAME MANAGER 4");

        yield return new WaitForEndOfFrame();
        yield return null;

        OnStartGame();
    }

    // В методе OnStartGame() ИЗМЕНИТЬ ПОРЯДОК:

    public void OnStartGame()
    {
        print("ON START GAME");

        FindFirstObjectByType<EnterToHouse>()?.DipFromBlack();

        isGameGoing = true;
        player = GetPlayer();

        // СНАЧАЛА загружаем сохранение
        if (pendingManualLoad != null)
        {
            SaveGameManager.Instance.LoadManual(pendingManualLoad);
            currentManualSlot = pendingManualLoad;
            pendingManualLoad = null;
        }
        else if (loadAutoOnStart)
        {
            SaveGameManager.Instance.LoadAuto();
            currentManualSlot = null;
        }

        // ❌ УБРАНО: GiveTutorialMailsAtStart() и GiveDailyMails() больше не нужны здесь

        if (autosaveRoutine != null)
            StopCoroutine(autosaveRoutine);

        autosaveRoutine = StartCoroutine(Autosave());
        FindFirstObjectByType<CheckForInHouse>().OnStartGame();
        Time.timeScale = 1;

        // ✅ Вызываем AddTutorial через 1 секунду
        Invoke(nameof(AddTutorial), 1f);
    }

    // ✅ ЭТОТ МЕТОД ОБЯЗАТЕЛЬНО ДОБАВЛЯЕТ 4 ТУТОРИАЛЬНЫХ ПИСЬМА В ИНВЕНТАРЬ
    void AddTutorial()
    {
        // Проверяем что инвентарь пуст (новый слот или после загрузки без писем)
        if (PlayerMailInventory.Instance == null) return;

        var saveData = PlayerMailInventory.Instance.GetSaveData();
        if (saveData.carriedMails.Count > 0)
        {
            Debug.Log("[AddTutorial] Инвентарь не пуст, пропускаем выдачу туториала");
            return;
        }

        Debug.Log("[AddTutorial] Выдаём 4 туториальных письма в инвентарь");

        // ✅ Первые 4 письма туториала — напрямую в инвентарь
        string[] tutorialIds = { "Tutorial_0", "Tutorial_1", "Tutorial_2", "Tutorial_3" };

        foreach (var id in tutorialIds)
        {
            Task mail = MailManager.Instance.GetMailByID(id);
            if (!string.IsNullOrEmpty(mail.id))
            {
                PlayerMailInventory.Instance.AddMailToInventory(mail);
                Debug.Log($"[AddTutorial] Добавлено письмо: {id}");
            }
            else
            {
                Debug.LogError($"[AddTutorial] Письмо с ID {id} не найдено в MailManager.catalog!");
            }
        }

        Debug.Log($"[AddTutorial] Готово! Всего писем в инвентаре: {PlayerMailInventory.Instance.carriedMails.Count}");
    }   

    public GameObject GetPlayer()
    {
        if (player != null) return player;
        player = GameObject.FindWithTag("Player");
        return player;
    }

    private IEnumerator Autosave()
    {
        while (isGameGoing)
        {
            yield return new WaitForSeconds(30f);
            SaveGameManager.Instance.SaveAuto(true);
        }
    }

    public void ExitToMenu()
    {
        SaveAndGoToMenu();
    }

    public void SaveAndGoToMenu()
    {
        string slot = currentManualSlot;

        if (string.IsNullOrEmpty(slot))
        {
            Debug.LogWarning("Cannot exit to menu — no manual slot selected!");
            return;
        }

        SaveGameManager.Instance.SaveManual(slot, false);

        AutoSaveSlot auto = new AutoSaveSlot { slotName = slot };
        string autosavePath = Path.Combine(
            Application.persistentDataPath,
            "Saves",
            "autosave.json"
        );
        File.WriteAllText(autosavePath, JsonUtility.ToJson(auto, true));

        Debug.Log("Saved slot + autosave. Returning to menu...");

        isGameGoing = false;
        if (autosaveRoutine != null)
        {
            StopCoroutine(autosaveRoutine);
            autosaveRoutine = null;
        }

        pendingManualLoad = null;
        loadAutoOnStart = true;

        SceneManager.LoadScene(0);
    }
}