using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Меню списка адресов (задач). Отображает письма игрока в виде вкладок/табы.
/// </summary>
public class AdressListMenu : MonoBehaviour
{
    // ===== Singleton для удобного доступа =====
    public static AdressListMenu Instance;

    [Header("UI Prefabs")]
    public GameObject tabTaskUIPrefab;   // Префаб одной вкладки письма
    public Transform tasksParent;        // Контейнер для всех вкладок
    public GameObject label;             // Текстовая метка/лейбл сверху

    private const int MAX_TASKS = 3;     // Максимальное количество отображаемых задач

    private void Awake()
    {
        // Singleton
        Instance = this;
    }

    private void Start()
    {
        // Активируем лейбл, если есть
        if (label != null) label.SetActive(true);

        // Инициализация списка задач
        UpdateTasks();
    }

    /// <summary>
    /// Обновляет список отображаемых задач в меню.
    /// Сначала отображаются сюжетные, потом обычные, с ограничением MAX_TASKS.
    /// </summary>
    public void UpdateTasks()
    {
        if (tasksParent == null || tabTaskUIPrefab == null) return;

        // Очищаем текущие вкладки
        for (int i = tasksParent.childCount - 1; i >= 0; i--)
            Destroy(tasksParent.GetChild(i).gameObject);

        var allMails = PlayerMailInventory.Instance.carriedMails;

        // Сюжетные письма
        var storyMails = allMails.Where(t => t.isStory);

        // Обычные письма
        var regularMails = allMails.Where(t => !t.isStory);

        // Объединяем, ограничиваем максимумом
        var mails = storyMails
            .Concat(regularMails)
            .Take(MAX_TASKS)
            .ToList();

        foreach (var task in mails)
        {
            // Создаем вкладку для письма
            var go = Instantiate(tabTaskUIPrefab, tasksParent);
            go.SetActive(true);

            // Обновляем текст
            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                // Проверяем, рыба ли это (Fish_)
                if (task.recieverName.Contains("Fish_"))
                {
                    // Формируем текст: адрес + имя рыбы + количество
                    var parts = task.recieverName.Replace("Fish_", "").Split(" ");
                    text.text = AdressConverter.Convert(task.adress) +
                                ":\n" +
                                LocalizationManager.Instance.Get(parts[0]) + " " +
                                parts[1] + " " +
                                LocalizationManager.Instance.Get("Count");
                }
                else
                {
                    text.text = AdressConverter.Convert(task.adress);
                }
            }

            // Сюжетные письма отмечаем звездочкой
            var star = go.transform.Find("star");
            if (star != null)
                star.gameObject.SetActive(task.isStory);

            // Иконка рыбы
            var fishIcon = go.transform.Find("FishIcon");
            if (fishIcon != null)
                fishIcon.gameObject.SetActive(task.recieverName.Contains("Fish"));
        }

        // Обновляем размеры лейбла в зависимости от количества задач
        if (label != null)
        {
            var rect = label.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0, 1000 - 229.6246f * tasksParent.childCount);
        }
    }

    /// <summary>
    /// Показывает или скрывает контейнер с задачами
    /// </summary>
    /// <param name="visible">true — показать, false — скрыть</param>
    public void SetVisible(bool visible)
    {
        if (tasksParent != null)
            tasksParent.gameObject.SetActive(visible);
    }
}

/// <summary>
/// Статический класс для конвертации внутреннего формата адреса в читаемый текст.
/// Например: Post_A3 -> "StreetName 3"
/// </summary>
public static class AdressConverter
{
    /// <summary>
    /// Конвертирует raw-адрес в локализованный формат
    /// </summary>
    /// <param name="rawAdress">Внутренний формат адреса (например: "Post_A3")</param>
    /// <returns>Читаемый адрес (например: "MainStreet 3")</returns>
    public static string Convert(string rawAdress)
    {
        Debug.Log("AdressConverter: " + rawAdress);

        // Если это NPC или Tutorial, сразу локализуем
        if (rawAdress.Contains("NPC") || rawAdress.Contains("Tutorial"))
            return LocalizationManager.Instance.Get(rawAdress);

        // Получаем часть после "Post_"
        string p1 = rawAdress.Split("_")[1]; // например "A3"

        // Разделяем буквы и цифры
        var chars = p1.ToList();
        var letters = new List<char>();
        string streetNumber = "";

        foreach (var c in chars)
        {
            if ("1234567890".Contains(c))
                streetNumber += c;  // цифра — номер улицы
            else
                letters.Add(c);     // буква — имя улицы
        }

        // Собираем имя улицы
        string rawStreetName = "";
        foreach (var c in letters)
            rawStreetName += c;

        // Локализуем имя улицы
        string streetName = LocalizationManager.Instance.Get(rawStreetName);

        return streetName + " " + streetNumber;
    }
}