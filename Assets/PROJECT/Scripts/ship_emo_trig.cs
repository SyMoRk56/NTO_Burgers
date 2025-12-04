using UnityEngine;
using System.Collections.Generic;

public class ObjectMovement : MonoBehaviour
{
    [System.Serializable]
    public class DropZoneData
    {
        public GameObject dropZoneObject; // Ссылка на GameObject зоны
        public Vector3 spawnPoint;        // Точка, куда телепортируется объект
        public Vector3[] waypoints;       // Маршрут движения
    }

    [Header("Настройки движения")]
    public float moveSpeed = 0.5f;
    public bool loopMovement = false;

    [Header("Настройки взаимодействия")]
    public float interactionRange = 3f;
    public KeyCode pickUpKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;

    [Header("Позиция в руках")]
    public Vector3 heldPosition = new Vector3(0, 1, 2);
    public float pickUpSpeed = 5f;

    [Header("Зоны сброса и маршруты")]
    public List<DropZoneData> dropZones = new List<DropZoneData>();

    [Header("Визуальные настройки")]
    public GameObject interactionIndicator;

    [Header("Настройки зоны")]
    public float dropZoneRadius = 5f; // Радиус для проверки зоны

    // Состояния
    private enum State { Idle, Held, Moving }
    private State currentState = State.Idle;

    private Transform player; // Основной объект игрока
    private Transform playerModel; // Модель игрока с тегом "Model"
    private PlayerMovement playerMovement; // Добавляем ссылку на PlayerMovement
    private int currentWaypointIndex = 0;
    private Vector3[] currentWaypoints;
    private SphereCollider interactionCollider;
    private Rigidbody rb;
    private DropZoneData currentDropZone;
    private bool playerInDropZone = false;
    private Transform originalParent; // Оригинальный родитель
    private Vector3 originalScale; // Оригинальный масштаб

