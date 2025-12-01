using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class DeskInteraction : MonoBehaviour
{
    public GameObject letterPrefab;
    [Header("Canvas Settings")]
    public Canvas deskCanvas;
    public List<GameObject> randomImages = new();

    [Header("Camera Settings")]
    public Camera deskCamera;
    public Vector3 cameraOffset = new Vector3(0, 3, 0);
    public Vector3 cameraRotation = new Vector3(90, 0, 0);

    [Header("Player References")]
    public PlayerMovement playerMovement;
    public MonoBehaviour playerCameraScript;

    [Header("Interaction UI Reference")]
    public InteractionUI interactionUI;

    private bool playerInRange = false;
    private bool isCanvasOpen = false;
    private GameObject player;

    public bool isInTable => isCanvasOpen;
    public bool IsCanvasOpen => isCanvasOpen;

    void Start()
    {
        if (deskCanvas != null)
            deskCanvas.gameObject.SetActive(false);

        if (deskCamera != null)
            deskCamera.gameObject.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isCanvasOpen && HasBag())
                OpenDeskCanvas();
            else if (!HasBag())
            {
                Debug.Log("You need a bag to interact with the desk!");
            }
        }

        if (isCanvasOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDeskCanvas();
        }
    }

    private bool HasBag()
    {
        if (player == null) return false;
        return FindChildWithTag(player.transform, "Bag") != null;
    }

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

    void OpenDeskCanvas()
    {
        isCanvasOpen = true;

        if (interactionUI != null)
            interactionUI.HidePopup();

        if (deskCamera != null)
        {
            DisableMainCameraSystems();
            deskCamera.gameObject.SetActive(true);
            deskCamera.transform.position = transform.position + cameraOffset;
            deskCamera.transform.rotation = Quaternion.Euler(cameraRotation);
        }

        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(true);
            ShowAvailableMails();
        }

        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Оставляем основной метод как private
    private void CloseDeskCanvas()
    {
        isCanvasOpen = false;

        if (deskCamera != null)
        {
            deskCamera.gameObject.SetActive(false);
            EnableMainCameraSystems();
        }

        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(false);
            ClearAllLetters();
        }

        if (playerMovement != null)
            playerMovement.enabled = true;

        StartCoroutine(LockCursorNextFrame());

        if (interactionUI != null && playerInRange)
            interactionUI.ShowPopup();
    }

    // Добавляем публичный метод для внешнего закрытия
    public void ForceCloseDesk()
    {
        if (isCanvasOpen)
        {
            CloseDeskCanvas();
        }
    }

    System.Collections.IEnumerator LockCursorNextFrame()
    {
        yield return null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void DisableMainCameraSystems()
    {
        CameraSwitcher switcher = FindObjectOfType<CameraSwitcher>();
        if (switcher != null)
            switcher.enabled = false;

        CameraManagement cameraManagement = FindObjectOfType<CameraManagement>();
        if (cameraManagement != null)
            cameraManagement.enabled = false;

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

        CameraManagement cameraManagement = FindObjectOfType<CameraManagement>();
        if (cameraManagement != null)
            cameraManagement.enabled = true;

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
            cameraController.enabled = true;

        if (playerCameraScript != null)
            playerCameraScript.enabled = true;
    }

    private void ShowAvailableMails()
    {
        ClearAllLetters();

        if (TaskManager.Instance != null && TaskManager.Instance.tasks.Count > 0)
        {
            var availableTasks = new List<Task>(TaskManager.Instance.tasks);

            foreach (var task in availableTasks)
            {
                if (!PlayerMailInventory.Instance.ContainsTask(task.id))
                {
                    CreateMailUI(task);
                }
            }
        }
    }

    private void CreateMailUI(Task task)
    {
        if (letterPrefab == null) return;

        var letter = Instantiate(letterPrefab, transform.GetChild(0).GetChild(0));
        letter.transform.localPosition = Vector3.zero;

        var drag = letter.GetComponent<DraggableUI>();
        if (drag != null)
        {
            drag.recipient = task.recieverName;
            drag.id = task.id;
            drag.address = task.adress;
        }

        var button = letter.GetComponent<Button>();
        if (button == null)
            button = letter.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => TakeMailToInventory(task));

        randomImages.Add(letter);
    }

    private void TakeMailToInventory(Task task)
    {
        if (PlayerMailInventory.Instance != null)
        {
            PlayerMailInventory.Instance.AddMailToInventory(task);
            RemoveMailFromUI(task.id);
            Debug.Log($"Письмо добавлено в инвентарь: {task.recieverName}");
        }
    }

    private void RemoveMailFromUI(string taskId)
    {
        for (int i = randomImages.Count - 1; i >= 0; i--)
        {
            var img = randomImages[i];
            if (img != null)
            {
                var draggable = img.GetComponent<DraggableUI>();
                if (draggable != null && draggable.id == taskId)
                {
                    Destroy(img.gameObject);
                    randomImages.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void ClearAllLetters()
    {
        foreach (var img in randomImages)
        {
            if (img != null)
                Destroy(img.gameObject);
        }
        randomImages.Clear();
    }

    public void PlayerEntered()
    {
        playerInRange = true;
    }

    public void PlayerExited()
    {
        playerInRange = false;
        if (isCanvasOpen)
            CloseDeskCanvas();
    }
}