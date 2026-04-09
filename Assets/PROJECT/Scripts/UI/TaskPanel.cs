using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Панель отображения задач игрока: письма и карта с точками игрока и адреса.
/// </summary>
public class TaskPanel : MonoBehaviour
{
    // ===== Singleton для легкого доступа =====
    public static TaskPanel Instance;

    [Header("Left - Letters")]
    public Transform lettersContainer;   // Контейнер для всех писем/передаваемых предметов
    public GameObject letterPrefab;      // Префаб письма/предмета на панели

    [Header("Right - Map")]
    public RectTransform mapRect;        // UI элемент карты
    public RectTransform playerDot;      // Точка игрока на карте
    public RectTransform adressDot;      // Точка цели/адреса на карте

    [Header("Map Bounds (мировые координаты)")]
    public Vector2 mapWorldMin;          // Минимальные координаты мира для карты
    public Vector2 mapWorldMax;          // Максимальные координаты мира для карты

    [Header("UI - Money")]
    public TMP_Text moneyText;           // Отображение денег игрока

    private void Awake()
    {
        // Singleton
        Instance = this;
    }
    
    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Обновление положения игрока и цели на карте каждый кадр
        UpdatePlayerDot();
        UpdateAdressDot();
    }

    /// <summary>
    /// Инициализация панели: спавн писем и предметов (рыбы)
    /// </summary>
    public void Populate()
    {
        SpawnLetters();  // Спавн писем игрока
        SpawnFish();     // Спавн рыб, если есть
    }

    /// <summary>
    /// Очистка панели от всех писем/предметов
    /// </summary>
    public void Clear()
    {
        for (int i = lettersContainer.childCount - 1; i >= 0; i--)
            Destroy(lettersContainer.GetChild(i).gameObject);
    }

    private void OnEnable()
    {
        // Обновление UI при открытии панели
        UpdatePlayerDot();
        UpdateAdressDot();
        UpdateMoney();

        // Скрываем первый элемент списка адресов и кнопку сумки
        FindFirstObjectByType<AdressListMenu>().transform.GetChild(0).gameObject.SetActive(false);
        TaskUI.Instance.bagButton.gameObject.SetActive(false);
        PlayerManager.instance.tabOpenCount++;
    }

    private void OnDisable()
    {
        // Показываем элементы UI обратно
        FindFirstObjectByType<AdressListMenu>().transform.GetChild(0).gameObject.SetActive(true);
        TaskUI.Instance.bagButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Обновление отображения денег игрока
    /// </summary>
    void UpdateMoney()
    {
        moneyText.text = PlayerManager.instance.Money.ToString() + " " + LocalizationManager.Instance.Get("Money");
    }

    // ===== Левый блок: письма =====
    private void SpawnLetters()
    {
        Clear(); // очищаем контейнер

        var mails = PlayerMailInventory.Instance.carriedMails;
        Rect containerRect = ((RectTransform)lettersContainer).rect;
        float padding = 50f;
        float minX = containerRect.xMin + padding;
        float maxX = containerRect.xMax - padding;
        float minY = containerRect.yMin + padding;
        float maxY = containerRect.yMax - padding;

        foreach (var task in mails)
        {
            print("Create tab " + task.recieverName);

            if (task.adress.Contains("Tutorial")) continue; // Пропускаем туториальные письма

            // Создаем письмо
            var go = Instantiate(letterPrefab, lettersContainer, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );
            rect.localScale *= 1.1f;

            // Настройка DeskLetterUI
            var letterUI = go.GetComponent<DeskLetterUI>();
            if (letterUI != null)
            {
                letterUI.recipient = task.recieverName;
                letterUI.address = task.adress;
                letterUI.id = task.id;
                letterUI.isStory = task.isStory;
            }
        }
    }

    // ===== Левый блок: рыбы =====
    public void SpawnFish()
    {
        var fishes = FishInventory.instance.carriedFishes;

        Rect containerRect = ((RectTransform)lettersContainer).rect;
        float padding = 50f;
        float minX = containerRect.xMin + padding;
        float maxX = containerRect.xMax - padding;
        float minY = containerRect.yMin + padding;
        float maxY = containerRect.yMax - padding;

        foreach (var fishCountPair in fishes)
        {
            for (int i = 0; i < fishCountPair.Value; i++)
            {
                var go = Instantiate(letterPrefab, lettersContainer, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY)
                );
                rect.localScale *= 1.5f;

                var letterUI = go.GetComponent<DeskLetterUI>();
                letterUI.SetCustomSprites(fishCountPair.Key.spriteFront, fishCountPair.Key.spriteBack, "", "");
            }
        }
    }

    // ===== Правая карта: позиция игрока =====
    private void UpdatePlayerDot()
    {
        if (playerDot == null || mapRect == null) return;

        var player = PlayerManager.instance?.transform;
        if (player == null) return;

        // Если координаты странные — перемещаем игрока к точке дома
        if (player.position.magnitude > 1000)
            player = GameObject.Find("postmanhouse (1)").transform;

        // Нормализуем координаты по карте
        float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, player.position.x);
        float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, player.position.z);

        Vector2 mapSize = mapRect.rect.size;
        playerDot.anchoredPosition = new Vector2(
            (normX - 0.5f) * mapSize.x - playerDot.rect.width / 2,
            (normY - 0.5f) * mapSize.y
        );
    }

    // ===== Правая карта: позиция цели/адреса =====
    public void UpdateAdressDot()
    {
        bool broke = false;

        // Сначала ищем Story задачи
        foreach (var task in PlayerMailInventory.Instance.carriedMails)
        {
            if (!task.isStory) continue;

            foreach (var mailbox in FindObjectsByType<MailBox>(FindObjectsSortMode.None))
            {
                if (mailbox.mailboxAddress == task.adress)
                {
                    // нормализуем координаты по карте
                    float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, mailbox.transform.position.x);
                    float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, mailbox.transform.position.z);

                    Vector2 mapSize = mapRect.rect.size;
                    adressDot.anchoredPosition = new Vector2(
                        (normX - 0.5f) * mapSize.x,
                        (normY - 0.5f) * mapSize.y
                    );
                    broke = true;
                    break;
                }
            }
            if (broke) break;
        }

        // Если Story не найдено — ищем обычные письма
        if (!broke)
        {
            foreach (var task in PlayerMailInventory.Instance.carriedMails)
            {
                foreach (var mailbox in FindObjectsByType<MailBox>(FindObjectsSortMode.None))
                {
                    if (mailbox.mailboxAddress == task.adress)
                    {
                        float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, mailbox.transform.position.x);
                        float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, mailbox.transform.position.z);

                        Vector2 mapSize = mapRect.rect.size;
                        adressDot.anchoredPosition = new Vector2(
                            (normX - 0.5f) * mapSize.x - 10,
                            (normY - 0.5f) * mapSize.y + adressDot.rect.height * 0.5f - 10
                        );
                        broke = true;
                        break;
                    }
                }
                if (!broke) { adressDot.anchoredPosition = new Vector2(1000, 1000); break; }
            }
        }
    }
}