using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class DeskUI : MonoBehaviour, IInteractObject
{
    // ===== Проверка дистанции до игрока для взаимодействия =====
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    [Header("Префаб письма")]
    public GameObject letterPrefab; // Префаб для писем, которые появляются на столе

    [Header("Настройки Canvas")]
    public Canvas deskCanvas;                   // Основной канвас стола
    public List<GameObject> randomImages = new(); // Список всех писем на UI для очистки

    [Header("Настройки камеры")]
    public Camera deskCamera;                   // Камера для просмотра стола
    public Vector3 cameraOffset = new Vector3(0, 3, 0); // Смещение камеры относительно стола
    public Vector3 cameraRotation = new Vector3(90, 0, 0); // Поворот камеры для удобного обзора

    [Header("Ссылки на игрока")]
    public PlayerMovement playerMovement;      // Скрипт движения игрока
    public MonoBehaviour playerCameraScript;   // Скрипт камеры игрока

    [Header("Ссылка на Interaction UI")]
    public InteractionUI interactionUI;        // UI, показывающий подсказки при подходе

    private bool playerInRange = false;        // Флаг, что игрок рядом
    private bool isCanvasOpen = false;         // Флаг, что стол открыт
    private GameObject player;                 // Ссылка на объект игрока

    public bool isInTable => isCanvasOpen;     // Публичный геттер состояния стола
    public bool IsCanvasOpen => isCanvasOpen;  // Публичный геттер состояния UI

    // ===== Инициализация =====
    IEnumerator Start()
    {
        // Скрываем канвас и камеру при старте
        if (deskCanvas != null)
            deskCanvas.gameObject.SetActive(false);

        if (deskCamera != null)
            deskCamera.gameObject.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player"); // Находим игрока по тегу

        yield return new WaitForSeconds(.2f); // Короткая задержка для инициализации

        // Скрываем специальный объект стола (например, для задания)
        var ou = GameObject.Find("Table (1)");
        if ((ou != null))
        {
            ou.SetActive(false);
        }

        // Проверяем наличие письма "Tutorial_2" и показываем объект стола при необходимости
        if (PlayerMailInventory.Instance.carriedMails.Contains(
            new Task("Tutorial_2", "Tutorial_2", "Tutorial_2", true)))
        {
            ou.SetActive(true);
        }
    }

    void Update()
    {
        // ===== Закрытие столового UI кнопкой ESC =====
        if (isCanvasOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDeskCanvas();
        }

        // ===== Открытие столового UI комментировано =====
        // if (playerInRange && Input.GetKeyDown(KeyCode.E))
        // {
        //     if (!isCanvasOpen && HasBag())
        //         OpenDeskCanvas();
        //     else if (!HasBag())
        //         Debug.Log("You need a bag to interact with the desk!");
        // }
    }

    // Проверка, есть ли у игрока сумка для взаимодействия
    private bool HasBag()
    {
        if (player == null) return false;
        return FindChildWithTag(player.transform, "Bag") != null;
    }

    // Рекурсивный поиск дочернего объекта с тегом
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

    // ===== Открытие столового UI =====
    void OpenDeskCanvas()
    {
        // Скрываем специальный объект стола
        var ou = GameObject.Find("Table (1)");
        if (ou != null)
            ou.SetActive(false);

        isCanvasOpen = true; // Флаг что UI открыт

        if (interactionUI != null)
            interactionUI.HidePopup(); // Скрываем подсказку при открытии

        // Активируем камеру стола
        if (deskCamera != null)
        {
            DisableMainCameraSystems(); // Выключаем камеру игрока
            deskCamera.gameObject.SetActive(true);
            deskCamera.transform.position = transform.position + cameraOffset;
            deskCamera.transform.rotation = Quaternion.Euler(cameraRotation);
        }

        // Активируем канвас и показываем письма
        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(true);
            ShowAvailableMails();
        }

        if (playerMovement != null)
            playerMovement.enabled = false; // Блокируем движение игрока

        Cursor.visible = true;             // Показываем курсор
        Cursor.lockState = CursorLockMode.None;

        // Убираем письмо "Tutorial_2" из инвентаря
        if (PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_2")
        {
            PlayerMailInventory.Instance.RemoveFirstMail();
            TaskManager.Instance.RemoveTask("Tutorial_2");
        }
    }

    // ===== Закрытие столового UI =====
    private void CloseDeskCanvas()
    {
        isCanvasOpen = false;

        // Деактивируем камеру стола и включаем системы игрока
        if (deskCamera != null)
        {
            deskCamera.gameObject.SetActive(false);
            EnableMainCameraSystems();
        }

        // Деактивируем канвас и очищаем письма
        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(false);
            ClearAllLetters();
        }

        if (playerMovement != null)
            playerMovement.enabled = true; // Разблокируем движение

        StartCoroutine(LockCursorNextFrame()); // Локируем курсор на следующем кадре

        if (interactionUI != null && playerInRange)
            interactionUI.ShowPopup(); // Показываем подсказку
    }

    // Внешнее закрытие UI стола
    public void ForceCloseDesk()
    {
        if (isCanvasOpen)
        {
            CloseDeskCanvas();
        }
    }

    // Задержка для блокировки курсора на следующем кадре
    System.Collections.IEnumerator LockCursorNextFrame()
    {
        yield return null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ===== Включение/выключение систем камеры игрока =====
    private void DisableMainCameraSystems()
    {
        CameraSwitcher switcher = FindObjectOfType<CameraSwitcher>();
        if (switcher != null)
            switcher.enabled = false;

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
            cameraController.enabled = false;

        if (playerCameraScript != null)
            playerCameraScript.enabled = false;
    }

    private void EnableMainCameraSystems()
    {
        CameraSwitcher switcher = FindObjectOfType<CameraSwitcher>();
        if (switcher != null)
            switcher.enabled = true;

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
            cameraController.enabled = true;

        if (playerCameraScript != null)
            playerCameraScript.enabled = true;
    }

    // ===== Показ доступных писем =====
    private void ShowAvailableMails()
    {
        ClearAllLetters(); // Очищаем предыдущие письма

        if (TaskManager.Instance != null && TaskManager.Instance.tasks.Count > 0)
        {
            var availableTasks = new List<Task>(TaskManager.Instance.tasks);

            foreach (var task in availableTasks)
            {
                if (!PlayerMailInventory.Instance.ContainsTask(task.id) && !task.id.Contains("Tutorial"))
                {
                    CreateMailUI(task); // Создаём UI для каждого письма
                }
            }
        }
    }

    // Создание UI письма на столе
    private void CreateMailUI(Task task)
    {
        if (letterPrefab == null) return;

        var letter = Instantiate(letterPrefab, transform.GetChild(0).GetChild(0));
        letter.transform.localPosition = Vector3.zero + new Vector3(Random.Range(-100, 100), Random.Range(-100, 100));

        var drag = letter.GetComponent<DeskLetterUI>();
        if (drag != null)
        {
            drag.recipient = task.recieverName;
            drag.id = task.id;
            drag.address = task.adress;
            drag.isStory = task.isStory;
        }

        var button = letter.GetComponent<Button>();
        if (button == null)
            button = letter.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => TakeMailToInventory(task));

        randomImages.Add(letter); // Добавляем письмо в список для очистки
    }

    // Добавление письма в инвентарь игрока
    private void TakeMailToInventory(Task task)
    {
        if (PlayerMailInventory.Instance != null)
        {
            PlayerMailInventory.Instance.AddMailToInventory(task);
            RemoveMailFromUI(task.id);
            Debug.Log($"Письмо добавлено в инвентарь: {task.recieverName} {task.isStory}");
        }
    }

    // Удаление письма с UI стола
    private void RemoveMailFromUI(string taskId)
    {
        for (int i = randomImages.Count - 1; i >= 0; i--)
        {
            var img = randomImages[i];
            if (img != null)
            {
                var draggable = img.GetComponent<DeskLetterUI>();
                if (draggable != null && draggable.id == taskId)
                {
                    Destroy(img.gameObject);
                    randomImages.RemoveAt(i);
                    break;
                }
            }
        }
    }

    // Удаление всех писем со стола
    private void ClearAllLetters()
    {
        foreach (var img in randomImages)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        randomImages.Clear();
    }

    // ===== Методы взаимодействия с игроком =====
    public void PlayerEntered()
    {
        playerInRange = true;
    }

    public void PlayerExited()
    {
        playerInRange = false;
        if (isCanvasOpen)
            CloseDeskCanvas(); // Закрываем стол при уходе игрока
    }

    public int InteractPriority() => 0;

    public bool CheckInteract()
    {
        return HasBag(); // Можно взаимодействовать только если есть сумка
    }

    public void Interact()
    {
        OpenDeskCanvas(); // Открытие столового UI
    }

    public void OnBeginInteract() { }

    public void OnEndInteract(bool success) { }
}