    void Start()
    {
        // Находим основной объект игрока
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            // Получаем компонент PlayerMovement
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("PlayerMovement не найден на игроке!");
            }
        }
        else
        {
            Debug.LogWarning("Игрок не найден! Убедитесь, что у игрока есть тег 'Player'");
        }

        // Находим модель игрока по тегу "model"
        GameObject modelObj = GameObject.FindGameObjectWithTag("model");
        if (modelObj != null)
        {
            playerModel = modelObj.transform;
        }
        else
        {
            Debug.LogWarning("Модель игрока не найдена! Убедитесь, что у модели есть тег 'model'");
            playerModel = player;
        }

        // Сохраняем оригинальные параметры
        originalParent = transform.parent;
        originalScale = transform.localScale;

        // Настройка коллайдера для взаимодействия
        interactionCollider = gameObject.AddComponent<SphereCollider>();
        interactionCollider.isTrigger = true;
        interactionCollider.radius = interactionRange;

        // Настройка физики
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Скрываем индикатор
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
    }

    void Update()
    {
        // Проверка взаимодействия с игроком
        if (player != null && currentState == State.Idle)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (interactionIndicator != null)
                interactionIndicator.SetActive(distance <= interactionRange);

            if (distance <= interactionRange && Input.GetKeyDown(pickUpKey))
                PickUp();
        }

        // Сброс объекта
        if (currentState == State.Held && Input.GetKeyDown(dropKey))
            Drop();

        // Проверка находится ли игрок в зоне сброса
        CheckPlayerInDropZone();

        // ПРЯМОЙ ЗАПУСК ПРИ ПОСТАНОВКЕ В ЗОНУ
        if (currentState == State.Held && playerInDropZone && Input.GetKeyDown(pickUpKey))
        {
            LaunchImmediately();
        }

        // Движение по точкам
        if (currentState == State.Moving)
            MoveAlongWaypoints();
    }

    void CheckPlayerInDropZone()
    {
        if (player == null) return;

        playerInDropZone = false;
        currentDropZone = null;

        foreach (var zone in dropZones)
        {
            if (zone.dropZoneObject == null) continue;

            float playerZoneDistance = Vector3.Distance(player.position, zone.dropZoneObject.transform.position);

            if (playerZoneDistance <= dropZoneRadius)
            {
                playerInDropZone = true;
                currentDropZone = zone;
                break;
            }
        }
    }

    void PickUp()
    {
        if (playerModel == null) return;

        currentState = State.Held;
        interactionCollider.enabled = false;

        // Делаем объект дочерним к модели игрока
        transform.SetParent(playerModel);
        transform.localPosition = heldPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;

        // Выключаем гравитацию и включаем кинематику
        rb.isKinematic = true;
        rb.useGravity = false;

        // Устанавливаем флаг переноски в PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.isCarrying = true;
            // Обновляем анимацию сразу на idle с переноской
            if (playerMovement.animScript != null)
            {
                playerMovement.animScript.HeroIdleAnim(true);
            }
        }

        Debug.Log("Объект взят в руки и прикреплен к модели игрока");
    }

    void Drop()
    {
        currentState = State.Idle;

        // Открепляем от модели игрока и возвращаем в оригинального родителя
        transform.SetParent(originalParent);
        transform.localScale = originalScale;

        // Включаем гравитацию - объект упадет
        rb.isKinematic = false;
        rb.useGravity = true;

        // Включаем коллайдер обратно
        interactionCollider.enabled = true;

        // Сбрасываем флаг переноски в PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.isCarrying = false;
            // Обновляем анимацию на обычный idle
            if (playerMovement.animScript != null)
            {
                playerMovement.animScript.HeroIdleAnim(false);
            }
        }

        Debug.Log("Объект отпущен, падает...");
    }

    void LaunchImmediately()
    {
        if (currentDropZone == null) return;

        // ПРОВЕРЯЕМ, ЧТО spawnPoint НЕ НУЛЕВОЙ
        if (currentDropZone.spawnPoint == Vector3.zero)
        {
            Debug.LogError($"spawnPoint не задан для зоны: {currentDropZone.dropZoneObject.name}!");
            return;
        }

        // Проверяем наличие точек маршрута
        if (currentDropZone.waypoints == null || currentDropZone.waypoints.Length == 0)
        {
            Debug.LogWarning("Нет маршрута для этой зоны!");
            return;
        }

        // Открепляем от модели игрока
        transform.SetParent(originalParent);
        transform.localScale = originalScale;

        // ПРЯМОЙ СТАРТ - телепортируем к первой точке маршрута и сразу начинаем движение
        currentState = State.Moving;
        currentWaypoints = currentDropZone.waypoints;
        currentWaypointIndex = 0;

        // Телепортируем к первой точке маршрута (или spawnPoint)
        if (currentWaypoints.Length > 0)
        {
            // Используем первую точку маршрута как стартовую позицию
            Vector3 startPosition = currentDropZone.spawnPoint;
            transform.position = startPosition;

            // Если есть хотя бы 2 точки, начинаем движение ко второй
            if (currentWaypoints.Length > 1)
            {
                currentWaypointIndex = 1;
                Debug.Log($"Объект поставлен в зону и начал движение к точке {currentWaypointIndex}");
            }
            else
            {
                // Если только 1 точка, просто остаемся в ней
                Debug.Log("Объект поставлен в зону, но только 1 точка маршрута");
            }
        }

        // Настраиваем физику для движения
        rb.isKinematic = true;
        rb.useGravity = false;

        // Включаем коллайдер взаимодействия
        interactionCollider.enabled = true;

        // Сбрасываем флаг переноски в PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.isCarrying = false;
            // Обновляем анимацию на обычный idle
            if (playerMovement.animScript != null)
            {
                playerMovement.animScript.HeroIdleAnim(false);
            }
        }

        Debug.Log($"Объект запущен из зоны: {currentDropZone.dropZoneObject.name}");
        Debug.Log($"Количество точек маршрута: {currentWaypoints.Length}");
    }

    void MoveAlongWaypoints()
    {
        if (currentWaypoints == null || currentWaypoints.Length == 0)
        {
            currentState = State.Idle;
            rb.isKinematic = false;
            rb.useGravity = true;
            return;
        }

        // Проверяем, что currentWaypointIndex в пределах массива
        if (currentWaypointIndex < 0 || currentWaypointIndex >= currentWaypoints.Length)
        {
            CompleteRoute();
            return;
        }

        Vector3 target = currentWaypoints[currentWaypointIndex];

        // Проверяем, что точка не нулевая
        if (target == Vector3.zero)
        {
            Debug.LogWarning($"Точка {currentWaypointIndex} равна (0,0,0)! Пропускаем.");
            currentWaypointIndex++;
            return;
        }

        // Движение к точке
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        // Поворот в сторону движения
        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }

        // Проверка достижения точки
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentWaypointIndex++;

            // Проверка завершения маршрута
            if (currentWaypointIndex >= currentWaypoints.Length)
            {
                CompleteRoute();
            }
        }
    }

    void CompleteRoute()
    {
        if (loopMovement)
        {
            currentWaypointIndex = 0;
            Debug.Log("Маршрут завершен, начинаем заново");
        }
        else
        {
            currentState = State.Idle;
            rb.isKinematic = false;
            rb.useGravity = true;
            Debug.Log("Маршрут завершен");
        }
    }

    void LateUpdate()
    {
        // Объект следует за моделью игрока когда в руках
        if (currentState == State.Held && playerModel != null)
        {
            transform.localPosition = heldPosition;
            transform.localRotation = Quaternion.identity;
        }
    }

    void OnDestroy()
    {
        // При уничтожении объекта сбрасываем флаг переноски
        if (playerMovement != null)
        {
            playerMovement.isCarrying = false;
            if (playerMovement.animScript != null)
            {
                playerMovement.animScript.HeroIdleAnim(false);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация зон сброса
        foreach (var zone in dropZones)
        {
            if (zone.dropZoneObject == null) continue;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(zone.dropZoneObject.transform.position, dropZoneRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(zone.spawnPoint, 0.5f);
            Gizmos.DrawLine(zone.dropZoneObject.transform.position, zone.spawnPoint);

            if (zone.waypoints != null && zone.waypoints.Length > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < zone.waypoints.Length; i++)
                {
                    Gizmos.DrawSphere(zone.waypoints[i], 0.3f);
                    if (i < zone.waypoints.Length - 1)
                    {
                        Gizmos.DrawLine(zone.waypoints[i], zone.waypoints[i + 1]);
                    }
                }
            }
        }
    }

    public bool IsHeld() => currentState == State.Held;
    public bool IsMoving() => currentState == State.Moving;
    public bool IsIdle() => currentState == State.Idle;
}