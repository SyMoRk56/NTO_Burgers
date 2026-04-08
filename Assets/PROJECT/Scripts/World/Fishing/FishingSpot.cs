using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FishingSpot : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        // Проверяем дистанцию через UI-интеракцию
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    [Header("References")]
    public Transform fishingRod;          // Удочка (модель)
    public GameObject fishPrefab;         // Префаб рыбы (не используется напрямую)
    public Transform fishSpawnPoint;      // Точка появления рыбы
    public FishingMinigame fishingMinigame; // Ссылка на мини-игру
    public GameObject fishingCanvas;      // Канвас мини-игры
    public TMPro.TMP_Text cooldownText;   // Текст кулдауна

    [Header("Rod Settings")]
    public Vector3 rodStartRotation;      // Исходный поворот удочки
    public Vector3 rodStartPosition;      // Исходная позиция удочки
    public Vector3 rodCastRotation;       // Поворот при забросе
    public Vector3 rodCastPosition;       // Позиция при забросе
    public float rodAnimationDuration = 0.3f; // Длительность анимации

    [Header("Fish Settings")]
    public float fishFlyDuration = 2f;    // Время полёта рыбы к игроку
    public float minFishSize = 0.2f;      // Минимальный размер
    public float maxFishSize = 3f;        // Максимальный размер

    [Header("Cooldown")]
    public float cooldownDuration = 30f;  // Длительность кулдауна
    public Color activeColor = Color.white;   // Цвет когда можно ловить
    public Color cooldownColor = Color.red;   // Цвет во время кулдауна

    private bool isPlayerInRange = false; // Игрок рядом
    private bool isFishingActive = false; // Рыбалка идёт
    private bool isRodCast = false;       // Удочка заброшена
    private bool isOnCooldown = false;    // Сейчас кулдаун
    private float cooldownEndTime;        // Когда закончится кулдаун
    private GameObject currentPlayer;     // Игрок
    private Coroutine rodAnimationCoroutine; // Текущая анимация удочки

    [HideInInspector]
    public bool isFishingEnding = false; // Флаг окончания (для NPC)

    void Start()
    {
        // Получаем игрока из менеджера
        currentPlayer = PlayerManager.instance.gameObject;
    }

    void Update()
    {
        // Обновляем UI кулдауна
        if (isOnCooldown)
            UpdateCooldownUI();

        // Если игрока нет или он далеко — выходим
        if (!isPlayerInRange || currentPlayer == null) return;

        return; // (похоже, тут намеренно отключен ввод)

        if (Input.GetKeyDown(KeyCode.E))
        {
            Fish(); // Начать рыбалку
            return;
        }
    }

    private void Fish()
    {
        // Если ещё не начали — запускаем
        if (!isFishingActive)
        {
            StartFishing();
            CastRod();
        }

        //else if (!isRodCast)

    }

    void StartFishing()
    {
        // Блокируем движение игрока
        currentPlayer.GetComponent<PlayerManager>().CanMove = false;

        // Сбрасываем удочку в стартовое положение
        fishingRod.localPosition = rodStartPosition;
        fishingRod.localEulerAngles = rodStartRotation;

        isFishingActive = true;
        isRodCast = false;

        Debug.Log("Рыбалка начата! Нажмите E для заброса.");
    }

    void CastRod()
    {
        // Включаем UI мини-игры
        fishingCanvas.SetActive(true);
        fishingMinigame.gameObject.SetActive(true);

        // Подписываемся на результат мини-игры
        fishingMinigame.OnFinish += OnMinigameFinished;

        // Анимируем заброс удочки
        rodAnimationCoroutine = StartCoroutine(AnimateRod(rodCastRotation, rodCastPosition, rodAnimationDuration));

        isRodCast = true;
        Debug.Log("Удочка заброшена! Мини-игра началась.");
    }

    void OnMinigameFinished(bool success, FishScriptableObject fish)
    {
        // Выключаем UI
        fishingCanvas.SetActive(false);

        // Если успех — спавним рыбу
        if (success)
        {
            SpawnFish(fish.prefab);
        }

        EndFishing();
    }

    void SpawnFish(GameObject fishPrefab)
    {
        // Проверка на null
        if (fishPrefab == null || fishSpawnPoint == null) return;

        // Создаём рыбу
        GameObject fish = Instantiate(fishPrefab, fishSpawnPoint.position, Quaternion.identity);

        // Случайный размер
        float size = Random.Range(minFishSize, maxFishSize);
        fish.transform.localScale = Vector3.one * size;

        // Запускаем полёт к игроку
        StartCoroutine(FlyFish(fish));

        //Destroy(fish, 20f);

        Debug.Log($"Рыба поймана! Размер: {size:F2}");

        // Отписываемся от события
        fishingMinigame.OnFinish -= OnMinigameFinished;
    }

    IEnumerator FlyFish(GameObject fish)
    {
        Vector3 startPos = fish.transform.position;

        // Точка возле игрока (чуть выше)
        Vector3 targetPos = currentPlayer.transform.position + Vector3.up * 1.5f;

        float time = 0f;

        // Плавный перелёт
        while (time < fishFlyDuration)
        {
            fish.transform.position = Vector3.Lerp(startPos, targetPos, time / fishFlyDuration);
            time += Time.deltaTime;
            yield return null;
        }

        // Финальная позиция
        fish.transform.position = targetPos;
    }

    void EndFishing()
    {
        // Возвращаем управление игроку
        currentPlayer.GetComponent<PlayerManager>().CanMove = true;

        isFishingActive = false;
        isRodCast = false;

        isFishingEnding = true; // сигнал для NPC

        // Запускаем кулдаун
        StartCooldown();

        // Возвращаем удочку назад
        rodAnimationCoroutine = StartCoroutine(AnimateRod(rodStartRotation, rodStartPosition, rodAnimationDuration));

        Debug.Log("Рыбалка завершена! Кулдаун активен.");

        // Сбрасываем флаг через секунду
        StartCoroutine(ResetFishingEndingAfterDelay(1f));
    }

    IEnumerator ResetFishingEndingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isFishingEnding = false;
    }

    IEnumerator AnimateRod(Vector3 targetRotation, Vector3 targetPosition, float duration)
    {
        Vector3 startRot = fishingRod.localEulerAngles;
        Vector3 startPos = fishingRod.localPosition;

        float time = 0f;

        // Плавная анимация позиции и поворота
        while (time < duration)
        {
            fishingRod.localEulerAngles = Vector3.Lerp(startRot, targetRotation, time / duration);
            fishingRod.localPosition = Vector3.Lerp(startPos, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Финальное значение
        fishingRod.localEulerAngles = targetRotation;
        fishingRod.localPosition = targetPosition;
    }

    void StartCooldown()
    {
        isOnCooldown = true;

        // Запоминаем время окончания
        cooldownEndTime = Time.time + cooldownDuration;

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = cooldownColor;
        }

        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        // Пока кулдаун не закончился — обновляем UI
        while (Time.time < cooldownEndTime)
        {
            UpdateCooldownUI();
            yield return new WaitForSeconds(0.2f);
        }

        isOnCooldown = false;

        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }

    void UpdateCooldownUI()
    {
        if (cooldownText == null) return;

        float timeLeft = Mathf.Clamp(cooldownEndTime - Time.time, 0f, cooldownDuration);

        // Если кулдаун закончился
        if (timeLeft <= 0f)
        {
            cooldownText.text = "Можно рыбачить!";
            cooldownText.color = activeColor;
        }
        else
        {
            // Показываем секунды
            cooldownText.text = $"{Mathf.CeilToInt(timeLeft)}";
            cooldownText.color = cooldownColor;
        }
    }



    public int InteractPriority()
    {
        return 0;
    }

    public bool CheckInteract()
    {
        // Можно взаимодействовать если не ловим и нет кулдауна
        return (!isRodCast || !isFishingActive) && !isOnCooldown;
    }

    public void Interact()
    {
        Fish(); // Основной вход
    }

    public void OnBeginInteract()
    {

    }

    public void OnEndInteract(bool success)
    {

    }
}