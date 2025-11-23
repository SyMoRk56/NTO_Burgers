using UnityEngine;
using System.Collections.Generic;

public class DeskInteraction : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas deskCanvas;
    public List<GameObject> randomImages;

    [Header("Player References")]
    public PlayerMovement playerMovement;
    public MonoBehaviour playerCameraScript;

    private bool playerInRange = false;
    private bool isCanvasOpen = false;

    void Start()
    {
        if (deskCanvas != null)
            deskCanvas.gameObject.SetActive(false);

        HideAllImages();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isCanvasOpen)
                OpenDeskCanvas();
        }

        if (isCanvasOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDeskCanvas();
        }
    }

    void OpenDeskCanvas()
    {
        isCanvasOpen = true;

        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(true);
            //Canvas.ForceUpdateCanvases();
            ShowRandomImages();
        }

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerCameraScript != null)
            playerCameraScript.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void CloseDeskCanvas()
    {
        isCanvasOpen = false;

        if (deskCanvas != null)
        {
            deskCanvas.gameObject.SetActive(false);
            HideAllImages();
        }

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerCameraScript != null)
            playerCameraScript.enabled = true;

        StartCoroutine(LockCursorNextFrame());
    }

    System.Collections.IEnumerator LockCursorNextFrame()
    {
        yield return null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ShowRandomImages()
    {
        HideAllImages();

        if (randomImages == null || randomImages.Count == 0)
            return;

        int imagesToShow = Random.Range(1, Mathf.Min(4, randomImages.Count + 1));
        List<GameObject> available = new List<GameObject>(randomImages);

        for (int i = 0; i < imagesToShow; i++)
        {
            if (available.Count == 0)
                break;

            int id = Random.Range(0, available.Count);
            GameObject img = available[id];

            img.SetActive(true);

            RectTransform rt = img.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = GetSafeScreenPosition(rt);
            }

            available.RemoveAt(id);
        }
    }

    Vector2 GetSafeScreenPosition(RectTransform rectTransform)
    {
        RectTransform canvasRect = deskCanvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 size = rectTransform.rect.size;

        float minX = -canvasSize.x / 2 + size.x / 2;
        float maxX = canvasSize.x / 2 - size.x / 2;
        float minY = -canvasSize.y / 2 + size.y / 2;
        float maxY = canvasSize.y / 2 - size.y / 2;

        return new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );
    }

    void HideAllImages()
    {
        if (randomImages == null)
            return;

        foreach (var img in randomImages)
            if (img != null)
                img.SetActive(false);
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
