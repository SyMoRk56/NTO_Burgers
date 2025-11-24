using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GameObject player;

    public bool isGameGoing = false;
    private Coroutine autosaveRoutine;

    // Flags for saves
    public string pendingManualLoad = null;
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

        // Load MANUAL save if selected
        if (pendingManualLoad != null)
        {
            SaveGameManager.Instance.LoadManual(pendingManualLoad);
            pendingManualLoad = null;
        }
        // Or load AUTOSAVE by default
        else if (loadAutoOnStart)
        {
            SaveGameManager.Instance.LoadAuto();
        }

        if (autosaveRoutine != null)
            StopCoroutine(autosaveRoutine);

        autosaveRoutine = StartCoroutine(Autosave());
    }

    public GameObject GetPlayer()
    {
        if (player != null) return player;

        player = GameObject.FindWithTag("Player");

        if (player == null)
            Debug.LogWarning("Player not found! Add 'Player' tag to player object.");

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
}
