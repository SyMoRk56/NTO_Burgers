using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("UI элементы")]
    public TextMeshProUGUI nameText;       // Поле для имени персонажа
    public TextMeshProUGUI phraseText;     // Поле для отображения текста фразы
    public Transform choicesParent;        // Родитель для кнопок выбора
    public GameObject choiceButtonPrefab;  // Префаб кнопки выбора

    DialogueRunner runner;                 // Ссылка на DialogueRunner родителя
    Coroutine typeCoroutine;               // Корутин для эффекта печати текста
    float typeSpeed = 0.03f;               // Скорость печати символов

    public bool hideOnAwake = true;       // Скрывать UI при запуске сцены

    bool isTyping = false;                 // Флаг активной печати текста
    string currentFullText = "";           // Полный текст текущей фразы

    public bool IsTyping => isTyping;      // Публичный геттер для проверки печати

    void Awake()
    {
        // Получаем ссылку на DialogueRunner родителя
        runner = GetComponentInParent<DialogueRunner>();

        // Скрываем UI при старте, если нужно
        if (hideOnAwake)
            Hide();
    }

    // Показ новой фразы NPC
    public void ShowPhrase(string name, string text)
    {
        // Если уже печатается текст, останавливаем предыдущий корутин
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        ClearChoices(); // Убираем предыдущие кнопки выбора
        gameObject.SetActive(true); // Показываем UI

        phraseText.text = ""; // Очищаем текст
        nameText.text = LocalizationManager.Instance.Get(name); // Локализуем имя NPC

        currentFullText = LocalizationManager.Instance.Get(text); // Локализуем текст фразы
        typeCoroutine = StartCoroutine(TypeText(currentFullText)); // Запускаем эффект печати
    }

    // Корутина печати текста по символам
    IEnumerator TypeText(string text)
    {
        isTyping = true;   // Начало печати
        phraseText.text = ""; // Сбрасываем текст

        yield return new WaitForSeconds(typeSpeed * 2); // Маленькая задержка перед началом

        // Проходим по каждому символу текста
        foreach (char c in text)
        {
            phraseText.text += c;               // Добавляем символ к UI
            yield return new WaitForSeconds(typeSpeed); // Ждём перед следующим
        }

        isTyping = false; // Печать завершена
        typeCoroutine = null;
    }

    // Мгновенное завершение печати текста
    public void CompleteTypingInstantly()
    {
        if (!isTyping) return; // Если текст уже напечатан — выходим

        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine); // Останавливаем корутин
            typeCoroutine = null;
        }

        phraseText.text = currentFullText; // Печатаем полный текст сразу
        isTyping = false;                  // Ставим флаг завершения печати
    }

    // Показ кнопок выбора фразы
    public void ShowChoices(DialogueChoice[] choices)
    {
        ClearChoices();        // Сначала убираем старые кнопки
        phraseText.text = "";  // Скрываем текущий текст фразы

        for (int i = 0; i < choices.Length; i++)
        {
            // Создаём кнопку из префаба
            var obj = Instantiate(choiceButtonPrefab, choicesParent);
            var btn = obj.GetComponent<Button>();
            var txt = obj.GetComponentInChildren<TextMeshProUGUI>();

            // Локализуем текст кнопки
            txt.text = LocalizationManager.Instance.Get(choices[i].text);

            int index = i; // Локальная переменная для замыкания
            btn.onClick.AddListener(() => runner.Choose(index)); // Назначаем обработчик выбора
        }

        phraseText.text = "..."; // Показываем многоточие, пока игрок не выберет
    }

    // Скрытие UI диалога
    public void Hide()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine); // Останавливаем печать текста
            typeCoroutine = null;
        }

        ClearChoices();       // Убираем кнопки выбора
        phraseText.text = ""; // Сбрасываем текст фразы
        nameText.text = "";   // Сбрасываем имя NPC
        gameObject.SetActive(false); // Скрываем UI

        // Разблокируем движение игрока и скрываем курсор
        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var pm = player.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.CanMove = true;
                pm.ShowCursor(false);
            }
        }
    }

    // Принудительное скрытие UI, независимо от состояния
    public void ForceHide()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine); // Останавливаем печать текста
            typeCoroutine = null;
        }

        ClearChoices();       // Убираем кнопки выбора
        phraseText.text = ""; // Сбрасываем текст
        nameText.text = "";   // Сбрасываем имя
        gameObject.SetActive(false);

        var player = GameManager.Instance.GetPlayer(); // Разблокируем игрока
        if (player != null)
        {
            var pm = player.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.CanMove = true;
                pm.ShowCursor(false);
            }
        }
    }

    // Удаление всех кнопок выбора
    void ClearChoices()
    {
        foreach (Transform t in choicesParent)
            Destroy(t.gameObject); // Удаляем каждый дочерний объект
    }
}