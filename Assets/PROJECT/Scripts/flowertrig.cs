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

    [Header("Звуки")]
    public AudioClip sneezeSound; // Звук чихания
    public float sneezeVolume = 1f;
    [SerializeField] private AudioSource audioSource; // Аудиоисточник (если не назначен - создадим)

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
    private playerAnimations playerAnim;

    void Start()
    {
        // Находим игрока
        if (autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
                playerAnim = player.GetComponent<playerAnimations>();
            }
        }

        // Находим DialogueRunner
        dialogueRunner = GetComponent<DialogueRunner>();

        // Находим VFX
        if (autoFindVFX)
        {
            GameObject taggedObj = GameObject.FindGameObjectWithTag("snezy");
            if (taggedObj != null)
                sneezeVFX = taggedObj.GetComponent<ParticleSystem>();
            else
                Debug.LogWarning("Объект с тегом 'snezy' не найден!");
        }

        if (sneezeVFX != null && sneezeVFX.isPlaying)
            sneezeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Проверяем аудиоисточник
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Создаем новый AudioSource, если нет
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D звук
                audioSource.maxDistance = 20f;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !sequenceTriggered)
        {
            Debug.Log("Тест: запуск последовательности по нажатию T");
            StartCoroutine(FlowerSequence());
        }

        CheckDialogueStatus();
    }

    void CheckDialogueStatus()
    {
        if (sequenceTriggered || dialogueRunner == null) return;

        bool isDialogueActive = dialogueRunner.IsDialogueActive;

        if (!dialogueWasActiveLastFrame && isDialogueActive)
        {
            dialogueStartTime = Time.time;
        }
        else if (dialogueWasActiveLastFrame && !isDialogueActive)
        {
            print("ENDED");
            float dialogueDuration = Time.time - dialogueStartTime;
            Debug.Log($"Диалог завершился. Длительность: {dialogueDuration:F1} сек");
            StartCoroutine(FlowerSequence());
        }

        dialogueWasActiveLastFrame = isDialogueActive;
    }

    private static bool dialogueWasActiveLastFrame = false;
    private static float dialogueStartTime = 0f;

    IEnumerator FlowerSequence()
    {
        if (sequenceTriggered) yield break;
        sequenceTriggered = true;

        Debug.Log("=== НАЧАЛО ПОСЛЕДОВАТЕЛЬНОСТИ ЦВЕТКА ===");

        // 1. Блокируем управление
        if (playerMovement != null)
            playerMovement.enabled = false;

        // 2. Спавн цветка
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeight;

        if (flowerPrefab != null)
            currentFlower = Instantiate(flowerPrefab, spawnPosition, Quaternion.identity);
        else
        {
            currentFlower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentFlower.transform.position = spawnPosition;
            currentFlower.transform.localScale = Vector3.one * 0.3f;
            currentFlower.GetComponent<Renderer>().material.color = Color.yellow;
        }

        // 3. Двигаем к игроку
        if (player != null && currentFlower != null)
        {
            Vector3 targetPosition = player.transform.position + Vector3.up * 1.5f;

            float distance = Vector3.Distance(spawnPosition, targetPosition);
            float travelTime = distance / prefabSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < travelTime && currentFlower != null)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / travelTime;
                float smoothT = t * t * (3f - 2f * t);

                currentFlower.transform.position = Vector3.Lerp(spawnPosition, targetPosition, smoothT);
                currentFlower.transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);

                yield return null;
            }

            if (currentFlower != null)
                Destroy(currentFlower);
        }

        // 4. Небольшая задержка перед чихом
        yield return new WaitForSeconds(sneezeDelay);

        // 5. ВКЛЮЧАЕМ АНИМАЦИЮ ЧИХА
        if (playerAnim != null)
            playerAnim.PlaySneezy();

        // 6. ПРОИГРЫВАЕМ ЗВУК ЧИХАНИЯ
        PlaySneezeSound();

        // 7. VFX Чихания
        if (sneezeVFX != null)
        {
            Vector3 pos = player.transform.position +
                          player.transform.forward * 0.5f +
                          Vector3.up * 0.3f;

            sneezeVFX.transform.position = pos;
            sneezeVFX.transform.rotation = Quaternion.LookRotation(player.transform.forward);
            sneezeVFX.Play();

            yield return new WaitForSeconds(sneezeVFX.main.duration);
            sneezeVFX.Stop();
        }
        else
        {
            // Если нет VFX, ждем 1 секунду для анимации чиха
            yield return new WaitForSeconds(1f);
        }

        // 8. ВОЗВРАТ АНИМАЦИИ
        if (playerAnim != null)
            playerAnim.HeroIdleAnim();

        // 9. Разблокировка управления
        if (playerMovement != null)
            playerMovement.enabled = true;

        Debug.Log("=== ПОСЛЕДОВАТЕЛЬНОСТЬ ЦВЕТКА ЗАВЕРШЕНА ===");
        sequenceTriggered = false;
    }

    private void PlaySneezeSound()
    {
        if (sneezeSound != null && audioSource != null)
        {
            // Можно добавить небольшую случайность в pitch для разнообразия
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(sneezeSound, sneezeVolume);
            Debug.Log("Звук чихания проигран");
        }
        else
        {
            if (sneezeSound == null)
                Debug.LogWarning("Не назначен звук чихания!");
            if (audioSource == null)
                Debug.LogWarning("Не найден AudioSource!");
        }
    }

    public void TriggerSequence()
    {
        if (!sequenceTriggered)
            StartCoroutine(FlowerSequence());
    }

    [ContextMenu("Запустить последовательность")]
    void TriggerSequenceEditor()
    {
        if (Application.isPlaying)
            TriggerSequence();
        else
            Debug.LogWarning("Метод работает только в режиме Play");
    }

    void OnDestroy()
    {
        if (currentFlower != null)
            Destroy(currentFlower);
    }
}