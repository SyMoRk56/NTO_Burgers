using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSaves : MonoBehaviour
{
    public SavePanel[] saves;
    private string manualFolder;
    private void Start()
    {
        manualFolder = Path.Combine(Application.persistentDataPath, "Saves/manual");

        if (!Directory.Exists(manualFolder))
        {
            Debug.Log("Manual save folder not found.");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(manualFolder, "*.json");

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
            else
            {

            }
        }
    }
    private void FillPanel(SavePanel panel)
    {
        // Загружаем JSON и вытаскиваем дату
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
        bool success = SaveGameManager.Instance.HasManual(num.ToString());
        if (!success)
        {
            SaveGameManager.Instance.SaveManual(num.ToString());
        }
        SceneManager.LoadScene("Game");
    }
    public void Close()
    {
        transform.parent.Find("MainMenu").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
