using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 minDragBound;
    private Vector2 maxDragBound;

    public string recipient;
    public string address;
    public string id;
    public float zoomScale = 1.7f;
    public float zoomDuration = 0.2f;

    private bool isZoomed = false;
    private Vector3 originalScale;
    private int originalSiblingIndex;
    private bool isDragging = false;

    [Header("Button Reference")]
    public Button actionButton; // Ссылка на дочернюю кнопку

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Сохраняем оригинальные значения
        originalScale = transform.localScale;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Находим кнопку если не установлена в инспекторе
        if (actionButton == null)
            actionButton = GetComponentInChildren<Button>();

        // Делаем кнопку неактивной по умолчанию
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);

            // Добавляем обработчик нажатия на кнопку
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonClick);
        }

        CalculateDragBounds();
    }

    void CalculateDragBounds()
    {
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 size = rectTransform.rect.size;

        minDragBound = new Vector2(
            -canvasSize.x / 2 + size.x / 2,
            -canvasSize.y / 2 + size.y / 2
        );

        maxDragBound = new Vector2(
            canvasSize.x / 2 - size.x / 2,
            canvasSize.y / 2 - size.y / 2
        );
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        transform.SetAsLastSibling();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || canvas == null) return;

        Vector2 pos = rectTransform.anchoredPosition + eventData.delta / canvas.scaleFactor;

        pos.x = Mathf.Clamp(pos.x, minDragBound.x, maxDragBound.x);
        pos.y = Mathf.Clamp(pos.y, minDragBound.y, maxDragBound.y);

        rectTransform.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;
        ToggleZoom();
    }

    void ToggleZoom()
    {
        if (isZoomed)
        {
            // Уменьшаем и деактивируем кнопку
            transform.localScale = originalScale;
            transform.SetSiblingIndex(originalSiblingIndex);
            isZoomed = false;

            if (actionButton != null)
                actionButton.gameObject.SetActive(false);
        }
        else
        {
            // Увеличиваем и активируем кнопку
            transform.localScale = originalScale * zoomScale;
            transform.SetAsLastSibling();
            isZoomed = true;

            if (actionButton != null)
                actionButton.gameObject.SetActive(true);
        }
    }

    // Обработчик нажатия на кнопку
    private void OnActionButtonClick()
    {
        // Создаем задачу и передаем в TaskUI
        Task newTask = new Task(recipient, address, id);
        TaskUI.Instance.SetTask(newTask);

        // Можно также автоматически закрыть зум после нажатия
        if (isZoomed)
            ToggleZoom();
    }

    // Дополнительные методы для управления зумом из других скриптов
    public void ZoomIn()
    {
        if (!isZoomed)
            ToggleZoom();
    }

    public void ZoomOut()
    {
        if (isZoomed)
            ToggleZoom();
    }

    public bool IsZoomed()
    {
        return isZoomed;
    }

    public bool IsDragging()
    {
        return isDragging;
    }
}