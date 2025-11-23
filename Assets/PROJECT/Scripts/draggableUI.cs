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
    public float zoomScale = 1.7f; // На сколько увеличивать (70% = 1.7x)
    public float zoomDuration = 0.2f; // Длительность анимации увеличения

    private bool isZoomed = false;
    private Vector3 originalScale;
    private int originalSiblingIndex;
    private bool isDragging = false;

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

        // При начале перетаскивания ставим поверх всех
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

    // Обработка клика для увеличения/уменьшения
    public void OnPointerClick(PointerEventData eventData)
    {
        // Игнорируем клик если был драг
        if (isDragging) return;

        ToggleZoom();
    }

    void ToggleZoom()
    {
        if (isZoomed)
        {
            // Уменьшаем
            transform.localScale = originalScale;
            transform.SetSiblingIndex(originalSiblingIndex);
            isZoomed = false;
            StartCoroutine(TaskManager.Instance.GetPlayerNextLetter(new Task(recipient, address)));
        }
        else
        {
            // Увеличиваем
            transform.localScale = originalScale * zoomScale;
            transform.SetAsLastSibling(); // Ставим поверх других
            isZoomed = true;
        }
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