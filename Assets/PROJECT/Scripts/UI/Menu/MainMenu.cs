using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    public GameObject savesPanel;

    public GameObject autosaveButton;
    void Start()
    {
        autosaveButton.GetComponent<Button>().interactable = SaveGameManager.Instance.HasAutosave();
    }
    public void PlayGame()
    {
        savesPanel.SetActive(true);
        gameObject.SetActive(false);
    }
    public void ExitGame()
    {
        Debug.Log("Игра закрылась");
        Application.Quit();
    }
    public void PlayAutosave()
    {
        bool success = SaveGameManager.Instance.HasAutosave();
        if (success)
        {
            SceneManager.LoadScene("Game");
        }
    }
}
