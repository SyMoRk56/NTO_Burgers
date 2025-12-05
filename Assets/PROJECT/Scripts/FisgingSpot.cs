using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FishingSpot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fishingRod;
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private Transform fishSpawnPoint;
    [SerializeField] private Text cooldownText; // UI текст для отображения кулдауна

    [Header("Настройки удочки")]
    [SerializeField] private Vector3 rodStartRotation = new Vector3(0f, 0f, 20f);
    [SerializeField] private Vector3 rodStartPosition = Vector3.zero;
    [SerializeField] private Vector3 rodCastRotation = new Vector3(-30f, 0f, -15f);
    [SerializeField] private Vector3 rodCastPosition = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private Vector3 rodIdleRotation = new Vector3(-45f, 0f, 0f);
    [SerializeField] private Vector3 rodIdlePosition = new Vector3(0f, -0.05f, 0f);
    [SerializeField] private float rodAnimationDuration = 0.3f;

    [Header("Настройки рыбы")]
    [SerializeField] private float fishFlyDuration = 2f;
    [SerializeField] private float fishSpawnDelay = 0.5f;
    [SerializeField] private float minFishSize = 0.2f; // Минимальный размер рыбы
    [SerializeField] private float maxFishSize = 3f;   // Максимальный размер рыбы
    [SerializeField] private float maxGrowthTime = 10f; // Время для достижения максимального размера

    [Header("Настройки кулдауна")]
    [SerializeField] private float cooldownDuration = 30f; // Кулдаун в секундах
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.red;

    // Состояния
    private bool isPlayerInRange = false;
    private bool isFishingActive = false;
    private bool isRodCast = false;
    private bool isOnCooldown = false;
    private GameObject currentPlayer;

    // Таймеры
    private float fishingStartTime; // Время начала рыбалки (заброса)
    private float cooldownEndTime; // Время окончания кулдауна
    private float fishGrowthTime; // Время роста рыбы

    // Компоненты игрока
    private playerAnimations playerAnim;
    private PlayerManager playerManager;
    private CharacterController characterController;
    private Transform playerModel;

    // Ссылка на корутину анимации удочки
    private Coroutine rodAnimationCoroutine;

    void Start()
    {
        // Автоматически находим удочку по тегу
        if (fishingRod == null)
        {
            fishingRod = FindChildWithTag(transform, "fishrod");
        }

        // Устанавливаем удочку в начальное положение
        if (fishingRod != null)
        {
            fishingRod.localEulerAngles = rodStartRotation;
            fishingRod.localPosition = rodStartPosition;
        }

        // Скрываем текст кулдауна
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isOnCooldown)
        {
            UpdateCooldownUI();
        }

        if (!isPlayerInRange || currentPlayer == null) return;

        // Если кулдаун активен, не разрешаем начинать новую рыбалку
        if (isOnCooldown && !isFishingActive)
        {
            Debug.Log("Кулдаун активен, подождите");
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isFishingActive)
            {
                StartFishing();
            }
            else if (!isRodCast)
            {
                CastRod();
            }
            else
            {
                ReelRod();
            }
        }
    }

    private void StartFishing()
    {
        // Получаем компоненты
        playerAnim = currentPlayer.GetComponent<playerAnimations>();
        playerManager = currentPlayer.GetComponent<PlayerManager>();
        characterController = currentPlayer.GetComponent<CharacterController>();

        // Находим модель игрока по тегу
        playerModel = FindChildWithTag(currentPlayer.transform, "model");

        if (playerAnim == null)
        {
            Debug.LogError("Нет компонента playerAnimations!");
            return;
        }

        if (playerManager == null)
        {
            Debug.LogError("Нет компонента PlayerManager!");
            return;
        }

        // 1. Блокируем движение
        playerManager.CanMove = false;

        // 2. Отключаем CharacterController
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // 3. Позиционируем игрока
        currentPlayer.transform.position = transform.position;
        currentPlayer.transform.Rotate(0, -100, 0, Space.World);
        playerModel.transform.Rotate(0, -100, 0, Space.World);


        //4.Поворачиваем модель игрока
        //if (playerModel != null)
        //{
        //    playerModel.rotation = transform.rotation;
        //}
        //else
        //{
        //    currentPlayer.transform.rotation = transform.rotation;
        //}

        // 5. Устанавливаем удочку в начальное положение
        if (fishingRod != null)
        {
            if (rodAnimationCoroutine != null)
                StopCoroutine(rodAnimationCoroutine);

            rodAnimationCoroutine = StartCoroutine(AnimateRod(
                rodStartRotation,
                rodStartPosition,
                rodAnimationDuration
            ));
        }

        // 6. Включаем анимацию fishing_bros
        playerAnim.StartFishing();

        isFishingActive = true;
        isRodCast = false;

        Debug.Log("Рыбалка начата! Нажми E для заброса");
    }

    private void CastRod()
    {
        // Запоминаем время начала рыбалки (для расчета размера рыбы)
        fishingStartTime = Time.time;

        // 1. Анимируем удочку - плавный заброс
        if (fishingRod != null)
        {
            if (rodAnimationCoroutine != null)
                StopCoroutine(rodAnimationCoroutine);

            rodAnimationCoroutine = StartCoroutine(AnimateRod(
                rodCastRotation,
                rodCastPosition,
                rodAnimationDuration
            ));
        }

        // 2. Меняем анимацию игрока на fishing_idle
        if (playerAnim != null)
        {
            playerAnim.FishingIdle();
        }

        isRodCast = true;
        Debug.Log("Удочка заброшена! Нажми E чтобы вытащить");
    }

    private void ReelRod()
    {
        // Вычисляем время, которое удочка была заброшена
        fishGrowthTime = Time.time - fishingStartTime;

        // Ограничиваем время роста максимальным значением
        if (fishGrowthTime > maxGrowthTime)
            fishGrowthTime = maxGrowthTime;

        Debug.Log($"Рыба росла {fishGrowthTime:F1} секунд");

        // 1. Возвращаем удочку в начальное положение
        if (fishingRod != null)
        {
            if (rodAnimationCoroutine != null)
                StopCoroutine(rodAnimationCoroutine);

            rodAnimationCoroutine = StartCoroutine(AnimateRod(
                rodStartRotation,
                rodStartPosition,
                rodAnimationDuration
            ));
        }

        // 2. Возвращаем анимацию fishing_bros
        if (playerAnim != null)
        {
            playerAnim.StartFishing();
        }

        // 3. Создаем рыбу с задержкой, передавая время роста
        if (fishPrefab != null && fishSpawnPoint != null)
        {
            Invoke(nameof(SpawnFish), fishSpawnDelay);
        }

        // 4. Завершаем рыбалку
        Invoke(nameof(EndFishing), fishSpawnDelay + 1f);

        isRodCast = false;
    }

    private void SpawnFish()
    {
        if (fishPrefab != null && fishSpawnPoint != null)
        {
            GameObject fish = Instantiate(fishPrefab, fishSpawnPoint.position, Quaternion.identity);

            // Рассчитываем размер рыбы на основе времени роста
            float fishSize = CalculateFishSize(fishGrowthTime);
            fish.transform.localScale = Vector3.one * fishSize;

            Debug.Log($"Размер рыбы: {fishSize:F2}x");

            StartCoroutine(FlyFish(fish, fishSize));
            Destroy(fish, 5f);
        }
    }

    private float CalculateFishSize(float growthTime)
    {
        // Рассчитываем размер от minFishSize до maxFishSize в зависимости от времени
        // Линейная интерполяция: размер = min + (growthTime/maxGrowthTime) * (max-min)
        float normalizedTime = Mathf.Clamp01(growthTime / maxGrowthTime);
        float size = minFishSize + normalizedTime * (maxFishSize - minFishSize);

        // Можно добавить небольшую случайность (например, ±10%)
        float randomFactor = Random.Range(0.9f, 1.1f);
        return size * randomFactor;
    }

    private IEnumerator FlyFish(GameObject fish, float fishSize)
    {
        float time = 0f;
        Vector3 startPos = fish.transform.position;

        // Цель - немного перед игроком
        Vector3 targetPos = currentPlayer.transform.position +
                           (playerModel != null ? playerModel.forward : currentPlayer.transform.forward) * 1f +
                           Vector3.up * 1f;

        while (time < fishFlyDuration)
        {
            if (fish == null) yield break;

            float t = time / fishFlyDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            fish.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            // Поворачиваем рыбу в направлении движения
            if (time > 0.1f)
            {
                Vector3 direction = (targetPos - fish.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    fish.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Гарантируем конечную позицию
        if (fish != null)
        {
            fish.transform.position = targetPos;
        }
    }

    private void EndFishing()
    {
        // 1. Разрешаем движение
        if (playerManager != null)
        {
            playerManager.CanMove = true;
        }

        // 2. Включаем CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // 3. Выключаем анимацию рыбалки
        if (playerAnim != null)
        {
            playerAnim.EndFishing();
        }

        // 4. Сбрасываем состояния
        isFishingActive = false;
        isRodCast = false;

        // 5. Запускаем кулдаун
        StartCooldown();

        Debug.Log("Рыбалка завершена! Кулдаун 30 секунд.");
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Time.time + cooldownDuration;

        // Показываем UI кулдауна
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = cooldownColor;
        }

        // Запускаем проверку окончания кулдауна
        StartCoroutine(CheckCooldown());
    }

    private IEnumerator CheckCooldown()
    {
        while (isOnCooldown && Time.time < cooldownEndTime)
        {
            UpdateCooldownUI();
            yield return new WaitForSeconds(1f);
        }

        // Кулдаун закончился
        isOnCooldown = false;

        // Скрываем UI кулдауна
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }

        Debug.Log("Кулдаун завершен! Можно рыбачить снова.");
    }

    private void UpdateCooldownUI()
    {
        if (cooldownText == null || !isOnCooldown) return;

        float timeLeft = cooldownEndTime - Time.time;

        if (timeLeft <= 0)
        {
            cooldownText.text = "Можно рыбачить!";
            cooldownText.color = activeColor;
        }
        else
        {
            int secondsLeft = Mathf.CeilToInt(timeLeft);
            cooldownText.text = $"До следующей рыбалки: {secondsLeft} сек.";
            cooldownText.color = cooldownColor;
        }
    }

    // Корутина для плавной анимации удочки
    private IEnumerator AnimateRod(Vector3 targetRotation, Vector3 targetPosition, float duration)
    {
        if (fishingRod == null) yield break;

        Vector3 startRotation = fishingRod.localEulerAngles;
        Vector3 startPosition = fishingRod.localPosition;

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            fishingRod.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, smoothT);
            fishingRod.localPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        fishingRod.localEulerAngles = targetRotation;
        fishingRod.localPosition = targetPosition;
        rodAnimationCoroutine = null;
    }

    private void CancelFishing()
    {
        if (isFishingActive)
        {
            // Останавливаем все корутины
            if (rodAnimationCoroutine != null)
            {
                StopCoroutine(rodAnimationCoroutine);
                rodAnimationCoroutine = null;
            }

            CancelInvoke(nameof(EndFishing));
            CancelInvoke(nameof(SpawnFish));
            EndFishing();
        }
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child;

            Transform result = FindChildWithTag(child, tag);
            if (result != null)
                return result;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayer = other.gameObject;

            // Показываем UI, если активен кулдаун
            if (isOnCooldown && cooldownText != null)
            {
                cooldownText.gameObject.SetActive(true);
            }

            Debug.Log("Игрок в зоне рыбалки");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Скрываем UI кулдауна при выходе из зоны
            if (cooldownText != null && !isFishingActive)
            {
                cooldownText.gameObject.SetActive(false);
            }

            if (isFishingActive)
            {
                CancelFishing();
            }

            currentPlayer = null;
        }
    }

    private void OnDestroy()
    {
        CancelFishing();
    }

    // Отладка в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetComponent<Collider>().bounds.extents.magnitude);

        if (fishSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(fishSpawnPoint.position, 0.1f);
            Gizmos.DrawWireSphere(fishSpawnPoint.position, 0.3f);
        }
    }
}