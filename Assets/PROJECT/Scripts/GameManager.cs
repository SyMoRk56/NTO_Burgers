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
        yield return null;
        while (GetPlayer() == null)
            yield return null;
        while (PlayerSaveSystem.Instance == null)
            yield return null;
        while (PlayerSaveSystem.Instance.GetData().position[1] == 0) yield return null;
        yield return new WaitForEndOfFrame();

        // Теперь мы точно можем загружать сейв
        OnStartGame();
    }

    public void OnStartGame()
    {
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
        FindFirstObjectByType<EnterToHouse>().DipFromBlack();

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

        // 1. Сохраняем данные в слот
        SaveGameManager.Instance.SaveManual(slot, false);

        // 2. Обновляем autosave.json (записываем только имя слота)
        AutoSaveSlot auto = new AutoSaveSlot { slotName = slot };
        string autosavePath = Path.Combine(
            Application.persistentDataPath,
            "Saves",
            "autosave.json"
        );
        File.WriteAllText(autosavePath, JsonUtility.ToJson(auto, true));

        Debug.Log("Saved slot + autosave. Returning to menu...");

        // 3. Останавливаем игру и корутины
        isGameGoing = false;
        if (autosaveRoutine != null)
        {
            StopCoroutine(autosaveRoutine);
            autosaveRoutine = null;
        }

        // 4. Сбрасываем параметры загрузки
        pendingManualLoad = null;
        loadAutoOnStart = true;

        // 5. Загружаем главное меню
        SceneManager.LoadScene(0);
    }

}
