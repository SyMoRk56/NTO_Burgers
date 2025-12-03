using UnityEngine;
using System.Collections;

public class FishingSpot : MonoBehaviour
{
    [Header("Ссылки")]
    public GameObject fishingUI;
    public GameObject fishPrefab;
    public GameObject npcPrefab; // Префаб NPC, который появится

    [Header("Настройки рыбалки")]
    [Range(0f, 1f)] public float successChance = 0.8f; // 80% шанс поймать рыбу
    [Range(0f, 1f)] public float npcSpawnChance = 0.2f; // 20% шанс появления NPC

    [Header("Точка появления NPC")]
    public Transform npcSpawnPoint; // Точка, где появится NPC

    [Header("Тайминги")]
    public float fishingTime = 2f; // Время рыбалки
    public float fishAttachTime = 2f; // Время перемещения рыбы к игроку
    public float npcWaitTime = 5f; // Время ожидания NPC у игрока

    // Приватные переменные
    private bool isPlayerNear = false;
    private bool isFishing = false;
    private GameObject spawnedFish;
    private NPCController currentNPC;
    private PlayerMovement playerMovement;

    void Start()
    {
        Debug.Log($"FishingSpot: Инициализирован на {gameObject.name}");

        // Находим компонент движения игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            Debug.Log($"FishingSpot: Найден игрок - {player.name}");
        }
        else
        {
            Debug.LogError("FishingSpot: Игрок не найден!");
        }
    }

    void Update()
    {
        // Проверяем, можно ли начать рыбалку
        if (isPlayerNear && !isFishing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartFishing();
            }
        }

        // Для теста: нажмите F для принудительного запуска
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Тест: принудительный запуск полной последовательности");
            StartCoroutine(TestFullSequence());
        }
    }

    void StartFishing()
    {
        if (isFishing) return;

        isFishing = true;
        Debug.Log("FishingSpot: Начало рыбалки...");

        if (fishingUI != null)
        {
            fishingUI.SetActive(false);
        }

        StartCoroutine(FishingProcess());
    }

    IEnumerator FishingProcess()
    {
        // Ждем время рыбалки
        yield return new WaitForSeconds(fishingTime);

        // Проверяем успешность рыбалки (80% шанс)
        bool catchSuccess = Random.Range(0f, 1f) <= successChance;

        if (catchSuccess)
        {
            Debug.Log("FishingSpot: Рыба поймана!");

            // Показываем рыбу у игрока на 2 секунды
            yield return StartCoroutine(ShowFishAtPlayer());

            // Проверяем шанс появления NPC (20%)
            bool spawnNPC = Random.Range(0f, 1f) <= npcSpawnChance;

            if (spawnNPC && npcPrefab != null && npcSpawnPoint != null)
            {
                Debug.Log("FishingSpot: Случилось чудо! Появится NPC!");
                yield return StartCoroutine(SpawnNPCAndWait());
            }
            else
            {
                Debug.Log("FishingSpot: NPC не появился (не выпал шанс или нет префаба)");
            }
        }
        else
        {
            Debug.Log("FishingSpot: Рыба сорвалась!");
        }

        isFishing = false;

        if (fishingUI != null && isPlayerNear)
        {
            fishingUI.SetActive(true);
        }
    }

    IEnumerator ShowFishAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && fishPrefab != null)
        {
            // Создаем рыбу у игрока
            spawnedFish = Instantiate(fishPrefab, player.transform);
            spawnedFish.transform.localPosition = Vector3.up * 2f;
            Debug.Log("FishingSpot: Рыба появилась у игрока");

            // Ждем 2 секунды
            yield return new WaitForSeconds(fishAttachTime);

            // Уничтожаем рыбу
            Destroy(spawnedFish);
            Debug.Log("FishingSpot: Рыба исчезла");
        }
    }

    IEnumerator SpawnNPCAndWait()
    {
        // 1. Блокируем движение игрока
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("FishingSpot: Движение игрока заблокировано");
        }

        // 2. Создаем NPC в указанной точке
        GameObject npcObject = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        currentNPC = npcObject.GetComponent<NPCController>();

        if (currentNPC == null)
        {
            currentNPC = npcObject.AddComponent<NPCController>();
        }

        Debug.Log($"FishingSpot: NPC создан в точке {npcSpawnPoint.position}");

        // 3. Заставляем NPC подойти к игроку
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentNPC.StartApproach(player.transform);

            // Ждем, пока NPC подойдет к игроку
            yield return new WaitUntil(() => currentNPC.HasReachedPlayer());
            Debug.Log("FishingSpot: NPC подошел к игроку");
        }

        // 4. NPC стоит 5 секунд
        Debug.Log($"FishingSpot: NPC стоит у игрока {npcWaitTime} секунд");
        yield return new WaitForSeconds(npcWaitTime);

        // 5. NPC уходит
        Debug.Log("FishingSpot: NPC уходит");
        currentNPC.GoAway();

        // Ждем пока NPC уйдет
        yield return new WaitForSeconds(2f);

        // 6. Разрешаем игроку ходить
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("FishingSpot: Движение игрока восстановлено");
        }
    }

    IEnumerator TestFullSequence()
    {
        Debug.Log("=== ТЕСТ ПОЛНОЙ ПОСЛЕДОВАТЕЛЬНОСТИ ===");

        // Блокируем движение
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Создаем NPC для теста
        if (npcPrefab != null && npcSpawnPoint != null)
        {
            GameObject npcObject = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
            currentNPC = npcObject.GetComponent<NPCController>();

            if (currentNPC == null)
            {
                currentNPC = npcObject.AddComponent<NPCController>();
            }

            Debug.Log("Тест: NPC создан");

            // NPC подходит к игроку
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                currentNPC.StartApproach(player.transform);

                yield return new WaitUntil(() => currentNPC.HasReachedPlayer());
                Debug.Log("Тест: NPC подошел к игроку");

                // Ждем 5 секунд
                yield return new WaitForSeconds(5f);

                // NPC уходит
                currentNPC.GoAway();

                yield return new WaitForSeconds(2f);
            }
        }

        // Восстанавливаем движение
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Debug.Log("=== ТЕСТ ЗАВЕРШЕН ===");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (fishingUI != null)
                fishingUI.SetActive(true);

            Debug.Log("FishingSpot: Игрок вошел в зону рыбалки");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (fishingUI != null)
                fishingUI.SetActive(false);

            // Сбрасываем рыбалку, если игрок ушел
            if (isFishing)
            {
                isFishing = false;
                StopAllCoroutines();

                // Уничтожаем рыбу, если она была
                if (spawnedFish != null)
                {
                    Destroy(spawnedFish);
                }

                // Уничтожаем NPC, если он был
                if (currentNPC != null)
                {
                    Destroy(currentNPC.gameObject);
                }

                // Восстанавливаем движение
                if (playerMovement != null)
                {
                    playerMovement.enabled = true;
                }

                Debug.Log("FishingSpot: Рыбалка прервана - игрок ушел");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Показываем точку появления NPC
        if (npcSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(npcSpawnPoint.position, 1f);
            Gizmos.DrawLine(transform.position, npcSpawnPoint.position);
        }
    }
}