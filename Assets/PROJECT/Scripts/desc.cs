using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PostalDesk : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas deskCanvas;
    public GameObject interactionHint; // Сфера с подсказкой
    public List<GameObject> randomImages; // Список UI Image для случайного показа

    [Header("Player Reference")]
    public PlayerMovement playerMovement;
    public MonoBehaviour playerCameraScript; // Скрипт управления камерой

    private bool canInteract = false;
    private bool isCanvasOpen = false;

    void Start()
    {
        // Скрываем канвас и подсказку при старте
        if (deskCanvas != null)
            deskCanvas.gameObject.SetActive(false);

        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void Update()
    {
        // Проверяем возможность взаимодействия
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            if (!isCanvasOpen)
            {
                OpenDeskCanvas();
            }
        }

        // Закрытие канваса по ESC
        if (isCanvasOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDeskCanvas();
        }
    }

    void OpenDeskCanvas()
    {
        isCanvasOpen = true;

        // Активируем канвас
        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(true);
            ShowRandomImages();
        }

        // Блокируем движение игрока и камеру
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCameraScript != null)
        {
            playerCameraScript.enabled = false;
        }

        // Разблокируем и показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Скрываем подсказку взаимодействия
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void CloseDeskCanvas()
    {
        isCanvasOpen = false;

        // Деактивируем канвас
        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(false);
            HideAllImages();
        }

        // Разблокируем движение игрока и камеру
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerCameraScript != null)
        {
            playerCameraScript.enabled = true;
        }

        // Блокируем и скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Показываем подсказку если все еще в триггере
        if (canInteract && interactionHint != null)
            interactionHint.SetActive(true);
    }

    void ShowRandomImages()
    {
        // Сначала скрываем все изображения
        HideAllImages();

        // Показываем случайные изображения из списка
        if (randomImages != null && randomImages.Count > 0)
        {
            // Выбираем случайное количество изображений (1-3)
            int imagesToShow = Random.Range(1, Mathf.Min(4, randomImages.Count + 1));

            // Создаем временный список для случайного выбора
            List<GameObject> availableImages = new List<GameObject>(randomImages);

            for (int i = 0; i < imagesToShow; i++)
            {
                if (availableImages.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableImages.Count);
                    GameObject randomImage = availableImages[randomIndex];

                    // Активируем случайное изображение
                    if (randomImage != null)
                    {
                        randomImage.SetActive(true);

                        // Опционально: случайная позиция на канвасе
                        RectTransform rectTransform = randomImage.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition = new Vector2(
                                Random.Range(-200f, 200f),
                                Random.Range(-100f, 100f)
                            );
                        }
                    }

                    // Убираем из доступных чтобы не повторяться
                    availableImages.RemoveAt(randomIndex);
                }
            }
        }
    }

    void HideAllImages()
    {
        // Скрываем все изображения в списке
        if (randomImages != null)
        {
            foreach (GameObject image in randomImages)
            {
                if (image != null)
                    image.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;

            // Показываем подсказку взаимодействия
            if (interactionHint != null)
                interactionHint.SetActive(true);

            // Получаем ссылку на игрока если нет
            if (playerMovement == null)
            {
                playerMovement = other.GetComponent<PlayerMovement>();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;

            // Скрываем подсказку взаимодействия
            if (interactionHint != null)
                interactionHint.SetActive(false);

            // Если канвас открыт - закрываем его
            if (isCanvasOpen)
            {
                CloseDeskCanvas();
            }
        }
    }
}