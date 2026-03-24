using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
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
        
        RefreshPanels();

    }

    public void RefreshPanels()
    {
        print("Refresh");

        manualFolder = Path.Combine(Application.persistentDataPath, "Saves/manual");

        if (!Directory.Exists(manualFolder))
        {
            Debug.Log("Manual save folder not found.");
            return;
        }
        print("Refresh1");

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

                FillPanel(panel);
            }
        }
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
        autosaveButton.GetComponent<Button>().interactable = SaveGameManager.Instance.HasAutosave();
    }

    private void Awake()
    {
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
            warningPanel.SetActive(true);
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

        panel.screenshot.texture = tex;
        panel.screenshot.CrossFadeAlpha(1, 0, false);

    }

    public void LoadSave(int num)
    {
        string saveName = num.ToString();

        // ���� ����� ��� � ������ ������
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

        // ��������� GameManager ����� ���� ���������
        GameManager.Instance.pendingManualLoad = saveName;
        GameManager.Instance.loadAutoOnStart = false;

        // ��������� ����� ����
        SceneManager.LoadScene("Game");
    }


    public void Close()
    {
        transform.parent.Find("MainMenu").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    public void DeleteSave(string saveName)
    {
        print(saveName);
        SaveGameManager.Instance.DeleteSave(saveName);
        int i = int.Parse(saveName)-1;
        print(i);
        saves[i].screenshot.texture = null;
        saves[i].screenshot.CrossFadeAlpha(0, 0, false);

        saves[i].dateText.text = "";
        RefreshPanels();
    }
}
