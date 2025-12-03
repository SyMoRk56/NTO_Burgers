using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 3f;
    public float stopDistance = 2f;
    public float rotationSpeed = 5f;

    [Header("Визуальные эффекты")]
    public ParticleSystem appearEffect;
    public ParticleSystem disappearEffect;

    // Состояния
    private enum NPCState
    {
        Appearing,      // Появление
        Approaching,    // Подход к игроку
        Waiting,        // Ожидание у игрока
        Leaving,        // Уход
        Disappearing    // Исчезновение
    }

    private NPCState currentState = NPCState.Appearing;
    private Transform player;
    private bool hasReachedPlayer = false;
    private Vector3 leaveDirection;

    void Start()
    {
        Debug.Log($"NPC {name}: Появился");

        // Находим игрока
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError($"NPC {name}: Игрок не найден!");
        }

        // Запускаем эффект появления
        if (appearEffect != null)
        {
            appearEffect.Play();
        }

        // Через 1 секунду начинаем движение к игроку
        Invoke("StartApproach", 1f);
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCState.Approaching:
                ApproachPlayer();
                break;

            case NPCState.Leaving:
                Leave();
                break;

            case NPCState.Disappearing:
                // Ничего не делаем, ждем уничтожения
                break;
        }
    }

    void StartApproach()
    {
        if (player != null)
        {
            currentState = NPCState.Approaching;
            Debug.Log($"NPC {name}: Начинает подход к игроку");
        }
    }

    // Метод для вызова из FishingSpot
    public void StartApproach(Transform playerTransform)
    {
        if (playerTransform == null) return;

        player = playerTransform;
        currentState = NPCState.Approaching;
        hasReachedPlayer = false;

        Debug.Log($"NPC {name}: Получил команду подойти к игроку");
    }

    void ApproachPlayer()
    {
        if (player == null) return;

        // Двигаемся к игроку
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Плавно поворачиваемся к игроку
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Проверяем расстояние до игрока
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= stopDistance)
        {
            hasReachedPlayer = true;
            currentState = NPCState.Waiting;
            Debug.Log($"NPC {name}: Достиг игрока, начинает ожидание");

            // Начинаем отсчет времени ожидания
            Invoke("StartLeaving", 5f);
        }
    }

    void StartLeaving()
    {
        currentState = NPCState.Leaving;

        // Выбираем случайное направление для ухода
        float randomAngle = Random.Range(0f, 360f);
        leaveDirection = new Vector3(Mathf.Sin(randomAngle * Mathf.Deg2Rad), 0, Mathf.Cos(randomAngle * Mathf.Deg2Rad));

        Debug.Log($"NPC {name}: Начинает уход");
    }

    void Leave()
    {
        // Двигаемся в выбранном направлении
        transform.position += leaveDirection * moveSpeed * Time.deltaTime;

        // Поворачиваемся в направлении движения
        if (leaveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(leaveDirection.x, 0, leaveDirection.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Через 3 секунды исчезаем
        Invoke("Disappear", 3f);
        currentState = NPCState.Disappearing;
    }

    void Disappear()
    {
        Debug.Log($"NPC {name}: Исчезает");

        // Запускаем эффект исчезновения
        if (disappearEffect != null)
        {
            disappearEffect.Play();
            // Ждем пока эффект проиграется
            Invoke("DestroyNPC", disappearEffect.main.duration);
        }
        else
        {
            DestroyNPC();
        }
    }

    void DestroyNPC()
    {
        Destroy(gameObject);
    }

    // Публичный метод для ухода (можно вызвать из FishingSpot)
    public void GoAway()
    {
        if (currentState == NPCState.Waiting)
        {
            StartLeaving();
        }
    }

    public bool HasReachedPlayer()
    {
        return hasReachedPlayer;
    }

    void OnDrawGizmos()
    {
        // Показываем направление к игроку (если есть)
        if (player != null && currentState == NPCState.Approaching)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Показываем направление ухода
        if (currentState == NPCState.Leaving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + leaveDirection * 5f);
        }
    }
}