using UnityEngine;
using System.Collections;

public class ObjectMovement : MonoBehaviour
{
    // Массив точек для перемещения
    public Vector3[] waypoints = new Vector3[]
    {
        new Vector3(127f, 0f, 50f),  // Стартовая точка (телепортация)
        new Vector3(130f, 0f, 50f),  // Точка 1
        new Vector3(130f, 0f, 55f),  // Точка 2
        new Vector3(135f, 0f, 60f)   // Точка 3
    };

    [Header("Настройки движения")]
    public float moveSpeed = 0.5f;
    public bool teleportToStart = true;
    public bool loopMovement = false;

    [Header("Настройки взаимодействия")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public bool showDebugRadius = true;

    [Header("Визуальные настройки")]
    public GameObject interactionIndicator; // Опциональный индикатор

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool hasInteracted = false;
    private Transform playerTransform;
    private SphereCollider interactionCollider;

    private void Start()
    {
        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Игрок не найден! Убедитесь, что у игрока есть тег 'Player'");
        }

        // Создаем коллайдер для зоны взаимодействия
        interactionCollider = gameObject.AddComponent<SphereCollider>();
        interactionCollider.isTrigger = true;
        interactionCollider.radius = interactionRange;

        // Добавляем Rigidbody если нужно для триггера
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Скрываем индикатор если он есть
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        // Проверяем расстояние до игрока
        if (playerTransform != null && !hasInteracted)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Показываем/скрываем индикатор
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(distanceToPlayer <= interactionRange);
            }

            // Проверяем взаимодействие
            if (distanceToPlayer <= interactionRange && Input.GetKeyDown(interactionKey))
            {
                OnInteract();
            }
        }

        // Обрабатываем движение
        if (isMoving && currentWaypointIndex < waypoints.Length)
        {
            MoveToWaypoint();
        }
        else if (isMoving && currentWaypointIndex >= waypoints.Length)
        {
            if (loopMovement)
            {
                RestartMovement();
            }
            else
            {
                isMoving = false;
                Debug.Log("Движение завершено");
            }
        }
    }

    private void OnInteract()
    {
        if (hasInteracted) return;

        hasInteracted = true;
        Debug.Log("Объект взаимодействует, начинаем движение!");

        // Скрываем индикатор
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }

        // Отключаем коллайдер взаимодействия
        if (interactionCollider != null)
        {
            interactionCollider.enabled = false;
        }

        // Начинаем движение
        StartMovement();
    }

    private void MoveToWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex];

        // Плавное перемещение к точке
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Поворачиваем объект в сторону движения (опционально)
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
            }
        }

        // Проверяем, достигли ли точки
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            Debug.Log($"Достигнута точка {currentWaypointIndex + 1}");
            currentWaypointIndex++;
        }
    }

    private void StartMovement()
    {
        isMoving = true;

        if (teleportToStart && waypoints.Length > 0)
        {
            // Мгновенная телепортация к первой точке
            transform.position = waypoints[0];
            Debug.Log($"Телепортирован в точку: {waypoints[0]}");

            // Начинаем движение ко второй точке
            currentWaypointIndex = 1;
        }
        else
        {
            // Двигаемся от текущей позиции
            currentWaypointIndex = 0;
        }
    }

    public void RestartMovement()
    {
        hasInteracted = false;
        isMoving = false;
        currentWaypointIndex = 0;

        // Включаем коллайдер обратно
        if (interactionCollider != null)
        {
            interactionCollider.enabled = true;
        }

        Debug.Log("Движение перезапущено");
    }

    public void SetNewWaypoints(Vector3[] newWaypoints)
    {
        waypoints = newWaypoints;
        RestartMovement();
    }

    // Для отладки: рисуем радиус взаимодействия в редакторе
    private void OnDrawGizmosSelected()
    {
        if (showDebugRadius)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // Рисуем точки маршрута
            if (waypoints != null && waypoints.Length > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < waypoints.Length; i++)
                {
                    Gizmos.DrawSphere(waypoints[i], 0.5f);

                    if (i < waypoints.Length - 1)
                    {
                        Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                    }

                    // Подписываем точки
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(waypoints[i] + Vector3.up, $"Точка {i + 1}");
#endif
                }
            }
        }
    }

    // Триггер для взаимодействия (альтернативный метод)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasInteracted)
        {
            playerTransform = other.transform;
            Debug.Log("Игрок вошел в зону взаимодействия");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок вышел из зоны взаимодействия");
        }
    }

    // Публичные методы для управления из других скриптов
    public void ForceStartMovement()
    {
        OnInteract();
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool HasInteracted()
    {
        return hasInteracted;
    }
}