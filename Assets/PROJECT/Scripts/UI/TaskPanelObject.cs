using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Элемент панели задач (TaskPanelObject), который можно переворачивать и перемещать мышью.
/// Используется для отображения писем, заданий или рыбы в UI.
/// </summary>
public class TaskPanelObject : MonoBehaviour,
    IPointerClickHandler,    // Реагирует на клики
    IBeginDragHandler,       // Начало перетаскивания
    IDragHandler,            // Перетаскивание
    IEndDragHandler          // Окончание перетаскивания
{
    [Header("Flip Settings")]
    public float duration = 0.4f;           // Длительность анимации переворота
    public Ease ease = Ease.InOutQuad;      // Тип easing для плавности

    [Header("Sides")]
    public GameObject front;                // Передняя сторона карточки
    public GameObject back;                 // Задняя сторона карточки

    [Header("Drag Settings")]
    public Canvas canvas;                   // Канвас нужен для корректного расчета перемещения
    public float dragThreshold = 5f;        // Порог для отличия клика от драга

    private bool isFlipped = false;         // Состояние переворота карточки
    private bool isAnimating = false;       // Флаг, чтобы не мешать текущей анимации
    private bool isDragging = false;        // Флаг, что карточка перетаскивается
    private Vector2 startClickPosition;     // Начальная позиция клика для отслеживания драга

    private RectTransform rectTransform;    // Для управления позиционированием
    private CanvasGroup canvasGroup;        // Для управления альфой и блокировкой raycast

    void Awake()
    {
        // Предполагаем, что front и back — первые два дочерних объекта
        front = transform.GetChild(0).gameObject;
        back = transform.GetChild(1).gameObject;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Получаем канвас из родителя для правильной работы drag
        canvas = transform.parent.gameObject.GetComponent<Canvas>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // ================= CLICK =================
    /// <summary>
    /// Обработка клика по объекту
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Игнорируем клик, если объект анимируется или перетаскивается
        if (isAnimating || isDragging) return;

        Flip();
    }

    /// <summary>
    /// Запуск анимации переворота карточки
    /// </summary>
    public void Flip()
    {
        isAnimating = true;

        float targetY = isFlipped ? 0f : 180f;

        // Первая половина анимации (сжатие + поворот)
        transform.DORotate(new Vector3(0, targetY / 2f, 0), duration / 2f)
            .SetEase(ease)
            .OnComplete(() =>
            {
                // Переключаем стороны на середине анимации
                front.SetActive(isFlipped);
                back.SetActive(!isFlipped);

                // Вторая половина анимации (разворот)
                transform.DORotate(new Vector3(0, targetY, 0), duration / 2f)
                    .SetEase(ease)
                    .OnComplete(() =>
                    {
                        // Завершаем анимацию
                        isFlipped = !isFlipped;
                        isAnimating = false;
                    });
            });
    }

    // ================= DRAG =================
    /// <summary>
    /// Начало перетаскивания объекта
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        startClickPosition = eventData.position;

        // Можно считать drag сразу, без проверки threshold
        StartDrag();
    }

    /// <summary>
    /// Установка флагов и визуальных эффектов при начале драга
    /// </summary>
    void StartDrag()
    {
        isDragging = true;

        // Разрешаем перетаскивание поверх других UI и делаем полупрозрачным
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    /// <summary>
    /// Обработка перемещения мышью
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Корректное перемещение по UI с учетом масштаба канваса
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    /// <summary>
    /// Завершение перетаскивания
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        // Восстанавливаем блокировку raycast и прозрачность
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}