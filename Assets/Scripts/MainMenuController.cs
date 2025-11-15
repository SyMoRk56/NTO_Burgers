using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button newGameButton;
    public Button continueButton;

    [Header("Scene Settings")]
    public string firstLevelScene = "Game"; // Название первой игровой сцены
    public string saveFileName = "gameSave.dat";

    private string savePath;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        // Настраиваем кнопки
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(StartNewGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);

            // Показываем кнопку только если есть сохранение
            continueButton.gameObject.SetActive(File.Exists(savePath));
        }
    }

    public void StartNewGame()
    {
        // Удаляем старое сохранение, если есть
        if (File.Exists(savePath))
            File.Delete(savePath);

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();

        SceneManager.LoadScene(firstLevelScene);
    }

    public void ContinueGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ContinueGame();
        else
            SceneManager.LoadScene(firstLevelScene);
    }

    // Метод для обновления кнопки Continue после выхода из игры
    public void UpdateContinueButton()
    {
        if (continueButton != null)
            continueButton.gameObject.SetActive(File.Exists(savePath));
    }
}
