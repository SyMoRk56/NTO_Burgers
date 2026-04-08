using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSaves : MonoBehaviour
{
    public SavePanel[] saves; // Панели для сохранений
    private string manualFolder; // Путь к папке с ручными сохранениями

    public GameObject warningPanel, autosaveButton; // Предупреждение и кнопка автосейва

    private void Awake()
    {
        // Проверка существования сохранений и блокировка кнопок при проблеме
        bool p = false;

        if (!SaveGameManager.Instance.CheckSave("1") && SaveGameManager.Instance.HasManual("1"))
        {
            p = true;
            saves[0].GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("2") && SaveGameManager.Instance.HasManual("2"))
        {
            p = true;
            saves[1].GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("3") && SaveGameManager.Instance.HasManual("3"))
        {
            saves[2].GetComponent<Button>().interactable = false;
            p = true;
        }
        if (p)
            warningPanel.SetActive(true); // Показываем предупреждение
    }

    private void Start()
    {
        // Обновляем панели после старта
        RefreshPanels();
    }

    public void RefreshPanels()
    {
        print("Refresh");

        // Определяем путь к папке с ручными сохранениями
        manualFolder = Path.Combine(Application.persistentDataPath, "Saves/manual");

        if (!Directory.Exists(manualFolder))
        {
            Debug.Log("Manual save folder not found.");
            return;
        }
        print("Refresh1");

        // Получаем список всех файлов json и сортируем
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
                print("YYYEEESS");

                panel.savePath = jsonPath;
                panel.screenshotPath = screenshotPath;

                FillPanel(panel); // Заполняем панель данными
            }
        }

        // Проверяем корректность сохранений и блокируем кнопки
        var p = false;
        if (!SaveGameManager.Instance.CheckSave("1") && SaveGameManager.Instance.HasManual("1"))
        {
            p = true;
            saves[0].GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("2") && SaveGameManager.Instance.HasManual("2"))
        {
            p = true;
            saves[1].GetComponent<Button>().interactable = false;
        }
        if (!SaveGameManager.Instance.CheckSave("3") && SaveGameManager.Instance.HasManual("3"))
        {
            saves[2].GetComponent<Button>().interactable = false;
            p = true;
        }
        if (p)
            warningPanel.SetActive(true);

        // Проверяем доступность автосейва
        autosaveButton.GetComponent<Button>().interactable = SaveGameManager.Instance.HasAutosave();
    }

    private void FillPanel(SavePanel panel)
    {
        // Считываем данные сохранения из json
        string json = File.ReadAllText(panel.savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        panel.dateText.text = data.saveDate; // Обновляем дату

        LoadScreenshot(panel); // Загружаем скриншот
    }

    private void LoadScreenshot(SavePanel panel)
    {
        if (!File.Exists(panel.screenshotPath))
        {
            // Если скриншота нет, скрываем изображение
            panel.screenshot.texture = null;
            panel.screenshot.CrossFadeAlpha(0, 0, false);
            return;
        }

        byte[] bytes = File.ReadAllBytes(panel.screenshotPath);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        panel.screenshot.texture = tex;
        panel.screenshot.CrossFadeAlpha(1, 0, false); // Показываем скриншот
    }

    public void LoadSave(int num)
    {
        string saveName = num.ToString();

        // Если сохранение отсутствует, создаем пустое
        if (!SaveGameManager.Instance.HasManual(saveName))
        {
            Debug.Log("Save not found, creating a new empty save: " + saveName);
            SaveGameManager.Instance.SaveManual(saveName, false);

            AutoSaveSlot auto = new AutoSaveSlot { slotName = saveName };
            string autosavePath = Path.Combine(
                Application.persistentDataPath,
                "Saves",
                "autosave.json"
            );
            File.WriteAllText(autosavePath, JsonUtility.ToJson(auto, true));
        }

        // Устанавливаем загрузку в GameManager
        GameManager.Instance.pendingManualLoad = saveName;
        GameManager.Instance.loadAutoOnStart = false;

        // Загружаем сцену игры
        SceneManager.LoadScene("Game");
    }

    public void Close()
    {
        // Закрываем меню сохранений
        transform.parent.Find("MainMenu").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void DeleteSave(string saveName)
    {
        print(saveName);

        SaveGameManager.Instance.DeleteSave(saveName);

        int i = int.Parse(saveName) - 1;
        print(i);

        // Сброс панели сохранения
        saves[i].screenshot.texture = null;
        saves[i].screenshot.CrossFadeAlpha(0, 0, false);
        saves[i].dateText.text = "";

        RefreshPanels(); // Обновляем панели
    }
}