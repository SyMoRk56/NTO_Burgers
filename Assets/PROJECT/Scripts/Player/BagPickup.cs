using UnityEngine;

public class BagPickup : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance(); // Проверка дистанции
    }

    [Header("Bag Settings")]
    public GameObject bagPrefab;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Attachment Settings")]
    public string attachToChildName = "";
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;

    [Header("UI Settings")]
    public GameObject pickupPrompt;

    private bool playerInRange = false;
    private GameObject player;

    public Door h;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Ищем игрока

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
        }

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false); // Скрываем UI

        h.enabled = false; // Отключаем дверь
    }

    void Update()
    {
        // старый способ через кнопку (не используется)
        //if (playerInRange && Input.GetKeyDown(pickupKey))
        //{
        //    PickUpBag();
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (pickupPrompt != null)
                pickupPrompt.SetActive(true); // Показываем подсказку
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (pickupPrompt != null)
                pickupPrompt.SetActive(false); // Скрываем подсказку
        }
    }

    private void PickUpBag()
    {
        if (player == null) return;

        // Проверка: есть ли уже сумка
        if (HasBagAlready())
        {
            Debug.Log("You already have a bag!");
            return;
        }

        if (bagPrefab != null)
        {
            Transform parentTransform = player.transform;

            // Поиск точки крепления
            if (!string.IsNullOrEmpty(attachToChildName))
            {
                Transform childTransform = FindChildRecursive(player.transform, attachToChildName);
                if (childTransform != null)
                {
                    parentTransform = childTransform;
                }
            }

            // Создаём сумку
            GameObject bagInstance = Instantiate(bagPrefab, parentTransform);
            bagInstance.transform.localPosition = localPosition;
            bagInstance.transform.localEulerAngles = localRotation;

            Debug.Log("Bag picked up!");

            print("SetH Enabled " + PlayerMailInventory.Instance.carriedMails[0].id);

            // Туториал логика
            if (PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_1")
            {
                PlayerMailInventory.Instance.RemoveFirstMail();
                TaskManager.Instance.RemoveTask("Tutorial_1");
            }

            try
            {
                // Активация объекта на сцене
                GameObject.Find("Table").transform.parent.GetChild(2).gameObject.SetActive(true);
            }
            catch
            {
            }

            // Разблокируем карту
            FindFirstObjectByType<WallMap>().canPickup = true;

            TaskUI.Instance.SetHasBagUI(true); // Обновляем UI

            if (SaveGameManager.Instance != null)
                SaveGameManager.Instance.SaveAuto(true); // Автосохранение

            h.enabled = true; // Включаем дверь
        }

        Destroy(gameObject); // Удаляем объект подбора
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        // Рекурсивный поиск дочернего объекта
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    private bool HasBagAlready()
    {
        if (player == null) return false;

        // Проверка наличия сумки у игрока
        foreach (Transform child in player.transform)
        {
            if (child.CompareTag("Bag"))
            {
                return true;
            }
        }
        return false;
    }

    public int InteractPriority()
    {
        return 0; // Приоритет взаимодействия
    }

    public bool CheckInteract()
    {
        return !HasBagAlready(); // Можно взаимодействовать если нет сумки
    }

    public void Interact()
    {
        PickUpBag(); // Основное действие
    }

    public void OnBeginInteract()
    {
    }

    public void OnEndInteract(bool success)
    {
    }
}