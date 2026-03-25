using UnityEngine;
using System.Collections;

public class Bench : MonoBehaviour
{
    [Header("Настройки камеры")]
    public Camera mainCamera;
    public Camera specialCamera;
    public bool autoFindMainCamera = true;

    [Header("Объект для анимации")]
    public GameObject objectToAnimate;

    [Header("Объект для активации после анимации")]
    public GameObject objectToActivateAfter;
    public bool activateObjectAfterAnimation = true;

    [Header("Сохранение")]
    public string objectId = "Bench_1"; // Уникальный ID для сохранения

    [Header("Тайминги")]
    public float sequenceDuration = 8f;
    public float cameraSwitchDelay = 0.5f;
    public float objectActivationDelay = 1f;

    [Header("Аудио")]
    public AudioClip sequenceSound;
    public float soundVolume = 1f;
    private AudioSource audioSource;

    private bool sequenceActive = false;
    private bool wasActivated = false; // Флаг для предотвращения повторной активации

    [Header("Настройки игрока")]
    public bool disablePlayerControl = true;

    private PlayerMovement playerMovement;
    private GameObject player;

    void Start()
    {
        Debug.Log($"[skamia] Инициализация на объекте: {gameObject.name}");

        // Проверяем сохранение
        CheckSaveState();

        if (disablePlayerControl)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        if (autoFindMainCamera && mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(false);
        }

        // Начальное состояние объектов
        UpdateObjectStates();
    }

    // Проверяем состояние из сохранения
    void CheckSaveState()
    {
        if (ObjectStateManager.Instance != null && !string.IsNullOrEmpty(objectId))
        {
            wasActivated = ObjectStateManager.Instance.IsObjectActivated(objectId);
            if (wasActivated)
            {
                Debug.Log($"[skamia] Объект {objectId} уже активирован в сохранении");
            }
        }
    }

    // Обновляем видимость объектов
    void UpdateObjectStates()
    {
        if (wasActivated)
        {
            // Если объект уже был активирован
            if (objectToAnimate != null)
                objectToAnimate.SetActive(false);

            if (objectToActivateAfter != null && activateObjectAfterAnimation)
                objectToActivateAfter.SetActive(true);
        }
        else
        {
            // Если объект еще не активирован
            if (objectToAnimate != null)
                objectToAnimate.SetActive(false);

            if (objectToActivateAfter != null && activateObjectAfterAnimation)
                objectToActivateAfter.SetActive(false);
        }
    }

    void Update()
    {
        // Тестовая клавиша
        if (Input.GetKeyDown(KeyCode.U) && !sequenceActive && !wasActivated)
        {
            Debug.Log($"[skamia] Тестовый запуск по клавише U");
            TriggerCameraSequence();
        }
    }

    public void TriggerCameraSequence()
    {
        if (!sequenceActive && !wasActivated)
        {
            Debug.Log($"[skamia] Запуск кинопоследовательности для скамейки");
            StartCoroutine(CameraSequence());
        }
    }

    IEnumerator CameraSequence()
    {
        if (sequenceActive || wasActivated) yield break;

        sequenceActive = true;
        Debug.Log("=== [skamia] НАЧАЛО КИНОСЦЕНЫ СКАМЕЙКИ ===");

        // 1. Блокируем управление игроком
        if (disablePlayerControl && playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // 2. Задержка перед переключением
        yield return new WaitForSeconds(cameraSwitchDelay);

        // 3. Переключаем камеру
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }

        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(true);

            // Направляем камеру на анимируемый объект
            if (objectToAnimate != null)
            {
                specialCamera.transform.LookAt(objectToAnimate.transform);
            }
        }

        // 4. Активируем и анимируем объект
        if (objectToAnimate != null)
        {
            objectToAnimate.SetActive(true);

            // Запускаем анимацию, если есть
            Animator benchAnimator = objectToAnimate.GetComponent<Animator>();
            if (benchAnimator != null)
            {
                benchAnimator.SetTrigger("BenchAppear");
                Debug.Log($"[skamia] Запущена анимация BenchAppear для объекта: {objectToAnimate.name}");
            }
        }

