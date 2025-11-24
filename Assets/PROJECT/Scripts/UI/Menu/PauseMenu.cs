using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public GameObject pauseGameMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseGame) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseGameMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        PauseGame = false;
    }

    public void Pause()
    {
        pauseGameMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        PauseGame = true;
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.ExitToMenu();
    }
}
