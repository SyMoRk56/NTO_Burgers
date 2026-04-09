using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Основной менеджер игры: загрузка, автосейв, старт игры, возврат в меню
/// </summary>
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
        // Singleton
        if (Instance != null && Instance != this) // не в класическом понимании
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
        // Загружаем настройки игры
        SettingsSaveManager.Instance.LoadSettings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Срабатывает при загрузке любой сцены
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
            StartCoroutine(DelayedGameStart());
    }

    /// <summary>
    /// Ждём инициализации игрока и данных
    /// </summary>
    private IEnumerator DelayedGameStart()
    {
        yield return null;

        while (GetPlayer() == null) yield return null;
        while (PlayerSaveSystem.Instance == null) yield return null;
        while (PlayerSaveSystem.Instance.GetData().position[1] == 0) yield return null;

        yield return new WaitForEndOfFrame();
        yield return null;

        OnStartGame();
    }

    /// <summary>
    /// Старт игры после загрузки сцены
    /// </summary>
    public void OnStartGame()
    {
        // Закрываем черный экран двери
        FindFirstObjectByType<Door>()?.DipFromBlack();

        isGameGoing = true;
        player = GetPlayer();

        // Загрузка слота
        if (!string.IsNullOrEmpty(pendingManualLoad))
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

        // Запуск автосейва
        if (autosaveRoutine != null)
            StopCoroutine(autosaveRoutine);
        autosaveRoutine = StartCoroutine(Autosave());

        FindFirstObjectByType<CheckForInHouse>()?.OnStartGame();

        Time.timeScale = 1;

        // Добавление стартового туториала
        Invoke(nameof(AddTutorial), 1f);
    }

    /// <summary>
    /// Добавление писем и туториала на старте
    /// </summary>
    private void AddTutorial()
    {
        StartCoroutine(AddTut());
    }

    private IEnumerator AddTut()
    {
        while (TaskManager.Instance == null) yield return null;
        while (TaskManager.Instance.tasks.Count == 0) yield return null;

        if (PlayerMailInventory.Instance.GetSaveData().carriedMails.Count == 0 &&
            !PlayerSaveSystem.Instance.GetData().hasBag)
        {
            int len = TaskManager.Instance.tasks.Count - 1;

            for (int i = 0; i <= 2; i++)
            {
                var task = TaskManager.Instance.tasks[len - i];
                var mailItem = MailManager.Instance.GetMailById(task.id);

                var enriched = new Task(
                    task.recieverName,
                    task.adress,
                    task.id,
                    mailItem != null && mailItem.isStory
                );

                PlayerMailInventory.Instance.AddMailToInventory(enriched);
            }
        }
    }

    /// <summary>
    /// Получение игрока
    /// </summary>
    public GameObject GetPlayer()
    {
        if (player != null) return player;
        player = GameObject.FindWithTag("Player");
        return player;
    }

    /// <summary>
    /// Автосейв каждые 30 секунд
    /// </summary>
    private IEnumerator Autosave()
    {
        while (isGameGoing)
        {
            yield return new WaitForSeconds(30f);
            SaveGameManager.Instance.SaveAuto(true);
        }
    }

    /// <summary>
    /// Выход в меню с сохранением
    /// </summary>
    public void ExitToMenu()
    {
        SaveAndGoToMenu();
    }

    public void SaveAndGoToMenu()
    {
        if (string.IsNullOrEmpty(currentManualSlot))
        {
            Debug.LogWarning("Cannot exit to menu — no manual slot selected!");
            return;
        }

        // Сохраняем выбранный слот
        SaveGameManager.Instance.SaveManual(currentManualSlot, false);

        // Создаём файл автосейва
        AutoSaveSlot auto = new AutoSaveSlot { slotName = currentManualSlot };
        string autosavePath = Path.Combine(
            Application.persistentDataPath,
            "Saves",
            "autosave.json"
        );
        File.WriteAllText(autosavePath, JsonUtility.ToJson(auto, true));

        Debug.Log("Saved slot + autosave. Returning to menu...");

        // Останавливаем игру и автосейв
        isGameGoing = false;
        if (autosaveRoutine != null)
        {
            StopCoroutine(autosaveRoutine);
            autosaveRoutine = null;
        }

        pendingManualLoad = null;
        loadAutoOnStart = true;

        // Загружаем сцену меню
        SceneManager.LoadScene(0);
    }
}