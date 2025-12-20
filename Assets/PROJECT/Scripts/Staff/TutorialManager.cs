using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [System.Serializable]
    public class TutorialArrow
    {
        public string stepId; // Уникальный ID шага туториала
        public GameObject arrowObject; // 3D стрелка
        public bool hideAfterComplete = true; // Скрывать ли стрелку после выполнения
    }

    public List<TutorialArrow> tutorialArrows = new List<TutorialArrow>();
    private HashSet<string> completedSteps = new HashSet<string>();

    private bool isInitialized = false;

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
        }
    }

    void Start()
    {
        InitializeTutorial();
    }

    // Инициализация туториала (вызывается при старте игры)
    public void InitializeTutorial()
    {
        if (isInitialized) return;

        // Загружаем данные туториала при старте
        LoadTutorialData();

        // Применяем состояние стрелок
        ApplyTutorialState();

        isInitialized = true;
    }

    // Отметить шаг туториала как выполненный
    public void CompleteTutorialStep(string stepId)
    {
        if (completedSteps.Contains(stepId)) return;

        completedSteps.Add(stepId);

        // Скрыть соответствующую стрелку
        HideArrowForStep(stepId);

        // Сохранить состояние туториала
        SaveTutorialData();

        Debug.Log($"Tutorial step completed: {stepId}");
    }

    // Проверить, выполнен ли шаг туториала
    public bool IsStepCompleted(string stepId)
    {
        return completedSteps.Contains(stepId);
    }

    // Показать стрелку для шага (если она еще не была завершена)
    public void ShowArrowForStep(string stepId)
    {
        if (IsStepCompleted(stepId)) return;

        foreach (var arrow in tutorialArrows)
        {
            if (arrow.stepId == stepId && arrow.arrowObject != null)
            {
                arrow.arrowObject.SetActive(true);
                break;
            }
        }
    }

    // Скрыть стрелку для шага
    public void HideArrowForStep(string stepId)
    {
        foreach (var arrow in tutorialArrows)
        {
            if (arrow.stepId == stepId && arrow.arrowObject != null)
            {
                if (arrow.hideAfterComplete)
                {
                    arrow.arrowObject.SetActive(false);
                }
                break;
            }
        }
    }

    // Скрыть все стрелки
    public void HideAllArrows()
    {
        foreach (var arrow in tutorialArrows)
        {
            if (arrow.arrowObject != null)
            {
                arrow.arrowObject.SetActive(false);
            }
        }
    }

    // Применить состояние туториала к стрелкам
    private void ApplyTutorialState()
    {
        foreach (var arrow in tutorialArrows)
        {
            if (arrow.arrowObject != null)
            {
                // Если шаг завершен - скрываем стрелку
                if (IsStepCompleted(arrow.stepId) && arrow.hideAfterComplete)
                {
                    arrow.arrowObject.SetActive(false);
                }
            }
        }
    }

    // Получить данные для сохранения
    public TutorialSaveData GetSaveData()
    {
        TutorialSaveData data = new TutorialSaveData();
        data.completedTutorialSteps = new List<string>(completedSteps);
        return data;
    }

    // Загрузить данные туториала
    public void LoadSaveData(TutorialSaveData data)
    {
        if (data == null || data.completedTutorialSteps == null)
        {
            completedSteps = new HashSet<string>();
        }
        else
        {
            completedSteps = new HashSet<string>(data.completedTutorialSteps);
        }

        // Применяем состояние после загрузки
        if (isInitialized)
        {
            ApplyTutorialState();
        }
    }

    // Сохранить данные туториала
    private void SaveTutorialData()
    {
        // Сохраняем через SaveGameManager
        SaveGameManager.Instance.SaveAuto(false);
    }

    // Загрузить данные туториала
    private void LoadTutorialData()
    {
        // Данные будут загружены через SaveGameManager при загрузке сохранения
        // Если это новое сохранение, список будет пустым
    }

    // Очистить все данные туториала (для тестирования)
    public void ClearTutorialData()
    {
        completedSteps.Clear();
        ApplyTutorialState();
        SaveTutorialData();
    }
}