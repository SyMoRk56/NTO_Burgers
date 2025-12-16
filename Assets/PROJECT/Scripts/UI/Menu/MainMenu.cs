using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
public class MainMenu : MonoBehaviour
{
    public GameObject savesPanel;

    public GameObject autosaveButton;
    void Start()
    {
        Time.timeScale = 1;
        if (!SaveGameManager.Instance.CheckAutoSave())
        {

            autosaveButton.GetComponent<Button>().interactable = false;
            FindFirstObjectByType<MainMenuSaves>().warningPanel.SetActive(true);
            return;
        }
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
        LoadAutoSave();
    }
    public void LoadAutoSave()
    {
        
        // Проверяем, существует ли автосейв
        if (!SaveGameManager.Instance.HasAutosave())
        {
            Debug.LogWarning("No autosave found!");
            return;
        }

        // Читаем autosave.json
        string autosavePath = Path.Combine(
            Application.persistentDataPath,
            "Saves",
            "autosave.json"
        );

        string json = File.ReadAllText(autosavePath);
        AutoSaveSlot data = JsonUtility.FromJson<AutoSaveSlot>(json);

        if (string.IsNullOrEmpty(data.slotName))
        {
            Debug.LogError("Autosave is corrupted — slot name missing!");
            return;
        }

        string slotName = data.slotName;

        // Проверяем, что сохранённый слот существует
        if (!SaveGameManager.Instance.HasManual(slotName))
        {
            Debug.LogWarning("Autosave points to missing slot, creating new one: " + slotName);
            SaveGameManager.Instance.SaveManual(slotName, false);
        }

        // Говорим GameManager какой слот грузить
        GameManager.Instance.pendingManualLoad = slotName;
        GameManager.Instance.loadAutoOnStart = false; // т.к. мы загружаем вручную

        // Загружаем сцену игры
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");

        Debug.Log("Loading autosave slot: " + slotName);
    }

}
