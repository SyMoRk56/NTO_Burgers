using UnityEngine;

public class BagPickup : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
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

    // ✅ ИСПРАВЛЕНО: Находим в Start вместо инспектора
    private EnterToHouse[] allHouseDoors;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
        }

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        // ✅ ИСПРАВЛЕНО: Находим все двери вместо одной ссылки
        allHouseDoors = FindObjectsByType<EnterToHouse>(FindObjectsSortMode.None);
        DisableAllDoors();
    }

    void DisableAllDoors()
    {
        if (allHouseDoors != null)
        {
            foreach (var door in allHouseDoors)
            {
                if (door != null) door.enabled = false;
            }
        }
    }

    void EnableAllDoors()
    {
        if (allHouseDoors != null)
        {
            foreach (var door in allHouseDoors)
            {
                if (door != null) door.enabled = true;
            }
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickUpBag();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (pickupPrompt != null)
                pickupPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupPrompt != null)
                pickupPrompt.SetActive(false);
        }
    }

    private void PickUpBag()
    {
        if (player == null) return;

        if (HasBagAlready())
        {
            Debug.Log("You already have a bag!");
            return;
        }

        if (bagPrefab != null)
        {
            Transform parentTransform = player.transform;

            if (!string.IsNullOrEmpty(attachToChildName))
            {
                Transform childTransform = FindChildRecursive(player.transform, attachToChildName);
                if (childTransform != null)
                {
                    parentTransform = childTransform;
                }
            }

            GameObject bagInstance = Instantiate(bagPrefab, parentTransform);
            bagInstance.transform.localPosition = localPosition;
            bagInstance.transform.localEulerAngles = localRotation;

            Debug.Log("Bag picked up!");

            // ✅ ИСПРАВЛЕНО: Безопасная проверка с Count
            if (PlayerMailInventory.Instance != null &&
                PlayerMailInventory.Instance.carriedMails.Count > 0)
            {
                Debug.Log("First mail ID: " + PlayerMailInventory.Instance.carriedMails[0].id);

                if (PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_1")
                {
                    PlayerMailInventory.Instance.RemoveFirstMail();
                }
            }

            // Разблокируем Tab
            if (TaskUI.Instance != null)
                TaskUI.Instance.SetHasBag(true);

            if (SaveGameManager.Instance != null)
                SaveGameManager.Instance.SaveAuto(true);

            // ✅ ИСПРАВЛЕНО: Включаем все двери
            EnableAllDoors();
        }

        Destroy(gameObject);
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
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

        foreach (Transform child in player.transform)
        {
            if (child.CompareTag("Bag"))
            {
                return true;
            }
        }
        return false;
    }

    public int InteractPriority() => 0;
    public bool CheckInteract() => !HasBagAlready();
    public void Interact() => PickUpBag();
    public void OnBeginInteract() { }
    public void OnEndInteract(bool success) { }
}