using UnityEngine;
using System.Collections;

public class CameraSequenceHandler : MonoBehaviour
{
    [Header("Настройки камеры")]
    public Camera mainCamera; // Основная камера (если не назначена - будет Camera.main)
    public Camera specialCamera; // Специальная камера для последовательности
    public bool autoFindMainCamera = true;

    [Header("Объект для активации")]
    public GameObject objectToActivate; // Объект, который нужно активировать

    [Header("Тайминги")]
    public float sequenceDuration = 10f; // Длительность последовательности
    public float cameraSwitchDelay = 0.5f; // Задержка перед переключением камеры

    [Header("Аудио")]
    public AudioClip sequenceSound; // Звук для последовательности
    public float soundVolume = 1f;
    private AudioSource audioSource;

    // Приватные переменные
    private bool sequenceActive = false;
    private DialogueRunner dialogueRunner;
    private bool dialogueWasActiveLastFrame = false;
    private float dialogueStartTime = 0f;

    [Header("Автопоиск и настройки")]
    public bool autoFindDialogueRunner = true;
    public bool disablePlayerControl = true;

    // Ссылки на игрока (опционально)
    private PlayerMovement playerMovement;
    private GameObject player;

    void Start()
    {
        // Находим игрока если нужно
        if (disablePlayerControl)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        // Находим DialogueRunner
        if (autoFindDialogueRunner)
        {
            dialogueRunner = GetComponent<DialogueRunner>();
        }

        // Находим основную камеру если не назначена
        if (autoFindMainCamera && mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Проверяем и находим AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // Отключаем специальную камеру и объект на старте
        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(false);
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }

    void Update()
    {
        // Тестовая клавиша для запуска последовательности
        if (Input.GetKeyDown(KeyCode.Y) && !sequenceActive)
        {
            Debug.Log("Тест: запуск последовательности камеры по нажатию Y");
            StartCoroutine(CameraSequence());
        }

        CheckDialogueStatus();
    }

    void CheckDialogueStatus()
    {
        if (sequenceActive || dialogueRunner == null) return;

        bool isDialogueActive = dialogueRunner.IsDialogueActive;

        if (!dialogueWasActiveLastFrame && isDialogueActive)
        {
            dialogueStartTime = Time.time;
        }
        else if (dialogueWasActiveLastFrame && !isDialogueActive)
        {
            float dialogueDuration = Time.time - dialogueStartTime;
            Debug.Log($"Диалог завершился. Длительность: {dialogueDuration:F1} сек");
            StartCoroutine(CameraSequence());
        }

        dialogueWasActiveLastFrame = isDialogueActive;
    }

    IEnumerator CameraSequence()
    {
        if (sequenceActive) yield break;
        sequenceActive = true;

        Debug.Log("=== НАЧАЛО ПОСЛЕДОВАТЕЛЬНОСТИ КАМЕРЫ ===");

        // 1. Блокируем управление игроком (если нужно)
        if (disablePlayerControl && playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("Управление игроком отключено");
        }

        // 2. Небольшая задержка перед переключением
        yield return new WaitForSeconds(cameraSwitchDelay);

        // 3. Переключаем камеру на специальную
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
            Debug.Log("Основная камера отключена");
        }

        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(true);
            Debug.Log("Специальная камера включена");
        }

        // 4. Активируем объект
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log("Объект активирован: " + objectToActivate.name);
        }

        // 5. Проигрываем звук (если есть)
        if (sequenceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sequenceSound, soundVolume);
            Debug.Log("Звук последовательности проигран");
        }

        // 6. Ждем указанное время
        Debug.Log($"Ожидание {sequenceDuration} секунд...");
        yield return new WaitForSeconds(sequenceDuration);

        // 7. Возвращаем все как было

        // Деактивируем объект
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
            Debug.Log("Объект деактивирован");
        }

        // Возвращаем камеру
        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(false);
            Debug.Log("Специальная камера отключена");
        }

        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            Debug.Log("Основная камера включена");
        }

        // 8. Возвращаем управление игроку
        if (disablePlayerControl && playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("Управление игроком восстановлено");
        }

        Debug.Log("=== ПОСЛЕДОВАТЕЛЬНОСТЬ КАМЕРЫ ЗАВЕРШЕНА ===");
        sequenceActive = false;
    }

    // Публичный метод для запуска из других скриптов
    public void TriggerCameraSequence()
    {
        if (!sequenceActive)
        {
            StartCoroutine(CameraSequence());
        }
    }

    // Метод для запуска из инспектора (для тестов)
    [ContextMenu("Запустить последовательность камеры")]
    void TriggerSequenceEditor()
    {
        if (Application.isPlaying && !sequenceActive)
        {
            StartCoroutine(CameraSequence());
        }
        else if (!Application.isPlaying)
        {
            Debug.LogWarning("Метод работает только в режиме Play");
        }
    }

    // Опционально: метод для принудительного завершения последовательности
    public void ForceEndSequence()
    {
        if (sequenceActive)
        {
            StopAllCoroutines();

            // Возвращаем все как было
            if (objectToActivate != null)
                objectToActivate.SetActive(false);

            if (specialCamera != null)
                specialCamera.gameObject.SetActive(false);

            if (mainCamera != null)
                mainCamera.gameObject.SetActive(true);

            if (disablePlayerControl && playerMovement != null)
                playerMovement.enabled = true;

            sequenceActive = false;
            Debug.Log("Последовательность камеры принудительно завершена");
        }
    }

    void OnDestroy()
    {
        // При уничтожении объекта гарантируем, что камеры вернутся в исходное состояние
        if (specialCamera != null && specialCamera.gameObject.activeSelf)
        {
            specialCamera.gameObject.SetActive(false);
        }
    }
}