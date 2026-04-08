using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame; // Флаг паузы
    public GameObject pauseGameMenu; // Главное меню паузы

    public GameObject settings; // Панель настроек
    public GameObject saveMenu; // Панель сохранений
    public SavePanel[] saves; // Панели слотов сохранений

    private void Start()
    {
        Setup(); // Настройка меню сохранений при старте
    }

    void Update()
    {
        // Если нажата Escape и игрок может двигаться
        if (Input.GetKeyDown(KeyCode.Escape) && PlayerManager.instance.CanMove)
        {
            // Проверка активных диалогов
            if (IsAnyDialogueActive())
            {
                return; // Если диалог активен — не открываем паузу
            }

            // Проверка UI стола
            if (IsInTableUI()) return;

            // Переключение паузы
            if (PauseGame) Resume();
            else Pause();
        }
    }

    // Проверка активного диалога в сцене
    private bool IsAnyDialogueActive()
    {
        DialogueRunner[] dialogueRunners = FindObjectsOfType<DialogueRunner>();
        foreach (DialogueRunner runner in dialogueRunners)
        {
            if (runner.IsDialogueActive)
            {
                return true;
            }
        }
        return false;
    }

    // Проверка, открыт ли UI стола
    private bool IsInTableUI()
    {
        DeskUI[] desks = FindObjectsOfType<DeskUI>();
        foreach (DeskUI desk in desks)
        {
            if (desk.isInTable)
            {
                return true;
            }
        }
        return false;
    }

    // Возврат из паузы
    public void Resume()
    {
        pauseGameMenu.SetActive(false); // Скрываем меню паузы
        Cursor.lockState = CursorLockMode.Locked; // Блокируем курсор
        Cursor.visible = false;
        Time.timeScale = 1f; // Возобновляем время
        PauseGame = false; // Сбрасываем флаг
    }

    // Включение паузы
    public void Pause()
    {
        pauseGameMenu.SetActive(true); // Показываем меню паузы
        saves[0].transform.parent.parent.gameObject.SetActive(false); // Скрываем слоты сохранений
        settings.SetActive(false); // Скрываем настройки
        Cursor.lockState = CursorLockMode.None; // Освобождаем курсор
        Cursor.visible = true;
        Time.timeScale = 0f; // Останавливаем время
        PauseGame = true; // Устанавливаем флаг
    }

    // Возврат в главное меню
    public void ReturnToMainMenu()
    {
        GameManager.Instance.ExitToMenu();
    }

    // Сохранение в указанный слот
    public void SaveToSlot(string slotName)
    {
        if (string.IsNullOrEmpty(slotName))
        {
            Debug.LogWarning("SaveToSlot: slotName is null or empty!");
            return;
        }

        bool exists = SaveGameManager.Instance.HasManual(slotName);

        if (!exists)
        {
            Debug.Log("SaveToSlot: Slot does not exist, creating -> " + slotName);
        }

        GameManager.Instance.currentManualSlot = slotName; // Устанавливаем текущий слот
        SaveGameManager.Instance.SaveAuto(true); // Сохраняем игру
        Debug.Log("PauseMenu: Saved to slot -> " + slotName);
        Setup(); // Обновляем панели сохранений
    }

    // Переключение видимости меню сохранений
    public void ToggleSaveMenu()
    {
        saveMenu.SetActive(!saveMenu.activeSelf);
    }

    // Настройка меню сохранений (загрузка данных и скриншотов)
    void Setup()
    {
        var manualFolder = Path.Combine(Application.persistentDataPath, "Saves/manual");

        if (!Directory.Exists(manualFolder))
        {
            Debug.Log("Manual save folder not found.");
            return;
        }

        // Получаем все JSON файлы сохранений
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

                panel.savePath = jsonPath; // Путь к json
                panel.screenshotPath = screenshotPath; // Путь к скриншоту

                FillPanel(panel); // Заполняем панель данными
            }
        }
    }

    // Заполнение панели конкретного сохранения
    private void FillPanel(SavePanel panel)
    {
        string json = File.ReadAllText(panel.savePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        panel.dateText.text = data.saveDate; // Устанавливаем дату сохранения

        LoadScreenshot(panel); // Загружаем скриншот
    }

    // Загрузка скриншота на панель
    private void LoadScreenshot(SavePanel panel)
    {
        if (!File.Exists(panel.screenshotPath))
        {
            panel.screenshot.texture = null; // Если нет скриншота — скрываем
            panel.screenshot.CrossFadeAlpha(0, 0, false);
            return;
        }

        byte[] bytes = File.ReadAllBytes(panel.screenshotPath);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        panel.screenshot.CrossFadeAlpha(1, 0, false); // Показываем изображение

        panel.screenshot.texture = tex;
    }
}