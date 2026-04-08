using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public class FishingMinigame : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform bar;          // Основная шкала (границы движения)
    public RectTransform fish, fishChild; // Рыба и её визуал (для тряски)
    public RectTransform playerZone;   // Зона игрока
    public float barMinY = 0f;
    public float barMaxY = 400f;

    [Header("Gameplay Settings")]
    public float fishSpeed = 150f;
    public float zoneSpeed = 200f;
    public float gameDuration = 10f;   // Сколько длится мини-игра

    [HideInInspector] public System.Action<bool, FishScriptableObject> OnFinish;

    private float direction;   // Направление движения рыбы
    private float timer;       // Таймер игры
    private float inZoneTimer; // Сколько времени рыба в зоне
    private bool gameActive;

    [SerializeField] string fishResourcesPath = "Fish";

    FishScriptableObject currentFish;

    float currentSpeed;  // Текущая скорость рыбы
    float changeTimer;   // Таймер смены направления

    public Image progressBar; // Прогресс поимки

    float noMoveTimer;   // Время без движения игрока

    public GameObject tutorPanel; // Подсказка (если стоишь AFK)

    void OnEnable()
    {
        // Подгоняем границы с учётом размера зоны
        float halfZoneHeight = playerZone.rect.height / 2f;

        barMinY = bar.rect.yMin + halfZoneHeight - 50;
        barMaxY = bar.rect.yMax - halfZoneHeight + 50;

        StartGame();
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        // Лёгкая тряска рыбы, когда она в зоне (визуальный фидбек)
        while (true)
        {
            if (IsFishInZone())
            {
                fishChild.DOShakePosition(.3f, 12f);
            }
            yield return new WaitForSeconds(.3f);
        }
    }

    void Update()
    {
        if (!gameActive) return;

        float delta = Time.deltaTime;

        // Управление зоной игрока (W/S)
        if (Input.GetKey(KeyCode.W))
            MovePlayerZone(zoneSpeed * delta);
        else if (Input.GetKey(KeyCode.S))
            MovePlayerZone(-zoneSpeed * delta);
        else
        {
            noMoveTimer += Time.deltaTime;
        }

        // Если долго не двигаешься — показываем подсказку
        if (noMoveTimer > 5)
            tutorPanel.SetActive(true);
        else
            tutorPanel.SetActive(false);

        // Двигаем рыбу
        MoveFish(delta);

        // Проверяем, попала ли рыба в зону
        if (IsFishInZone())
        {
            inZoneTimer += delta;

            // Если достаточно удержал — победа
            if (inZoneTimer >= currentFish.fishingTime)
                Finish(true);
        }
        else
        {
            // Быстро теряем прогресс, если промазал
            inZoneTimer -= Time.deltaTime * 2.5f;
            inZoneTimer = Mathf.Clamp(inZoneTimer, 0, 10000000);
        }

        // Обновляем таймер игры
        timer -= delta;

        float t = inZoneTimer / currentFish.fishingTime;
        t = Mathf.Clamp01(t);

        // Немного сглаживаем прогресс (чтобы выглядел приятнее)
        float smooth;
        if (t < 0.5f)
            smooth = 2f * t * t;
        else
            smooth = 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        progressBar.fillAmount = t;

        // Время вышло — проигрыш
        if (timer <= 0f)
            Finish(false);
    }

    void LoadRandomFishByWeight()
    {
        // Загружаем всех рыб из Resources
        FishScriptableObject[] fishes =
            Resources.LoadAll<FishScriptableObject>(fishResourcesPath);

        if (fishes == null || fishes.Length == 0)
        {
            Debug.LogError("FishingMinigame: в Resources/Fish нет рыб");
            return;
        }

        // Считаем общий вес
        int totalWeight = 0;
        foreach (var fish in fishes)
            totalWeight += fish.Weight;

        if (totalWeight <= 0)
        {
            Debug.LogError("FishingMinigame: суммарный вес рыб = 0");
            return;
        }

        // Рандом с учётом веса (чем больше Weight — тем выше шанс)
        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var fish in fishes)
        {
            current += fish.Weight;
            if (roll < current)
            {
                currentFish = fish;
                return;
            }
        }
    }

    void StartGame()
    {
        LoadRandomFishByWeight();

        // Проверка на забытые ссылки
        if (fish == null || playerZone == null || bar == null)
        {
            Debug.LogError("FishingMinigame: не назначены все UI элементы!");
            gameActive = false;
            return;
        }

        // Привязываем элементы к bar
        fish.SetParent(bar, false);
        playerZone.SetParent(bar, false);

        // Ставим рыбу в случайное место
        float fishMinY = barMinY + fish.rect.height / 2f;
        float fishMaxY = barMaxY - fish.rect.height / 2f;
        fish.anchoredPosition = new Vector2(fish.anchoredPosition.x, Random.Range(fishMinY, fishMaxY));

        // Зона игрока по центру
        float zoneY = Mathf.Clamp((barMinY + barMaxY) / 2f,
            barMinY + playerZone.rect.height / 2f,
            barMaxY - playerZone.rect.height / 2f);

        playerZone.anchoredPosition = new Vector2(playerZone.anchoredPosition.x, zoneY);

        // Случайное направление старта
        direction = Random.value > 0.5f ? 1f : -1f;

        timer = gameDuration;
        inZoneTimer = 0f;
        gameActive = true;
    }

    void MoveFish(float delta)
    {
        if (!gameActive || fish == null) return;

        float minY = barMinY + fish.rect.height / 2f;
        float maxY = barMaxY - fish.rect.height / 2f;

        // Таймер смены поведения (хаотичность)
        changeTimer -= delta;
        if (changeTimer <= 0f)
        {
            direction = Random.value > 0.5f ? 1f : -1f;
            currentSpeed = Random.Range(currentFish.MinSpeed, currentFish.MaxSpeed);

            changeTimer = Random.Range(
                currentFish.DirectionChangeIntervalMin,
                currentFish.DirectionChangeIntervalMax);
        }

        float newY = fish.anchoredPosition.y + direction * currentSpeed * delta;

        // Отбиваемся от краёв
        if (newY > maxY)
        {
            newY = maxY;
            direction = -1f;
        }
        else if (newY < minY)
        {
            newY = minY;
            direction = 1f;
        }

        fish.anchoredPosition = new Vector2(fish.anchoredPosition.x, newY);
    }

    void MovePlayerZone(float deltaY)
    {
        noMoveTimer = 0; // игрок двигается → сбрасываем AFK

        float newY = playerZone.anchoredPosition.y + deltaY;

        float minY = barMinY + playerZone.rect.height / 2f;
        float maxY = barMaxY - playerZone.rect.height / 2f;

        newY = Mathf.Clamp(newY, minY, maxY);
        playerZone.anchoredPosition = new Vector2(playerZone.anchoredPosition.x, newY);
    }

    bool IsFishInZone()
    {
        // Простая проверка пересечения по Y
        float fishTop = fish.anchoredPosition.y + fish.rect.height / 4f;
        float fishBottom = fish.anchoredPosition.y - fish.rect.height / 4f;

        float zoneTop = playerZone.anchoredPosition.y + playerZone.rect.height / 4f;
        float zoneBottom = playerZone.anchoredPosition.y - playerZone.rect.height / 4f;

        return fishBottom < zoneTop && fishTop > zoneBottom;
    }

    void Finish(bool success)
    {
        gameActive = false;

        // Сообщаем результат наружу
        OnFinish?.Invoke(success, currentFish);

        gameObject.SetActive(false);

        Debug.Log("FishingMinigame finished! Success: " + success);
    }
}