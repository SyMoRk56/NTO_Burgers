using UnityEngine;
using System.Collections;

public class FishingSpot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform fishingRod;
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private Transform fishSpawnPoint;

    [Header("Settings")]
    [SerializeField] private Vector3 rodCastRotation = new Vector3(-30f, 0f, 0f);
    [SerializeField] private Vector3 rodIdleRotation = Vector3.zero;
    [SerializeField] private float fishFlyDuration = 2f;
    [SerializeField] private float fishSpawnDelay = 0.5f;

    // Состояния
    private bool isPlayerInRange = false;
    private bool isFishingActive = false;
    private bool isRodCast = false;
    private GameObject currentPlayer;

    // Компоненты игрока
    private playerAnimations playerAnim;
    private PlayerManager playerManager;
    private CharacterController characterController;
    private Transform playerModel; // Модель игрока (дочерний объект)

    void Start()
    {
        // Автоматически находим удочку по тегу
        if (fishingRod == null)
        {
            Transform rod = FindChildWithTag(transform, "fishrod");
            if (rod != null) fishingRod = rod;
        }
    }

    void Update()
    {
        if (!isPlayerInRange || currentPlayer == null) return;

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

        // 3. Позиционируем ИГРОКА (родителя) в точку рыбалки
        currentPlayer.transform.position = transform.position;

        // 4. Поворачиваем только МОДЕЛЬ (дочерний объект)
        if (playerModel != null)
        {
            playerModel.rotation = transform.rotation;
            Debug.Log("Повернули модель игрока");
        }
        else
        {
            // Если нет модели, поворачиваем весь объект
            currentPlayer.transform.rotation = transform.rotation;
            Debug.LogWarning("Не найдена модель игрока (тег 'model'). Поворачиваем весь объект.");
        }

        // 5. Включаем анимацию fishing_bros
        playerAnim.StartFishing();

        isFishingActive = true;
        isRodCast = false;

        Debug.Log("Рыбалка начата! Нажми E для заброса");
    }

    private void CastRod()
    {
        // 1. Поворачиваем удочку
        if (fishingRod != null)
        {
            fishingRod.localEulerAngles = rodCastRotation;
            Debug.Log("Удочка повернута");
        }

        // 2. Меняем анимацию на fishing_idle
        if (playerAnim != null)
        {
            playerAnim.FishingIdle();
        }

        isRodCast = true;
        Debug.Log("Удочка заброшена! Нажми E чтобы вытащить");
    }

    private void ReelRod()
    {
        // 1. Возвращаем удочку в исходное положение
        if (fishingRod != null)
        {
            fishingRod.localEulerAngles = rodIdleRotation;
        }

        // 2. Возвращаем анимацию fishing_bros
        if (playerAnim != null)
        {
            playerAnim.StartFishing();
        }

        // 3. Создаем рыбу с задержкой
        if (fishPrefab != null && fishSpawnPoint != null)
        {
            Invoke(nameof(SpawnFish), fishSpawnDelay);
        }

        isRodCast = false;

        // 4. Завершаем рыбалку через 1 секунду
        Invoke(nameof(EndFishing), fishSpawnDelay + 1f);
    }

    private void SpawnFish()
    {
        if (fishPrefab != null && fishSpawnPoint != null)
        {
            GameObject fish = Instantiate(fishPrefab, fishSpawnPoint.position, Quaternion.identity);
            StartCoroutine(FlyFish(fish));
            Destroy(fish, 5f);
            Debug.Log("Рыба создана!");
        }
    }

    private IEnumerator FlyFish(GameObject fish)
    {
        float time = 0f;
        Vector3 startPos = fish.transform.position;

        // Цель - перед игроком на высоте груди
        Vector3 targetPos = currentPlayer.transform.position +
                           (playerModel != null ? playerModel.forward : currentPlayer.transform.forward) * 1f +
                           Vector3.up * 0.8f;

        while (time < fishFlyDuration)
        {
            if (fish == null) yield break;

            fish.transform.position = Vector3.Lerp(startPos, targetPos, time / fishFlyDuration);

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

        Debug.Log("Рыбалка завершена!");
    }

    private void CancelFishing()
    {
        if (isFishingActive)
        {
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
            Debug.Log("Игрок в зоне рыбалки");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

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

    // Отладка
    private void OnDrawGizmos()
    {
        if (isPlayerInRange)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

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