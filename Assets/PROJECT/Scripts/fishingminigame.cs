using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class FishingMinigame : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform bar;
    public RectTransform fish, fishChild;
    public RectTransform playerZone;
    public float barMinY = 0f;
    public float barMaxY = 400f;

    [Header("Gameplay Settings")]
    public float fishSpeed = 150f;
    public float zoneSpeed = 200f;
    public float gameDuration = 10f;

    [HideInInspector] public System.Action<bool, FishScriptableObject> OnFinish;

    private float direction;
    private float timer;
    private float inZoneTimer;
    private bool gameActive;

    [SerializeField] string fishResourcesPath = "Fish";

    FishScriptableObject currentFish;

    float currentSpeed;
    float changeTimer;


    public Image progressBar;

    void OnEnable()
    {
        StartGame();
    }

    void Update()
    {
        if (!gameActive) return;

        float delta = Time.deltaTime;

        // Управление зоной игрока
        if (Input.GetKey(KeyCode.W))
            MovePlayerZone(zoneSpeed * delta);
        else if (Input.GetKey(KeyCode.S))
            MovePlayerZone(-zoneSpeed * delta);

        // Движение рыбы
        MoveFish(delta);

        // Проверка попадания рыбы в зону
        if (IsFishInZone())
        {
            fishChild.DOShakePosition(.3f, 1f);
            inZoneTimer += delta;
            if (inZoneTimer >= currentFish.fishingTime)
                Finish(true); // Поймал
        }
        else
        {
            inZoneTimer -= Time.deltaTime * 2.5f;
            inZoneTimer = Mathf.Clamp(inZoneTimer, 0, 10000000);
        }

        // Таймер игры
        timer -= delta;
        progressBar.fillAmount = (inZoneTimer/currentFish.fishingTime);
        if (timer <= 0f)
            Finish(false); // Промах
    }
    void LoadRandomFishByWeight()
    {
        FishScriptableObject[] fishes =
            Resources.LoadAll<FishScriptableObject>(fishResourcesPath);

        if (fishes == null || fishes.Length == 0)
        {
            Debug.LogError("FishingMinigame: в Resources/Fish нет рыб");
            return;
        }

        int totalWeight = 0;
        foreach (var fish in fishes)
            totalWeight += fish.Weight;

        if (totalWeight <= 0)
        {
            Debug.LogError("FishingMinigame: суммарный вес рыб = 0");
            return;
        }

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
        if (fish == null || playerZone == null || bar == null)
        {
            Debug.LogError("FishingMinigame: не назначены все UI элементы!");
            gameActive = false;
            return;
        }

        // Сделать fish и playerZone дочерними bar
        fish.SetParent(bar, false);
        playerZone.SetParent(bar, false);

        // Случайная позиция рыбы
        float fishMinY = barMinY + fish.rect.height / 2f;
        float fishMaxY = barMaxY - fish.rect.height / 2f;
        fish.anchoredPosition = new Vector2(fish.anchoredPosition.x, Random.Range(fishMinY, fishMaxY));

        // PlayerZone в центре bar
        float zoneY = Mathf.Clamp((barMinY + barMaxY) / 2f, barMinY + playerZone.rect.height / 2f, barMaxY - playerZone.rect.height / 2f);
        playerZone.anchoredPosition = new Vector2(playerZone.anchoredPosition.x, zoneY);

        // Случайное направление
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

        // ⏱ таймер хаоса
        changeTimer -= delta;
        if (changeTimer <= 0f)
        {
            direction = Random.value > 0.5f ? 1f : -1f;
            currentSpeed = Random.Range(currentFish.MinSpeed, currentFish.MaxSpeed);
            changeTimer = Random.Range(currentFish.DirectionChangeIntervalMin, currentFish.DirectionChangeIntervalMax);
        }

        float newY = fish.anchoredPosition.y + direction * currentSpeed * delta;

        // 🚧 края
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
        float newY = playerZone.anchoredPosition.y + deltaY;

        float minY = barMinY + playerZone.rect.height / 2f;
        float maxY = barMaxY - playerZone.rect.height / 2f;

        newY = Mathf.Clamp(newY, minY, maxY);
        playerZone.anchoredPosition = new Vector2(playerZone.anchoredPosition.x, newY);
    }

    bool IsFishInZone()
    {
        float fishTop = fish.anchoredPosition.y + fish.rect.height / 2f;
        float fishBottom = fish.anchoredPosition.y - fish.rect.height / 2f;

        float zoneTop = playerZone.anchoredPosition.y + playerZone.rect.height / 2f;
        float zoneBottom = playerZone.anchoredPosition.y - playerZone.rect.height / 2f;

        return fishBottom < zoneTop && fishTop > zoneBottom;
    }

    void Finish(bool success)
    {
        gameActive = false;
        OnFinish?.Invoke(success, currentFish);
        gameObject.SetActive(false);
        Debug.Log("FishingMinigame finished! Success: " + success);
    }
}
