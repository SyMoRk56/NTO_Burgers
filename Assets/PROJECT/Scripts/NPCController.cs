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
    [SerializeField] private string carringWalkParam = "carringwalk"; // Название параметра в аниматоре

    public bool isGoingForFish = false;

    private FishingSpot fishingManager;
    private NavMeshAgent navMeshAgent;
    private GameObject targetFish;
    private bool isMovingToFish = false;
    private Coroutine movementCoroutine;
    private bool hasFishAttached = false;
    private GameObject attachedFish;
    private bool isCarryingFish = false;

    private void Start()
    {
        Debug.Log($"NPCController.Start() вызван для {gameObject.name}");

        fishingManager = FindObjectOfType<FishingSpot>();

        if (fishingManager == null)
        {
            Debug.LogWarning("FishingManager не найден в сцене!");
        }
        else
        {
            Debug.Log($"FishingManager найден: {fishingManager.gameObject.name}");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            Debug.Log("NavMeshAgent добавлен к NPC");
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
            if (npcAnimator == null)
            {
                Debug.LogWarning("Animator не найден на NPC!");
            }
            else
            {
                // Проверяем, есть ли параметр carringwalk в аниматоре
                bool hasParam = false;
                foreach (AnimatorControllerParameter param in npcAnimator.parameters)
                {
                    if (param.name == carringWalkParam && param.type == AnimatorControllerParameterType.Int)
                    {
                        hasParam = true;
                        break;
                    }
                }

                if (!hasParam)
                {
                    Debug.LogWarning($"Параметр '{carringWalkParam}' типа Int не найден в аниматоре!");
                }
                else
                {
                    Debug.Log($"Параметр '{carringWalkParam}' найден в аниматоре");
                }
            }
        }

        Debug.Log($"NPCController инициализирован. Bridge tag: '{bridgeTag}', Fish tag: '{fishTag}'");
    }

    private void Update()
    {
        // Если NPC несет рыбу, обновляем позицию рыбы
        if (isCarryingFish && attachedFish != null)
        {
            UpdateFishPosition();
        }

        // Обновляем анимацию в зависимости от состояния
        UpdateAnimation();

        // Постоянная проверка нахождения на мосту при isFishingEnding == true
        if (fishingManager != null && fishingManager.isFishingEnding)
        {
            CheckForBridgeContinuously();
        }

        // Отладочная информация каждые 2 секунды
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"NPC состояние: isGoingForFish={isGoingForFish}, isMovingToFish={isMovingToFish}, isCarryingFish={isCarryingFish}");
            if (fishingManager != null)
            {
                Debug.Log($"FishingManager.isFishingEnding = {fishingManager.isFishingEnding}");
            }
        }
    }

    // Обновление позиции рыбы при переносе
    private void UpdateFishPosition()
    {
        if (attachedFish == null) return;

        // Вычисляем целевую позицию рыбы
        Vector3 targetPosition = transform.position +
                                transform.forward * fishCarryOffset.z +
                                transform.up * fishCarryOffset.y +
                                transform.right * fishCarryOffset.x;

        // Плавное движение рыбы к целевой позиции
        attachedFish.transform.position = Vector3.Lerp(
            attachedFish.transform.position,
            targetPosition,
            Time.deltaTime * fishFollowSpeed
        );

        // Поворачиваем рыбу
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(fishRotationOffset);
        attachedFish.transform.rotation = Quaternion.Slerp(
            attachedFish.transform.rotation,
            targetRotation,
            Time.deltaTime * fishFollowSpeed
        );
    }

    // Обновление анимации
    private void UpdateAnimation()
    {
        if (npcAnimator == null) return;

        // Определяем значение параметра carringwalk
        int carringWalkValue = 0;

        if (isCarryingFish)
        {
            // Если несет рыбу - устанавливаем 4
            carringWalkValue = 4;
            Debug.Log($"Устанавливаем carringwalk = 4 (несет рыбу)");
        }
        else if (isMovingToFish)
        {
            // Если идет за рыбой - можно установить 1 (просто идет)
            carringWalkValue = 1;
        }
        // 0 - idle по умолчанию

        npcAnimator.SetInteger(carringWalkParam, carringWalkValue);

        // Также можно контролировать скорость анимации в зависимости от скорости движения
        if (navMeshAgent != null)
        {
            float speed = navMeshAgent.velocity.magnitude;
            npcAnimator.SetFloat("Speed", speed);
        }
    }

    // Постоянная проверка нахождения на мосту
    private void CheckForBridgeContinuously()
    {
        if (isMovingToFish || isCarryingFish) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, collisionCheckRadius);

        bool foundBridge = false;

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.gameObject.tag == bridgeTag)
            {
                foundBridge = true;
                Debug.Log($"Найден мост: {collider.gameObject.name} на дистанции: {Vector3.Distance(transform.position, collider.transform.position):F2}");

                // Отключаем компонент
                if (componentToDisable != null && componentToDisable.enabled)
                {
                    componentToDisable.enabled = false;
                    Debug.Log($"Компонент {componentToDisable.GetType().Name} отключен");
                }

                // Начинаем движение к рыбе
                if (!isMovingToFish)
                {
                    Debug.Log("Условие выполнено: isFishingEnding == true и NPC на мосту");
                    StartMovingToFish();
                }
                break;
            }
        }

        if (!foundBridge && colliders.Length > 0)
        {
            Debug.Log($"Мост не найден. Проверенные объекты ({colliders.Length}):");
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    Debug.Log($"  - {collider.gameObject.name} (тег: {collider.gameObject.tag})");
                }
            }
        }
    }

    private void StartMovingToFish()
    {
        Debug.Log($"Поиск объектов с тегом '{fishTag}'...");
        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag(fishTag);

        Debug.Log($"Найдено объектов с тегом '{fishTag}': {fishObjects.Length}");

        if (fishObjects.Length > 0)
        {
            // Сначала ищем активные (не уничтожаемые) рыбы
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

                // Выбираем ближайшую активную рыбу
                foreach (GameObject fish in activeFish)
                {
                    float distance = Vector3.Distance(transform.position, fish.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetFish = fish;
                    }
                }

                Debug.Log($"Выбрана рыба: {targetFish.name}, расстояние: {closestDistance:F2}");

                isMovingToFish = true;
                isGoingForFish = true;

                if (movementCoroutine != null)
                {
                    StopCoroutine(movementCoroutine);
                }

                movementCoroutine = StartCoroutine(MoveToFishCoroutine());
            }
            else
            {
                Debug.LogWarning("Не найдено активных рыб! Все рыбы могут быть помечены для уничтожения.");
            }
        }
        else
        {
            Debug.LogWarning($"Объектов с тегом '{fishTag}' не найдено! Создайте рыбу с тегом 'fish'.");
        }
    }

    private IEnumerator MoveToFishCoroutine()
    {
        Debug.Log($"Начало движения к рыбе: {targetFish?.name}");

        while (isMovingToFish && targetFish != null)
        {
            if (navMeshAgent.isActiveAndEnabled && targetFish.activeInHierarchy)
            {
                navMeshAgent.SetDestination(targetFish.transform.position);

                float distanceToFish = Vector3.Distance(transform.position, targetFish.transform.position);

                if (distanceToFish <= attachFishDistance && !isCarryingFish)
                {
                    Debug.Log($"Достаточно близко! Поднимаем рыбу...");
                    PickupFish(targetFish);
                }

                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
                    !navMeshAgent.pathPending)
                {
                    Debug.Log("Достигнута конечная точка");
                    StopMovingToFish();
                    OnReachedFish();
                    yield break;
                }
            }
            else if (targetFish == null || !targetFish.activeInHierarchy)
            {
                Debug.LogWarning("Целевая рыба уничтожена или не активна!");
                StopMovingToFish();
                yield break;
            }

            yield return new WaitForSeconds(updateTargetInterval);
        }

        Debug.Log("Движение завершено");
    }

    private void PickupFish(GameObject fish)
    {
        if (fish == null || isCarryingFish) return;

        Debug.Log($"NPC поднимает рыбу: {fish.name}");

        // Сохраняем ссылку на рыбу
        attachedFish = fish;
        hasFishAttached = true;
        isCarryingFish = true;

        // Отключаем физику
        Rigidbody rb = attachedFish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Отключаем коллайдер
        Collider col = attachedFish.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Включаем отключенный компонент обратно
        if (componentToDisable != null && !componentToDisable.enabled)
        {
            componentToDisable.enabled = true;
            Debug.Log($"Компонент {componentToDisable.GetType().Name} включен обратно");
        }

        // Останавливаем движение
        StopMovingToFish();

        // Устанавливаем параметр аниматора
        if (npcAnimator != null)
        {
            npcAnimator.SetInteger(carringWalkParam, 4);
            Debug.Log($"Установлен параметр {carringWalkParam} = 4");
        }

        Debug.Log("Рыба успешно поднята! NPC теперь несет рыбу.");

        // Запускаем корутину для ношения рыбы
        StartCoroutine(CarryFishForDuration(10f));
    }

    private IEnumerator CarryFishForDuration(float duration)
    {
        Debug.Log($"NPC будет нести рыбу {duration} секунд");
        yield return new WaitForSeconds(duration);

        // Бросаем рыбу
        DropFish();
    }

    private void DropFish()
    {
        if (!isCarryingFish || attachedFish == null) return;

        Debug.Log($"NPC бросает рыбу: {attachedFish.name}");

        // Включаем физику обратно
        Rigidbody rb = attachedFish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            // Добавляем небольшой толчок вперед
            rb.AddForce(transform.forward * 2f + Vector3.up * 1f, ForceMode.Impulse);
        }

        // Включаем коллайдер
        Collider col = attachedFish.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Возвращаем параметр аниматора в 0 (idle)
        if (npcAnimator != null)
        {
            npcAnimator.SetInteger(carringWalkParam, 0);
            Debug.Log($"Установлен параметр {carringWalkParam} = 0");
        }

        // Сбрасываем флаги
        isCarryingFish = false;
        hasFishAttached = false;
        attachedFish = null;

        Debug.Log("Рыба брошена!");
    }

    private void StopMovingToFish()
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

        Debug.Log("Движение остановлено");
    }

    private void OnReachedFish()
    {
        Debug.Log("NPC достиг позиции рыбы");

        if (targetFish != null && !isCarryingFish)
        {
            PickupFish(targetFish);
        }
    }

    private void OnDestroy()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        // Бросаем рыбу при уничтожении
        if (isCarryingFish)
        {
            DropFish();
        }
    }

    // Метод для ручной проверки состояния
    [ContextMenu("Проверить состояние NPC")]
    public void CheckNPCStatus()
    {
        Debug.Log($"=== ПРОВЕРКА СОСТОЯНИЯ NPC ===");
        Debug.Log($"Имя: {gameObject.name}");
        Debug.Log($"Позиция: {transform.position}");
        Debug.Log($"isGoingForFish: {isGoingForFish}");
        Debug.Log($"isMovingToFish: {isMovingToFish}");
        Debug.Log($"isCarryingFish: {isCarryingFish}");
        Debug.Log($"Компонент отключен: {componentToDisable != null && !componentToDisable.enabled}");

        if (fishingManager != null)
        {
            Debug.Log($"FishingManager.isFishingEnding: {fishingManager.isFishingEnding}");
        }

        // Проверим наличие мостов рядом
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        int bridgeCount = 0;
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.tag == bridgeTag)
            {
                bridgeCount++;
                Debug.Log($"Мост найден: {collider.gameObject.name} на расстоянии {Vector3.Distance(transform.position, collider.transform.position):F2}");
            }
        }
        Debug.Log($"Мостов в радиусе 5м: {bridgeCount}");

        // Проверим наличие рыб
        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag(fishTag);
        Debug.Log($"Рыб в сцене: {fishObjects.Length}");
        foreach (GameObject fish in fishObjects)
        {
            Debug.Log($"  - {fish.name} (активна: {fish.activeInHierarchy})");
        }

        // Проверим состояние аниматора
        if (npcAnimator != null)
        {
            Debug.Log($"Параметр {carringWalkParam}: {npcAnimator.GetInteger(carringWalkParam)}");
        }

        Debug.Log($"=== КОНЕЦ ПРОВЕРКИ ===");
    }

    // Метод для принудительного поднятия рыбы
    [ContextMenu("Поднять рыбу принудительно")]
    public void ForcePickupFish()
    {
        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag(fishTag);
        if (fishObjects.Length > 0 && !isCarryingFish)
        {
            PickupFish(fishObjects[0]);
        }
    }

    // Метод для принудительного броска рыбы
    [ContextMenu("Бросить рыбу")]
    public void ForceDropFish()
    {
        DropFish();
    }
}