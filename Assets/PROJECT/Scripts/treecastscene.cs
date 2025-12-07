using UnityEngine;
using System.Collections;

public class treecastscene : MonoBehaviour
{
    [Header("Настройки камеры")]
    public Camera mainCamera;
    public Camera specialCamera;
    public bool autoFindMainCamera = true;

    [Header("Объект для активации")]
    public GameObject objectToActivate; // Дерево яблони

    [Header("Тайминги")]
    public float sequenceDuration = 10f;
    public float cameraSwitchDelay = 0.5f;

    [Header("Аудио")]
    public AudioClip sequenceSound;
    public float soundVolume = 1f;
    private AudioSource audioSource;

    private bool sequenceActive = false;

    [Header("Настройки игрока")]
    public bool disablePlayerControl = true;

    private PlayerMovement playerMovement;
    private GameObject player;

    void Start()
    {
        Debug.Log($"[treecastscene] Инициализация на объекте: {gameObject.name}");

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
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

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
        // Тестовая клавиша
        if (Input.GetKeyDown(KeyCode.Y) && !sequenceActive)
        {
            Debug.Log($"[treecastscene] Тестовый запуск по клавише Y");
            StartCoroutine(CameraSequence());
        }
    }

    public void TriggerCameraSequence()
    {
        if (!sequenceActive)
        {
            Debug.Log($"[treecastscene] Запуск кинопоследовательности");
            StartCoroutine(CameraSequence());
        }
    }

    IEnumerator CameraSequence()
    {
        if (sequenceActive) yield break;

        sequenceActive = true;
        Debug.Log("=== [treecastscene] НАЧАЛО КИНОСЦЕНЫ ===");

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

            // Направляем камеру на дерево
            if (objectToActivate != null)
            {
                specialCamera.transform.LookAt(objectToActivate.transform);
            }
        }

        // 4. Активируем дерево
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);

            // Запускаем анимацию роста, если есть
            Animator treeAnimator = objectToActivate.GetComponent<Animator>();
            if (treeAnimator != null)
            {
                treeAnimator.SetTrigger("Grow");
            }
        }

        // 5. Звук
        if (sequenceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sequenceSound, soundVolume);
        }

        // 6. Ждем
        yield return new WaitForSeconds(sequenceDuration);

        // 7. Возвращаем все обратно
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }

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

        Debug.Log("=== [treecastscene] КИНОСЦЕНА ЗАВЕРШЕНА ===");
        sequenceActive = false;
    }

    [ContextMenu("Тест: Запустить кинопоследовательность")]
    void TestSequence()
    {
        if (Application.isPlaying && !sequenceActive)
        {
            TriggerCameraSequence();
        }
    }
}