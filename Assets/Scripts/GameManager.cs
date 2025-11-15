using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Elements")]
    public Button continueButton;
    public Button newGameButton;
    public Text saveInfoText;

    [Header("Save Settings")]
    public string saveFileName = "gameSave.dat";

    private string savePath;
    private GameData currentGameData;
    private bool gameLoaded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);

        CheckForSavedGame();
    }

    public void CheckForSavedGame()
    {
        bool saveExists = File.Exists(savePath);

        if (continueButton != null)
            continueButton.gameObject.SetActive(saveExists);

        if (saveInfoText != null)
        {
            if (saveExists)
            {
                GameData tempData = LoadGameData();
                if (tempData != null)
                {
                    saveInfoText.text = $"Уровень: {tempData.currentLevel}\nСчет: {tempData.playerScore}";
                }
            }
            else
            {
                saveInfoText.text = "Сохранение не найдено";
            }
        }
    }

    public void StartNewGame()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        currentGameData = GameData.CreateNewGame();
        ApplyGameData(currentGameData);
        gameLoaded = true;
        SaveGame();

        SceneManager.LoadScene("Game"); // Замените на вашу игровую сцену
    }

    public void ContinueGame()
    {
        if (LoadGame())
        {
            ApplyGameData(currentGameData);
            gameLoaded = true;
            SceneManager.LoadScene("Game"); // Можно заменить на Level + currentLevel
        }
        else
        {
            StartNewGame();
        }
    }

    private void ApplyGameData(GameData gameData)
    {
        AudioListener.volume = gameData.musicVolume;
        QualitySettings.SetQualityLevel(gameData.graphicsQuality);
    }

    public void SaveGame()
    {
        if (currentGameData == null) return;

        if (gameLoaded)
            UpdateGameDataFromCurrentState();

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            formatter.Serialize(stream, currentGameData);
        }

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        if (saveInfoText != null)
            saveInfoText.text = $"Уровень: {currentGameData.currentLevel}\nСчет: {currentGameData.playerScore}";
    }

    private bool LoadGame()
    {
        currentGameData = LoadGameData();
        return currentGameData != null;
    }

    private GameData LoadGameData()
    {
        if (!File.Exists(savePath)) return null;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                return (GameData)formatter.Deserialize(stream);
            }
        }
        catch
        {
            return null;
        }
    }

    private void UpdateGameDataFromCurrentState()
    {
        currentGameData.playTime += Time.deltaTime;
    }

    void OnApplicationQuit()
    {
        if (gameLoaded) SaveGame();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && gameLoaded) SaveGame();
    }
}