        // 5. Звук
        if (sequenceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sequenceSound, soundVolume);
        }

        // 6. Ждем основное время анимации
        yield return new WaitForSeconds(sequenceDuration - objectActivationDelay);

        // 7. Активируем постоянную скамейку и СОХРАНЯЕМ
        if (activateObjectAfterAnimation && objectToActivateAfter != null)
        {
            Debug.Log($"[skamia] Активация постоянной скамейки: {objectToActivateAfter.name}");
            objectToActivateAfter.SetActive(true);

            // Можно добавить дополнительную анимацию для скамейки
            Animator finalBenchAnimator = objectToActivateAfter.GetComponent<Animator>();
            if (finalBenchAnimator != null)
            {
                finalBenchAnimator.SetTrigger("FinalAppear");
            }

            // Деактивируем анимируемый объект
            if (objectToAnimate != null)
            {
                objectToAnimate.SetActive(false);
            }

            // 8. СОХРАНЯЕМ ФАКТ АКТИВАЦИИ
            if (!string.IsNullOrEmpty(objectId) && ObjectStateManager.Instance != null)
            {
                ObjectStateManager.Instance.MarkObjectAsActivated(objectId);
                wasActivated = true;
                Debug.Log($"[skamia] Сохранено состояние объекта: {objectId}");
            }

            // Ждем оставшееся время
            yield return new WaitForSeconds(objectActivationDelay);
        }
        else
        {
            yield return new WaitForSeconds(objectActivationDelay);
        }

        // 9. Возвращаем камеру
        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(false);
        }

        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        if (disablePlayerControl && playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Debug.Log("=== [skamia] КИНОСЦЕНА СКАМЕЙКИ ЗАВЕРШЕНА ===");
        sequenceActive = false;
    }

    // ДОБАВЛЕНО: Метод для сброса камеры
    public void ResetCamera()
    {
        if (specialCamera != null)
        {
            specialCamera.gameObject.SetActive(false);
        }

        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }

        if (disablePlayerControl && playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Debug.Log("[skamia] Камера сброшена");
    }

    // ДОБАВЛЕНО: Метод для полного сброса сцены
    public void ResetScene()
    {
        Debug.Log($"[skamia] Полный сброс сцены");

        StopAllCoroutines();
        ResetCamera();

        // Сбрасываем состояние
        wasActivated = false;

        // Обновляем объекты
        if (objectToAnimate != null)
        {
            objectToAnimate.SetActive(false);
        }

        if (objectToActivateAfter != null && activateObjectAfterAnimation)
        {
            objectToActivateAfter.SetActive(false);
        }

        sequenceActive = false;
    }

    // ДОБАВЛЕНО: Метод для остановки сцены без сброса объектов
    public void StopScene()
    {
        Debug.Log($"[skamia] Остановка сцены (без сброса объектов)");

        StopAllCoroutines();
        ResetCamera();

        // НЕ ВОССТАНАВЛИВАЕМ ОБЪЕКТЫ!
        // Они остаются в текущем состоянии

        sequenceActive = false;
    }

    [ContextMenu("Тест: Запустить кинопоследовательность скамейки")]
    void TestSequence()
    {
        if (Application.isPlaying && !sequenceActive && !wasActivated)
        {
            TriggerCameraSequence();
        }
    }

    [ContextMenu("Сбросить состояние")]
    void ResetState()
    {
        if (ObjectStateManager.Instance != null && !string.IsNullOrEmpty(objectId))
        {
            // Очищаем состояние в менеджере
            wasActivated = false;
            UpdateObjectStates();
            Debug.Log($"Состояние объекта {objectId} сброшено");
        }
    }
    // В оба скрипта добавьте этот метод
    public void ForceUpdateFromSave()
    {
        CheckSaveState();
        UpdateObjectStates();

        if (wasActivated)
        {
            Debug.Log($"Объект {objectId} принудительно обновлен из сохранения. Активен: {wasActivated}");
        }
    }
}   