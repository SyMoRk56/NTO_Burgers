using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 minDragBound;
    private Vector2 maxDragBound;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

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
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
