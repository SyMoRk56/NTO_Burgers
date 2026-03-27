using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class DeskUI : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

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
        if (isCanvasOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseDeskCanvas();
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
            if (child.CompareTag(tag)) return child;
            Transform result = FindChildWithTag(child, tag);
            if (result != null) return result;
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

        // Туториальная проверка — убираем Tutorial_2 если он есть
        if (PlayerMailInventory.Instance != null &&
            PlayerMailInventory.Instance.carriedMails.Count > 0 &&
            PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_2")
        {
            PlayerMailInventory.Instance.RemoveFirstMail();
        }
    }

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

    public void ForceCloseDesk()
    {
        if (isCanvasOpen) CloseDeskCanvas();
    }

    IEnumerator LockCursorNextFrame()
    {
        yield return null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void DisableMainCameraSystems()
    {
        CameraSwitcher switcher = FindObjectOfType<CameraSwitcher>();
        if (switcher != null) switcher.enabled = false;

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null) cameraController.enabled = false;

        if (playerCameraScript != null) playerCameraScript.enabled = false;
    }

    private void EnableMainCameraSystems()
    {
        CameraSwitcher switcher = FindObjectOfType<CameraSwitcher>();
        if (switcher != null) switcher.enabled = true;

        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null) cameraController.enabled = true;

        if (playerCameraScript != null) playerCameraScript.enabled = true;
    }

    // ── Показываем только письма которые лежат на столе сегодня ──────────
    private void ShowAvailableMails()
    {
        ClearAllLetters();

        if (DailyMailScheduler.Instance == null)
        {
            Debug.LogError("[DeskUI] DailyMailScheduler не найден!");
            return;
        }

        var available = DailyMailScheduler.Instance.GetAvailableForDesk();
        Debug.Log($"[DeskUI] Писем на столе: {available.Count}");

        foreach (var mail in available)
        {
            // Не показываем то что уже в инвентаре игрока
            if (PlayerMailInventory.Instance != null &&
                PlayerMailInventory.Instance.ContainsTask(mail.id))
                continue;

            CreateMailUI(mail);
        }
    }

    private void CreateMailUI(MailItem mail)
    {
        if (letterPrefab == null) return;

        var letter = Instantiate(letterPrefab, transform.GetChild(0).GetChild(0));
        letter.transform.localPosition = Vector3.zero;

        var drag = letter.GetComponent<DeskLetterUI>();
        if (drag != null)
        {
            drag.recipient = mail.reciever;
            drag.id = mail.id;
            drag.address = mail.adress;
        }

        var button = letter.GetComponent<Button>();
        if (button == null) button = letter.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => TakeMailFromDesk(mail));

        randomImages.Add(letter);
    }

    // Игрок берёт письмо со стола — уведомляем DailyMailScheduler
    private void TakeMailFromDesk(MailItem mail)
    {
        if (DailyMailScheduler.Instance == null) return;

        // DailyMailScheduler сам добавит в TaskManager
        DailyMailScheduler.Instance.TakeMailFromDesk(mail.id);

        // Добавляем в инвентарь игрока
        if (PlayerMailInventory.Instance != null)
        {
            var task = new Task(mail.reciever, mail.adress, mail.id);
            PlayerMailInventory.Instance.AddMailToInventory(task);
        }

        RemoveMailFromUI(mail.id);
        Debug.Log($"[DeskUI] Письмо взято: {mail.id}");
    }

    private void RemoveMailFromUI(string taskId)
    {
        for (int i = randomImages.Count - 1; i >= 0; i--)
        {
            var img = randomImages[i];
            if (img == null) continue;
            var drag = img.GetComponent<DeskLetterUI>();
            if (drag != null && drag.id == taskId)
            {
                Destroy(img.gameObject);
                randomImages.RemoveAt(i);
                break;
            }
        }
    }

    private void ClearAllLetters()
    {
        foreach (var img in randomImages)
            if (img != null) Destroy(img.gameObject);
        randomImages.Clear();
    }

    public void PlayerEntered() { playerInRange = true; }

    public void PlayerExited()
    {
        playerInRange = false;
        if (isCanvasOpen) CloseDeskCanvas();
    }

    public int InteractPriority() => 0;
    public bool CheckInteract() => HasBag();
    public void Interact() => OpenDeskCanvas();
    public void OnBeginInteract() { }
    public void OnEndInteract(bool success) { }
}