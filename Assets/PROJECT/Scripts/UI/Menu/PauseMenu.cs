using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public GameObject pauseGameMenu;

    private void Start()
    {
        Setup();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ������� ��������� �������� �������
            if (IsAnyDialogueActive())
            {
                return; // �� ��������� ����� �� ����� �������
            }

            // ����� ��������� UI �����
            if (IsInTableUI()) return;

            if (PauseGame) Resume();
            else Pause();
        }
    }

    // ����� ��� �������� �������� ��������
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

    // ����� ��� ��������, ��������� �� ����� � UI �����
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

    public void Resume()
    {
        pauseGameMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        PauseGame = false;
    }
    public GameObject settings;
    public void Pause()
    {
        pauseGameMenu.SetActive(true);
        saves[0].transform.parent.parent.gameObject.SetActive(false);
        settings.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        PauseGame = true;
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.ExitToMenu();
    }

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

        GameManager.Instance.currentManualSlot = slotName;
        SaveGameManager.Instance.SaveAuto(true);
        Debug.Log("PauseMenu: Saved to slot -> " + slotName);
        Setup();
    }

    public GameObject saveMenu;
    public void ToggleSaveMenu()
    {
        saveMenu.SetActive(!saveMenu.activeSelf);
    }

    public SavePanel[] saves;

    void Setup()
    {
        var manualFolder = Path.Combine(Application.persistentDataPath, "Saves/manual");

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
            panel.screenshot.CrossFadeAlpha(0, 0, false);

            return;
        }

        byte[] bytes = File.ReadAllBytes(panel.screenshotPath);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        panel.screenshot.CrossFadeAlpha(1, 0, false);

        panel.screenshot.texture = tex;
    }
}