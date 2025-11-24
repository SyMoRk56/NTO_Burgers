using System.Collections;
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

        Time.timeScale = 1;
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
        SaveGameManager.Instance.SaveAuto(false);

        if (!string.IsNullOrEmpty(currentManualSlot))
        {
            SaveGameManager.Instance.SaveManual(currentManualSlot);
        }

        isGameGoing = false;

        if (autosaveRoutine != null)
        {
            StopCoroutine(autosaveRoutine);
            autosaveRoutine = null;
        }

        pendingManualLoad = null;
        loadAutoOnStart = true;
        currentManualSlot = null;

        SceneManager.LoadScene(0);
    }
}
