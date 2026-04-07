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
        print("GAME MANGER 1");
        yield return null;
        while (GetPlayer() == null)
            yield return null;
        print("GAME MANGER 1");

        while (PlayerSaveSystem.Instance == null)
            yield return null;
        print("GAME MANGER 1");

        while (PlayerSaveSystem.Instance.GetData().position[1] == 0) yield return null;
        print("GAME MANGER 1");

        yield return new WaitForEndOfFrame();
        yield return null;

        OnStartGame();
    }

    public void OnStartGame()
    {
        print("ON start game");
        FindFirstObjectByType<Door>().DipFromBlack();

        isGameGoing = true;
        player = GetPlayer();

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

        if (autosaveRoutine != null)
            StopCoroutine(autosaveRoutine);

        autosaveRoutine = StartCoroutine(Autosave());
        FindFirstObjectByType<CheckForInHouse>().OnStartGame();
        Time.timeScale = 1;

        Invoke(nameof(AddTutorial), 1f);
    }

    void AddTutorial()
    {
        StartCoroutine(AddTut());
    }
    IEnumerator AddTut()
    {
        while (TaskManager.Instance == null) yield return null;
        while (TaskManager.Instance.tasks.Count == 0) yield return null;
        if (PlayerMailInventory.Instance.GetSaveData().carriedMails.Count == 0 && !PlayerSaveSystem.Instance.GetData().hasBag)
        {
            print(TaskManager.Instance.tasks.Count);
            var len = TaskManager.Instance.tasks.Count - 1;
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