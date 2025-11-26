using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSaves : MonoBehaviour
{
    public SavePanel[] saves;
    private string manualFolder;

    public GameObject warningPanel, autosaveButton;

    private void Start()
    {
        manualFolder = Path.Combine(Application.persistentDataPath, "Saves/manual");

        if (!Directory.Exists(manualFolder))
        {
            Debug.Log("Manual save folder not found.");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(manualFolder, "*.json")
            .OrderBy(f => f)
            .ToArray();

        for (int i = 0; i < saves.Length; i++)
        {
            SavePanel panel = saves[i];

            if (i < jsonFiles.Length)
            {
                string jsonPath = jsonFiles[i];
                string nameNoExt = Path.GetFileNameWithoutExtension(jsonPath);
                string screenshotPath = Path.Combine(manualFolder, nameNoExt + ".png");

                panel.savePath = jsonPath;
                panel.screenshotPath = screenshotPath;

                FillPanel(panel);
            }
        }
        if (!SaveGameManager.Instance.CheckAutoSave())
        {
            autosaveButton.GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("1") && SaveGameManager.Instance.HasManual("1"))
        {
            saves[0].GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("2") && SaveGameManager.Instance.HasManual("2"))
        {
            saves[1].GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("3") && SaveGameManager.Instance.HasManual("3"))
        {
            saves[2].GetComponent<Button>().interactable = false;
        }
    }

    private void FillPanel(SavePanel panel)
    {
        string json = File.ReadAllText(panel.savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        panel.dateText.text = data.saveDate;

        LoadScreenshot(panel);
    }

    private void LoadScreenshot(SavePanel panel)
    {
        if (!File.Exists(panel.screenshotPath))
        {
            panel.screenshot.texture = null;
            return;
        }

        byte[] bytes = File.ReadAllBytes(panel.screenshotPath);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        panel.screenshot.texture = tex;
    }

    public void LoadSave(int num)
    {
        string saveName = num.ToString();

        // Если сейва нет — создаём пустой
        if (!SaveGameManager.Instance.HasManual(saveName))
        {
            Debug.Log("Save not found, creating a new empty save: " + saveName);
            SaveGameManager.Instance.SaveManual(saveName);
        }

        // Указываем GameManager какой сейв загружать
        GameManager.Instance.pendingManualLoad = saveName;
        GameManager.Instance.loadAutoOnStart = false;

        // Загружаем сцену игры
        SceneManager.LoadScene("Game");
    }


    public void Close()
    {
        transform.parent.Find("MainMenu").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
