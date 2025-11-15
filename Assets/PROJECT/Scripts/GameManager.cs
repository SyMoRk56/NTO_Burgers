using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    GameObject player;
    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0.name == "Game")
        {
            OnStartGame();
        }
    }

    public void OnStartGame()
    {
        SaveGameManager.Instance.LoadAuto();
    }
    public GameObject GetPlayer()
    {
        if(player != null) return player;
        player = GameObject.FindWithTag("Player");
        return player;
    }
}

