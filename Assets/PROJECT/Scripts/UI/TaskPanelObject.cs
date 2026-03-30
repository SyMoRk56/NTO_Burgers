using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TaskPanelObject : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Flip Settings")]
    public float duration = 0.4f;
    public Ease ease = Ease.InOutQuad;

    [Header("Sides")]
    public GameObject front;
    public GameObject back;

    [Header("Drag Settings")]
    public Canvas canvas; // нужен для правильного перемещения
    public float dragThreshold = 5f; // чтобы отличать клик от драга

    private bool isFlipped = false;
    private bool isAnimating = false;

    private bool isDragging = false;
    private Vector2 startClickPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        front = transform.GetChild(0).gameObject;
        back = transform.GetChild(1).gameObject;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = transform.parent.gameObject.GetComponent<Canvas>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // ================= CLICK =================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating || isDragging) return;

        Flip();
    }

    public void Flip()
    {
        isAnimating = true;

        float targetY = isFlipped ? 0f : 180f;

        transform.DORotate(new Vector3(0, targetY / 2f, 0), duration / 2f)
            .SetEase(ease)
            .OnComplete(() =>
            {
                // смена стороны на середине
                front.SetActive(isFlipped);
                back.SetActive(!isFlipped);

                transform.DORotate(new Vector3(0, targetY, 0), duration / 2f)
                    .SetEase(ease)
                    .OnComplete(() =>
                    {
                        isFlipped = !isFlipped;
                        isAnimating = false;
                    });
            });
    }

    // ================= DRAG =================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        startClickPosition = eventData.position;

        // если сразу двигаем — считаем drag
        if (Vector2.Distance(startClickPosition, eventData.position) >= dragThreshold)
        {
            StartDrag();
        }
        else
        {
            StartDrag(); // можно упростить: всегда считаем drag при начале
        }
    }

    void StartDrag()
    {
        isDragging = true;

        canvasGroup.blocksRaycasts = false; // чтобы можно было дропать на другие UI
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // корректное движение в UI
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}