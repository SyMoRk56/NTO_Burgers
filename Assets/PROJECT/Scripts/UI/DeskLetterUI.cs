using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Скрипт для UI письма на столе. 
/// Позволяет перетаскивать письмо, кликать для переворота и показывать дополнительные кнопки.
/// </summary>
public class DeskLetterUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ===== Ссылки на компоненты =====
    private RectTransform rectTransform; // RectTransform письма для позиционирования
    private Canvas canvas;               // Канвас, на котором находится письмо
    private CanvasGroup canvasGroup;     // CanvasGroup для прозрачности и блокировки лучей

    // ===== Границы перетаскивания =====
    private Vector2 minDragBound;        // Минимальные координаты письма
    private Vector2 maxDragBound;        // Максимальные координаты письма

    // ===== Данные письма =====
    public string recipient;             // Имя получателя письма
    public string address;               // Адрес получателя
    public string id;                    // Уникальный ID письма
    public bool isStory;                 // Флаг, является ли письмо сюжетным

    [Header("Flip Animation")]
    public float flipDuration = 0.2f;    // Длительность анимации переворота

    [Header("Back Side")]
    public GameObject backSide;          // Задняя сторона письма
    public TMP_Text receiverText;        // Текст имени получателя на обороте
    public TMP_Text addressText;         // Текст адреса на обороте

    [Header("Button Reference")]
    public Button actionButton;          // Кнопка действий на письме (например, добавить в инвентарь)

    [Header("Hover Effect")]
    public float hoverScale = 1.1f;      // Масштаб при наведении
    public float hoverDuration = 0.2f;   // Время анимации наведения

    // ===== Внутренние состояния =====
    private bool isFlipped = false;      // Флаг переворота письма
    private bool isDragging = false;     // Флаг перетаскивания
    private bool isAnimating = false;    // Флаг текущей анимации
    public Sprite baseImage, backImage;  // Спрайты лицевой и обратной стороны
    private Vector3 originalScale;       // Оригинальный масштаб письма

    // ===== Инициализация =====
    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Инициализация кнопки действия
        if (actionButton == null)
            actionButton = GetComponentInChildren<Button>();

        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonClick);
        }

        if (backSide != null)
            backSide.SetActive(false); // Скрываем оборот письма при старте

        CalculateDragBounds(); // Вычисляем границы перетаскивания
        Invoke(nameof(DelayedSetScale), Time.deltaTime); // Устанавливаем оригинальный масштаб
    }

    /// <summary>
    /// Установка пользовательских спрайтов письма
    /// </summary>
    public void SetCustomSprites(Sprite frontSide, Sprite backSide, string recieverText, string addressText)
    {
        baseImage = frontSide;
        backImage = backSide;
        this.receiverText.text = AdressConverter.Convert(recieverText);
        this.addressText.text = AdressConverter.Convert(addressText);
        GetComponent<Image>().sprite = isFlipped ? backImage : baseImage;
    }

    /// <summary>
    /// Задержка для установки оригинального масштаба и случайного переворота письма
    /// </summary>
    void DelayedSetScale()
    {
        originalScale = transform.localScale;

        if (Random.value > 0.6f)
        {
            InstaFlip();
        }
    }

    /// <summary>
    /// Вычисление границ перетаскивания письма
    /// </summary>
    void CalculateDragBounds()
    {
        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 parentSize = parentRect.rect.size;
        Vector2 size = rectTransform.rect.size;

        minDragBound = new Vector2(-parentSize.x / 2 + size.x / 2, -parentSize.y / 2 + size.y / 2);
        maxDragBound = new Vector2(parentSize.x / 2 - size.x / 2, parentSize.y / 2 - size.y / 2);
    }

    // ===== Перетаскивание =====
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        transform.SetAsLastSibling();        // Поднимаем письмо выше остальных
        canvasGroup.alpha = 0.6f;            // Полупрозрачность при перетаскивании
        canvasGroup.blocksRaycasts = false;  // Игнорируем лучи для UI элементов

        rectTransform.DOKill();
        rectTransform.DOScale(originalScale, hoverDuration);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        Vector2 delta = eventData.delta / canvas.scaleFactor;
        Vector2 newPos = rectTransform.anchoredPosition + delta;

        // Ограничиваем перемещение по границам
        newPos.x = Mathf.Clamp(newPos.x, minDragBound.x, maxDragBound.x);
        newPos.y = Mathf.Clamp(newPos.y, minDragBound.y, maxDragBound.y);

        rectTransform.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // ===== Клик по письму =====
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || isAnimating) return;
        StartCoroutine(FlipAnimation());
    }

    /// <summary>
    /// Мгновенный переворот письма без анимации
    /// </summary>
    public void InstaFlip()
    {
        isFlipped = !isFlipped;

        if (backSide != null)
            backSide.SetActive(isFlipped);

        GetComponent<Image>().sprite = isFlipped ? backImage : baseImage;

        var mail = MailManager.Instance.GetMailById(id);
        if (mail != null)
        {
            if (receiverText != null)
                receiverText.text = AdressConverter.Convert(mail.reciever);
            if (addressText != null)
                addressText.text = AdressConverter.Convert(mail.adress);
        }
    }

    /// <summary>
    /// Анимация переворота письма с использованием DOTween
    /// </summary>
    public IEnumerator FlipAnimation()
    {
        if (isAnimating) yield break;

        isAnimating = true;
        Vector3 originalScale = transform.localScale;

        Sequence seq = DOTween.Sequence();

        // 1. Сжатие по X + поворот + лёгкий пульс по Y
        seq.Append(transform.DOScaleX(0f, flipDuration * 0.5f).SetEase(Ease.InQuad));
        seq.Join(transform.DORotate(new Vector3(0, 90, 0), flipDuration * 0.5f).SetEase(Ease.InQuad));
        seq.Join(transform.DOScaleY(originalScale.y * 1.05f, flipDuration * 0.25f)
            .SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));

        // 2. Смена стороны письма
        seq.AppendCallback(() =>
        {
            isFlipped = !isFlipped;

            if (backSide != null)
                backSide.SetActive(isFlipped);

            if (isFlipped)
            {
                var mail = MailManager.Instance.GetMailById(id);
                if (mail != null)
                {
                    if (receiverText != null)
                        receiverText.text = AdressConverter.Convert(mail.reciever);
                    if (addressText != null)
                        addressText.text = AdressConverter.Convert(mail.adress);
                }

                if (actionButton != null)
                    actionButton.gameObject.SetActive(false);
            }

            GetComponent<Image>().sprite = isFlipped ? backImage : baseImage;
        });

        // 3. Раскрытие письма + возврат поворота + лёгкий "отскок"
        seq.Append(transform.DOScaleX(originalScale.x, flipDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Join(transform.DORotate(Vector3.zero, flipDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Join(transform.DOScaleY(originalScale.y * 1.05f, flipDuration * 0.25f)
            .SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));

        // 4. Завершение анимации
        seq.OnComplete(() =>
        {
            transform.localScale = originalScale;
            transform.rotation = Quaternion.identity;
            isAnimating = false;
        });

        yield return null;
    }

    /// <summary>
    /// Нажатие кнопки действия письма — добавление письма в инвентарь игрока
    /// </summary>
    private void OnActionButtonClick()
    {
        if (PlayerMailInventory.Instance != null)
        {
            Task newTask = new Task(recipient, address, id, isStory);
            PlayerMailInventory.Instance.AddMailToInventory(newTask);
            Destroy(gameObject);
        }
    }

    // ===== Hover эффекты =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging || isAnimating) return;

        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * hoverScale, hoverDuration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging || isAnimating) return;

        rectTransform.DOKill();
        rectTransform.DOScale(originalScale, hoverDuration).SetEase(Ease.OutBack);
    }
}