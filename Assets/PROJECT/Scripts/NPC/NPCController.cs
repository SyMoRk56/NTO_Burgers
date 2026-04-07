using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    [Header("Настройки рыбалки")]
    [SerializeField] private MonoBehaviour componentToDisable; 
    [SerializeField] private string bridgeTag = "bridge";      
    [SerializeField] private string fishTag = "fish";        

    [Header("Настройки движения")]
    [SerializeField] private float stoppingDistance = 1.0f;   
    [SerializeField] private float updateTargetInterval = 1.0f;// Интервал обновления пути к рыбе (чтобы не делать это каждый кадр)
    [SerializeField] private float attachFishDistance = 2.0f;  
    [SerializeField] private float collisionCheckRadius = 2.0f;// Радиус сферы для проверки коллизий с мостом

    [Header("Настройки рыбы")]
    [SerializeField] private Vector3 fishCarryOffset = new Vector3(0f, 1.5f, 1.5f);  
    [SerializeField] private Vector3 fishRotationOffset = new Vector3(0f, 90f, 0f);  // Дополнительный поворот рыбы при переноске
    [SerializeField] private float fishFollowSpeed = 5f;      

    [Header("Настройки анимации")]
    [SerializeField] private Animator npcAnimator;             // Ссылка на Animator-компонент для управления анимациями
    [SerializeField] private string carringWalkParam = "carringwalk"; // Имя параметра в Animator, управляющего состоянием ходьбы с рыбой

    [Header("VFX эффекты - ПРОСТОЙ ВАРИАНТ")]
    [SerializeField] private bool enableVFX = true;            // Переключатель: включать ли визуальные эффекты при подборе рыбы
    [SerializeField] private string vfxTag = "PickupVFX";     
    [SerializeField] private float vfxDuration = 3f;           // Длительность проигрывания VFX-эффекта в секундах

    public bool isGoingForFish = false; 

    private FishingSpot fishingManager;  // Ссылка на менеджер рыбалки (контролирует состояние сессии)
    private NavMeshAgent navMeshAgent;  
    private GameObject targetFish;       // Текущая цель — рыба, к которой движется НПЦ
    private bool isMovingToFish = false; 
    private Coroutine movementCoroutine; // Ссылка на корутину движения (чтобы можно было остановить)
    private bool hasFishAttached = false
    private GameObject attachedFish;     
    private bool isCarryingFish = false;
    private List<GameObject> foundVFX = new List<GameObject>(); // Кэш: список найденных VFX-объектов при старте

    void Start()
    {
        Debug.Log($"NPCController.Start() вызван для {gameObject.name}");

        fishingManager = FindObjectOfType<FishingSpot>();

        if (fishingManager == null)
        {
            Debug.LogWarning("FishingManager не найден в сцене!");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.autoBraking = true;

        if (componentToDisable == null)
        {
            Debug.LogWarning("Не назначен компонент для отключения!");
        }

        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }

        // Ищем VFX объекты сразу при старте — чтобы не делать поиск каждый раз при подборе
        FindVFXObjects();

        Debug.Log($"NPCController инициализирован. VFX включен: {enableVFX}, тег: '{vfxTag}'");
    }

    // Метод для поиска VFX объектов по тегу и их кэширования
    void FindVFXObjects()
    {
        if (!enableVFX) return; // Если эффекты выключены — не тратим время на поиск

        GameObject[] vfxArray = GameObject.FindGameObjectsWithTag(vfxTag);
        foundVFX.Clear();

        foreach (GameObject vfx in vfxArray)
        {
            if (vfx != null)
            {
                foundVFX.Add(vfx);
                Debug.Log($"Найден VFX объект: {vfx.name}");

                // Сразу выключаем — включим только в момент подбора рыбы
                vfx.SetActive(false);

                // Проверяем, есть ли ParticleSystem (на самом объекте или в детях)
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps == null) ps = vfx.GetComponentInChildren<ParticleSystem>();

                if (ps != null)
                {
                    Debug.Log($"  ParticleSystem найден: {ps.name}");
                }
                else
                {
                    Debug.LogWarning($"  ParticleSystem не найден на {vfx.name}!");
                }
            }
        }

        if (foundVFX.Count == 0)
        {
            Debug.LogError($"НЕ НАЙДЕНО объектов с тегом '{vfxTag}' в сцене!");
            Debug.LogError("Создайте объекты с ParticleSystem и назначьте им тег '" + vfxTag + "'");
        }
        else
        {
            Debug.Log($"Всего найдено VFX объектов: {foundVFX.Count}");
        }
    }

    void Update()
    {
        // Если несём рыбу — обновляем её позицию и поворот относительно НПЦ
        if (isCarryingFish && attachedFish != null)
        {
            UpdateFishPosition();
        }

        // Обновляем параметры аниматора в зависимости от состояния
        UpdateAnimation();

        // Если рыбалка завершается — проверяем, не вошёл ли НПЦ в зону моста
        if (fishingManager != null && fishingManager.isFishingEnding)
        {
            CheckForBridgeContinuously();
        }
    }

    // Плавное перемещение рыбы к позиции "руки" НПЦ (чтобы не было телепортаций и дёрганий)
    void UpdateFishPosition()
    {
        if (attachedFish == null) return;

        // Вычисляем целевую позицию: вперёд + вверх + вправо от НПЦ
        Vector3 targetPosition = transform.position +
                                transform.forward * fishCarryOffset.z +
                                transform.up * fishCarryOffset.y +
                                transform.right * fishCarryOffset.x;

        // Плавное приближение к цели (Lerp)
        attachedFish.transform.position = Vector3.Lerp(
            attachedFish.transform.position,
            targetPosition,
            Time.deltaTime * fishFollowSpeed
        );

        // Вычисляем целевой поворот: поворот НПЦ + дополнительный оффсет
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(fishRotationOffset);
        // Плавный поворот (Slerp)
        attachedFish.transform.rotation = Quaternion.Slerp(
            attachedFish.transform.rotation,
            targetRotation,
            Time.deltaTime * fishFollowSpeed
        );
    }

    // Обновление параметров анимации: обычная ходьба / ходьба с рыбой / остановка
    void UpdateAnimation()
    {
        if (npcAnimator == null) return;

        int carringWalkValue = 0;

        if (isCarryingFish)
        {
            carringWalkValue = 4; // Анимация с рыбой
        }
        else if (isMovingToFish)
        {
            carringWalkValue = 1; // Анимация движения к цели
        }

        npcAnimator.SetInteger(carringWalkParam, carringWalkValue);

        // Дополнительно передаём скорость движения для более плавной анимации
        if (navMeshAgent != null)
        {
            float speed = navMeshAgent.velocity.magnitude;
            npcAnimator.SetFloat("Speed", speed);
        }
    }

    // Проверка: не вошёл ли НПЦ в радиус моста (триггер начала движения к рыбе)
    void CheckForBridgeContinuously()
    {
        // Если уже движемся к рыбе или несём её — проверка не нужна
        if (isMovingToFish || isCarryingFish) return;

        // Проверяем коллизии в радиусе вокруг НПЦ
        Collider[] colliders = Physics.OverlapSphere(transform.position, collisionCheckRadius);

        foreach (Collider collider in colliders)
        {
            // Ищем объект с тегом моста, исключая самого НПЦ
            if (collider.gameObject != gameObject && collider.gameObject.tag == bridgeTag)
            {
                // Отключаем компонент, если он был включён (например, патрулирование)
                if (componentToDisable != null && componentToDisable.enabled)
                {
                    componentToDisable.enabled = false;
                }

                // Запускаем движение к рыбе, если ещё не начали
                if (!isMovingToFish)
                {
                    StartMovingToFish();
                }
                break; // Нашли мост — выходим из цикла
            }
        }
    }

    // Поиск ближайшей активной рыбы и запуск движения к ней
    void StartMovingToFish()
    {
        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag(fishTag);

        if (fishObjects.Length > 0)
        {
            // Фильтруем только активные в иерархии объекты (не деактивированные в редакторе/коде)
            List<GameObject> activeFish = new List<GameObject>();
            foreach (GameObject fish in fishObjects)
            {
                if (fish != null && fish.activeInHierarchy)
                {
                    activeFish.Add(fish);
                }
            }

            if (activeFish.Count > 0)
            {
                // Берём первую как временную цель
                targetFish = activeFish[0];
                float closestDistance = Vector3.Distance(transform.position, targetFish.transform.position);

                // Ищем самую близкую рыбу из активных
                foreach (GameObject fish in activeFish)
                {
                    float distance = Vector3.Distance(transform.position, fish.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetFish = fish;
                    }
                }

                isMovingToFish = true;
                isGoingForFish = true;

                // Если уже есть запущенная корутина — останавливаем её перед запуском новой
                if (movementCoroutine != null)
                {
                    StopCoroutine(movementCoroutine);
                }

                movementCoroutine = StartCoroutine(MoveToFishCoroutine());
            }
        }
    }

    // Корутина: периодическое обновление пути к рыбе и проверка дистанции
    IEnumerator MoveToFishCoroutine()
    {
        while (isMovingToFish && targetFish != null)
        {
            if (navMeshAgent.isActiveAndEnabled && targetFish.activeInHierarchy)
            {
                // Устанавливаем новую точку назначения для агента
                navMeshAgent.SetDestination(targetFish.transform.position);

                // Проверяем дистанцию до рыбы
                float distanceToFish = Vector3.Distance(transform.position, targetFish.transform.position);

                // Если подошли достаточно близко и ещё не несём рыбу — подбираем
                if (distanceToFish <= attachFishDistance && !isCarryingFish)
                {
                    PickupFish(targetFish);
                }

                // Если агент достиг цели (осталось меньше stoppingDistance и путь не пересчитывается)
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
                    !navMeshAgent.pathPending)
                {
                    StopMovingToFish();
                    OnReachedFish();
                    yield break; // Завершаем корутину
                }
            }
            else if (targetFish == null || !targetFish.activeInHierarchy)
            {
                // Если рыба исчезла или деактивировалась — отменяем движение
                StopMovingToFish();
                yield break;
            }

            // Ждём указанный интервал перед следующим обновлением
            yield return new WaitForSeconds(updateTargetInterval);
        }
    }

    // Подбор рыбы: "прикрепляем" к НПЦ, отключаем физику, включаем анимацию и VFX
    void PickupFish(GameObject fish)
    {
        if (fish == null || isCarryingFish) return;

        Debug.Log($"=== ПОДБОР РЫБЫ НАЧАТ ===");
        Debug.Log($"Рыба: {fish.name}");
        Debug.Log($"VFX включен: {enableVFX}");
        Debug.Log($"Количество найденных VFX объектов: {foundVFX.Count}");
        Debug.Log($"=========================");

        attachedFish = fish;
        hasFishAttached = true;
        isCarryingFish = true;

        // Отключаем физику у рыбы, чтобы она не падала и не коллайдилась
        Rigidbody rb = attachedFish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Отключаем коллайдер, чтобы рыба не мешала проходу
        Collider col = attachedFish.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Включаем обратно компонент, который отключали при начале рыбалки
        if (componentToDisable != null && !componentToDisable.enabled)
        {
            componentToDisable.enabled = true;
        }

        // Останавливаем движение к рыбе
        StopMovingToFish();

        // Переключаем анимацию на "с рыбой"
        if (npcAnimator != null)
        {
            npcAnimator.SetInteger(carringWalkParam, 4);
        }

        Debug.Log("Рыба поднята! Запускаем VFX...");

        // ВКЛЮЧАЕМ VFX, если разрешено в настройках
        if (enableVFX)
        {
            StartCoroutine(ActivateVFX());
        }
        else
        {
            Debug.LogWarning("VFX отключен в настройках!");
        }

        // Запускаем таймер: через 10 секунд рыба будет автоматически брошена
        StartCoroutine(CarryFishForDuration(10f));
    }

    // Корутина для активации и проигрывания VFX-эффектов
    IEnumerator ActivateVFX()
    {
        Debug.Log($"=== АКТИВАЦИЯ VFX ===");

        // Если список пуст — пробуем найти объекты ещё раз (на случай, если добавили в редакторе после старта)
        if (foundVFX.Count == 0)
        {
            Debug.LogError("Нет VFX объектов для активации! Ищем заново...");
            FindVFXObjects();
        }

        // Если всё равно пусто — выходим с ошибкой
        if (foundVFX.Count == 0)
        {
            Debug.LogError($"ВСЁ РАВНО НЕТ ОБЪЕКТОВ С ТЕГОМ '{vfxTag}'!");
            yield break;
        }

        Debug.Log($"Включаем {foundVFX.Count} VFX объектов:");

        // Включаем все найденные VFX-объекты
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                Debug.Log($"  Включаем: {vfx.name}");
                vfx.SetActive(true);

                // Запускаем ParticleSystem, если он есть
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps == null) ps = vfx.GetComponentInChildren<ParticleSystem>();

                if (ps != null)
                {
                    ps.Play();
                    Debug.Log($"    ParticleSystem запущен: {ps.name}, частиц: {ps.particleCount}");
                }
                else
                {
                    Debug.LogWarning($"    ParticleSystem не найден!");
                }
            }
        }

        Debug.Log($"VFX будут активны {vfxDuration} секунд");

        // Ждём указанное время
        yield return new WaitForSeconds(vfxDuration);

        Debug.Log("Отключаем VFX...");

        // Выключаем все VFX-объекты после завершения
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                vfx.SetActive(false);
            }
        }

        Debug.Log("=== VFX ОТКЛЮЧЕНЫ ===");
    }

    // Корутина-таймер: сколько секунд НПЦ несёт рыбу перед броском
    IEnumerator CarryFishForDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        DropFish();
    }

    // Бросок рыбы: возвращаем физику, добавляем импульс, выключаем эффекты
    void DropFish()
    {
        if (!isCarryingFish || attachedFish == null) return;

        // Возвращаем физику
        Rigidbody rb = attachedFish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            // Добавляем небольшой импульс вперёд и вверх для естественности
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
        }

        // Включаем коллайдер обратно
        Collider col = attachedFish.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Возвращаем анимацию в состояние "без рыбы"
        if (npcAnimator != null)
        {
            npcAnimator.SetInteger(carringWalkParam, 0);
        }

        // Выключаем VFX при броске (на всякий случай, если ещё проигрываются)
        if (enableVFX)
        {
            Debug.Log("Выключаем VFX при броске рыбы");
            foreach (GameObject vfx in foundVFX)
            {
                if (vfx != null && vfx.activeSelf)
                {
                    vfx.SetActive(false);
                }
            }
        }

        // Сбрасываем флаги и ссылки
        isCarryingFish = false;
        hasFishAttached = false;
        attachedFish = null;
    }

    // Остановка движения к рыбе: сброс флагов, корутины и пути агента
    void StopMovingToFish()
    {
        isMovingToFish = false;
        isGoingForFish = false;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.ResetPath();
        }
    }

    // Если агент дошёл до точки, но рыба ещё не подобрана — подбираем
    void OnReachedFish()
    {
        if (targetFish != null && !isCarryingFish)
        {
            PickupFish(targetFish);
        }
    }

    // Чистка при уничтожении объекта: останавливаем корутины и бросаем рыбу, если несём
    void OnDestroy()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        if (isCarryingFish)
        {
            DropFish();
        }
    }

    // Методы для отладки (вызываются через контекстное меню в Инспекторе)

    [ContextMenu("Проверить VFX настройки")]
    public void DebugVFXSettings()
    {
        Debug.Log($"=== VFX НАСТРОЙКИ ===");
        Debug.Log($"Enable VFX: {enableVFX}");
        Debug.Log($"VFX Tag: '{vfxTag}'");
        Debug.Log($"VFX Duration: {vfxDuration}");
        Debug.Log($"Найдено VFX объектов: {foundVFX.Count}");

        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                Debug.Log($"  - {vfx.name} (активен: {vfx.activeSelf})");
            }
        }

        // Перепроверяем в сцене — вдруг добавили новые объекты после старта
        GameObject[] allVFX = GameObject.FindGameObjectsWithTag(vfxTag);
        Debug.Log($"Объектов с тегом '{vfxTag}' в сцене: {allVFX.Length}");

        foreach (GameObject vfx in allVFX)
        {
            Debug.Log($"  - {vfx.name}");
        }

        Debug.Log($"=====================");
    }

    [ContextMenu("Тест: Включить VFX на 5 сек")]
    public void TestVFX()
    {
        if (enableVFX)
        {
            StartCoroutine(TestVFXCoroutine());
        }
        else
        {
            Debug.LogWarning("VFX отключен!");
        }
    }

    // Тестовая корутина: включает VFX на 5 секунд для быстрой проверки в редакторе
    IEnumerator TestVFXCoroutine()
    {
        Debug.Log("=== ТЕСТ VFX ===");

        // Включаем все кэшированные VFX
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                vfx.SetActive(true);
                Debug.Log($"Включен: {vfx.name}");
            }
        }

        yield return new WaitForSeconds(5f);

        // Выключаем обратно
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                vfx.SetActive(false);
                Debug.Log($"Выключен: {vfx.name}");
            }
        }

        Debug.Log("=== ТЕСТ ЗАВЕРШЕН ===");
    }
}