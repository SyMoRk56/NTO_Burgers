using UnityEngine;
using System.Collections;

public class FlowerTriggerHandler : MonoBehaviour
{
    [Header("Настройки")]
    public string targetDialogueName = "flowertrig";
    public GameObject flowerPrefab;

    [Header("Скорости и задержки")]
    public float prefabSpeed = 5f;
    public float spawnHeight = 1f;
    public float sneezeDelay = 1f;

    [Header("Автопоиск")]
    public bool autoFindPlayer = true;
    public bool autoFindVFX = true;

    // Приватные переменные
    private bool sequenceTriggered = false;
    private GameObject currentFlower;
    private DialogueRunner dialogueRunner;
    private GameObject player;
    private ParticleSystem sneezeVFX;
    private PlayerMovement playerMovement;

    void Start()
    {
        // Автоматически находим компоненты
        if (autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        // Находим DialogueRunner
        dialogueRunner = GetComponent<DialogueRunner>();
        // Находим VFX для чихания по тегу
        if (autoFindVFX)
        {
            GameObject taggedObj = GameObject.FindGameObjectWithTag("snezy");

            if (taggedObj != null)
            {
                sneezeVFX = taggedObj.GetComponent<ParticleSystem>();
            }
            else
            {
                Debug.LogWarning("Объект с тегом 'snezy' не найден!");
            }
        }

        // Останавливаем VFX если он играет
        if (sneezeVFX != null && sneezeVFX.isPlaying)
        {
            sneezeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

    }

    void Update()
    {
        // Для тестирования: запускаем по нажатию T
        if (Input.GetKeyDown(KeyCode.T) && !sequenceTriggered)
        {
            Debug.Log("Тест: запуск последовательности по нажатию T");
            StartCoroutine(FlowerSequence());
        }

        // Мониторинг диалога
        CheckDialogueStatus();
    }

    void CheckDialogueStatus()
    {
        print(1);
        if (sequenceTriggered || dialogueRunner == null) return;
        // Проверяем, активен ли диалог
        print(dialogueRunner.gameObject.name);
        bool isDialogueActive = dialogueRunner.IsDialogueActive;
        print(2 + (dialogueWasActiveLastFrame).ToString() + isDialogueActive.ToString());

        // Сохраняем состояние в статической переменной
        if (!dialogueWasActiveLastFrame && isDialogueActive)
        {
            // Диалог только начался
            dialogueStartTime = Time.time;
        }
        else if (dialogueWasActiveLastFrame && !isDialogueActive)
        {
            print("ENDED");
            // Диалог только что завершился
            float dialogueDuration = Time.time - dialogueStartTime;

            // Проверяем, был ли это наш целевой диалог
            // Поскольку мы не знаем имя, будем проверять по времени
            // или просто запускать для любого завершенного диалога
            // (В реальном проекте нужно добавить поле в DialogueRunner)

            Debug.Log($"Диалог завершился. Длительность: {dialogueDuration:F1} сек");

            // Запускаем последовательность для любого диалога (для теста)
            // Или можно добавить проверку через рефлексию
            StartCoroutine(FlowerSequence());
        }

        dialogueWasActiveLastFrame = isDialogueActive;
    }

    // Статические переменные для отслеживания диалога
    private static bool dialogueWasActiveLastFrame = false;
    private static float dialogueStartTime = 0f;

    IEnumerator FlowerSequence()
    {
        if (sequenceTriggered) yield break;

        sequenceTriggered = true;
        Debug.Log("=== НАЧАЛО ПОСЛЕДОВАТЕЛЬНОСТИ ЦВЕТКА ===");

        // 1. Блокируем управление игроком
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("Управление игроком заблокировано");
        }

        // 2. Создаем префаб цветка
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeight;

        if (flowerPrefab != null)
        {
            currentFlower = Instantiate(flowerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Создан префаб цветка в позиции: {spawnPosition}");
        }
        else
        {
            // Создаем простой куб если префаба нет
            currentFlower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentFlower.transform.position = spawnPosition;
            currentFlower.transform.localScale = Vector3.one * 0.3f;
            currentFlower.GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log("Создан временный объект цветка (сфера)");
        }

        // 3. Двигаем цветок к игроку
        if (player != null && currentFlower != null)
        {
            Vector3 targetPosition = player.transform.position + Vector3.up * 1.5f;
            float distance = Vector3.Distance(spawnPosition, targetPosition);
            float travelTime = distance / prefabSpeed;
            float elapsedTime = 0f;

            Debug.Log($"Движение цветка к игроку. Дистанция: {distance:F1}, время: {travelTime:F1} сек");

            while (elapsedTime < travelTime && currentFlower != null)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / travelTime;

                // Плавное движение
                float smoothT = t * t * (3f - 2f * t); // Кубическая интерполяция

                currentFlower.transform.position = Vector3.Lerp(spawnPosition, targetPosition, smoothT);

                // Вращение
                currentFlower.transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);

                yield return null;
            }

            // Уничтожаем цветок
            if (currentFlower != null)
            {
                Destroy(currentFlower);
                Debug.Log("Цветок достиг игрока и уничтожен");
            }
        }

        // 4. Ждем перед чиханием
        Debug.Log($"Ожидание перед чиханием: {sneezeDelay} сек");
        yield return new WaitForSeconds(sneezeDelay);

        // 5. Активируем VFX чихания
        if (sneezeVFX != null)
        {
            if (player != null)
            {
                // Позиционируем VFX перед игроком
                Vector3 vfxPosition = player.transform.position +
                                     player.transform.forward * 0.5f +
                                     Vector3.up * 0.3f;
                sneezeVFX.transform.position = vfxPosition;

                // Направляем вперед от игрока
                sneezeVFX.transform.rotation = Quaternion.LookRotation(player.transform.forward);
            }

            sneezeVFX.Play();
            Debug.Log("VFX чихания активирован");

            // Ждем пока VFX проиграется
            yield return new WaitForSeconds(sneezeVFX.main.duration);

            // Останавливаем VFX
            sneezeVFX.Stop();
        }
        else
        {
            Debug.Log("VFX не найден. Пропускаем эффект чихания.");
        }

        // 6. Разблокируем управление
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("Управление игроком восстановлено");
        }

        Debug.Log("=== ПОСЛЕДОВАТЕЛЬНОСТЬ ЦВЕТКА ЗАВЕРШЕНА ===");
        sequenceTriggered = false;
    }

    // Публичный метод для запуска из других скриптов
    public void TriggerSequence()
    {
        if (!sequenceTriggered)
        {
            StartCoroutine(FlowerSequence());
        }
    }

    // Метод для тестирования в редакторе
    [ContextMenu("Запустить последовательность")]
    void TriggerSequenceEditor()
    {
        if (Application.isPlaying)
        {
            TriggerSequence();
        }
        else
        {
            Debug.LogWarning("Метод работает только в режиме Play");
        }
    }

    void OnDestroy()
    {
        // Очистка
        if (currentFlower != null)
        {
            Destroy(currentFlower);
        }
    }
}