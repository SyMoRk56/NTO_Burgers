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
    [SerializeField] private float updateTargetInterval = 1.0f;
    [SerializeField] private float attachFishDistance = 2.0f;
    [SerializeField] private float collisionCheckRadius = 2.0f;

    [Header("Настройки рыбы")]
    [SerializeField] private Vector3 fishCarryOffset = new Vector3(0f, 1.5f, 1.5f);
    [SerializeField] private Vector3 fishRotationOffset = new Vector3(0f, 90f, 0f);
    [SerializeField] private float fishFollowSpeed = 5f;

    [Header("Настройки анимации")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string carringWalkParam = "carringwalk";

    [Header("VFX эффекты - ПРОСТОЙ ВАРИАНТ")]
    [SerializeField] private bool enableVFX = true;
    [SerializeField] private string vfxTag = "PickupVFX"; // Тег для поиска VFX
    [SerializeField] private float vfxDuration = 3f;

    public bool isGoingForFish = false;

    private FishingSpot fishingManager;
    private NavMeshAgent navMeshAgent;
    private GameObject targetFish;
    private bool isMovingToFish = false;
    private Coroutine movementCoroutine;
    private bool hasFishAttached = false;
    private GameObject attachedFish;
    private bool isCarryingFish = false;
    private List<GameObject> foundVFX = new List<GameObject>(); // Найденные VFX

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

        // Ищем VFX объекты сразу при старте
        FindVFXObjects();

        Debug.Log($"NPCController инициализирован. VFX включен: {enableVFX}, тег: '{vfxTag}'");
    }

    // Метод для поиска VFX объектов
    void FindVFXObjects()
    {
        if (!enableVFX) return;

        GameObject[] vfxArray = GameObject.FindGameObjectsWithTag(vfxTag);
        foundVFX.Clear();

        foreach (GameObject vfx in vfxArray)
        {
            if (vfx != null)
            {
                foundVFX.Add(vfx);
                Debug.Log($"Найден VFX объект: {vfx.name}");

                // Сначала выключаем все VFX
                vfx.SetActive(false);

                // Проверяем, есть ли ParticleSystem
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
        if (isCarryingFish && attachedFish != null)
        {
            UpdateFishPosition();
        }

        UpdateAnimation();

        if (fishingManager != null && fishingManager.isFishingEnding)
        {
            CheckForBridgeContinuously();
        }
    }

    void UpdateFishPosition()
    {
        if (attachedFish == null) return;

        Vector3 targetPosition = transform.position +
                                transform.forward * fishCarryOffset.z +
                                transform.up * fishCarryOffset.y +
                                transform.right * fishCarryOffset.x;

        attachedFish.transform.position = Vector3.Lerp(
            attachedFish.transform.position,
            targetPosition,
            Time.deltaTime * fishFollowSpeed
        );

        Quaternion targetRotation = transform.rotation * Quaternion.Euler(fishRotationOffset);
        attachedFish.transform.rotation = Quaternion.Slerp(
            attachedFish.transform.rotation,
            targetRotation,
            Time.deltaTime * fishFollowSpeed
        );
    }

    void UpdateAnimation()
    {
        if (npcAnimator == null) return;

        int carringWalkValue = 0;

        if (isCarryingFish)
        {
            carringWalkValue = 4;
        }
        else if (isMovingToFish)
        {
            carringWalkValue = 1;
        }

        npcAnimator.SetInteger(carringWalkParam, carringWalkValue);

        if (navMeshAgent != null)
        {
            float speed = navMeshAgent.velocity.magnitude;
            npcAnimator.SetFloat("Speed", speed);
        }
    }

    void CheckForBridgeContinuously()
    {
        if (isMovingToFish || isCarryingFish) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, collisionCheckRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.gameObject.tag == bridgeTag)
            {
                if (componentToDisable != null && componentToDisable.enabled)
                {
                    componentToDisable.enabled = false;
                }

                if (!isMovingToFish)
                {
                    StartMovingToFish();
                }
                break;
            }
        }
    }

    void StartMovingToFish()
    {
        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag(fishTag);

        if (fishObjects.Length > 0)
        {
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
                targetFish = activeFish[0];
                float closestDistance = Vector3.Distance(transform.position, targetFish.transform.position);

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

                if (movementCoroutine != null)
                {
                    StopCoroutine(movementCoroutine);
                }

                movementCoroutine = StartCoroutine(MoveToFishCoroutine());
            }
        }
    }

    IEnumerator MoveToFishCoroutine()
    {
        while (isMovingToFish && targetFish != null)
        {
            if (navMeshAgent.isActiveAndEnabled && targetFish.activeInHierarchy)
            {
                navMeshAgent.SetDestination(targetFish.transform.position);

                float distanceToFish = Vector3.Distance(transform.position, targetFish.transform.position);

                if (distanceToFish <= attachFishDistance && !isCarryingFish)
                {
                    PickupFish(targetFish);
                }

                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
                    !navMeshAgent.pathPending)
                {
                    StopMovingToFish();
                    OnReachedFish();
                    yield break;
                }
            }
            else if (targetFish == null || !targetFish.activeInHierarchy)
            {
                StopMovingToFish();
                yield break;
            }

            yield return new WaitForSeconds(updateTargetInterval);
        }
    }

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

        Rigidbody rb = attachedFish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = attachedFish.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (componentToDisable != null && !componentToDisable.enabled)
        {
            componentToDisable.enabled = true;
        }

        StopMovingToFish();

        if (npcAnimator != null)
        {
            npcAnimator.SetInteger(carringWalkParam, 4);
        }

        Debug.Log("Рыба поднята! Запускаем VFX...");

        // ВКЛЮЧАЕМ VFX
        if (enableVFX)
        {
            StartCoroutine(ActivateVFX());
        }
        else
        {
            Debug.LogWarning("VFX отключен в настройках!");
        }

        StartCoroutine(CarryFishForDuration(10f));
    }

    // Корутина для активации VFX
    IEnumerator ActivateVFX()
    {
        Debug.Log($"=== АКТИВАЦИЯ VFX ===");

        if (foundVFX.Count == 0)
        {
            Debug.LogError("Нет VFX объектов для активации! Ищем заново...");
            FindVFXObjects();
        }

        if (foundVFX.Count == 0)
        {
            Debug.LogError($"ВСЁ РАВНО НЕТ ОБЪЕКТОВ С ТЕГОМ '{vfxTag}'!");
            yield break;
        }

        Debug.Log($"Включаем {foundVFX.Count} VFX объектов:");

        // Включаем все VFX
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                Debug.Log($"  Включаем: {vfx.name}");
                vfx.SetActive(true);

                // Запускаем ParticleSystem
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

        // Ждем указанное время
        yield return new WaitForSeconds(vfxDuration);

        Debug.Log("Отключаем VFX...");

        // Выключаем все VFX
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                vfx.SetActive(false);
            }
        }

        Debug.Log("=== VFX ОТКЛЮЧЕНЫ ===");
    }

    IEnumerator CarryFishForDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        DropFish();
    }

    void DropFish()
    {
        if (!isCarryingFish || attachedFish == null) return;

        Rigidbody rb = attachedFish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
        }

        Collider col = attachedFish.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        if (npcAnimator != null)
        {
            npcAnimator.SetInteger(carringWalkParam, 0);
        }

        // Выключаем VFX при броске
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

        isCarryingFish = false;
        hasFishAttached = false;
        attachedFish = null;
    }

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

    void OnReachedFish()
    {
        if (targetFish != null && !isCarryingFish)
        {
            PickupFish(targetFish);
        }
    }

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

    // Методы для отладки
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

        // Перепроверяем в сцене
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

    IEnumerator TestVFXCoroutine()
    {
        Debug.Log("=== ТЕСТ VFX ===");

        // Включаем
        foreach (GameObject vfx in foundVFX)
        {
            if (vfx != null)
            {
                vfx.SetActive(true);
                Debug.Log($"Включен: {vfx.name}");
            }
        }

        yield return new WaitForSeconds(5f);

        // Выключаем
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