using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class DeskLetterUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 minDragBound;
    private Vector2 maxDragBound;

    public string recipient;
    public string address;
    public string id;
    public bool isStory;

    [Header("Flip Animation")]
    public float flipDuration = 0.2f;

    [Header("Back Side")]
    public GameObject backSide;
    public TMP_Text receiverText;
    public TMP_Text addressText;

    [Header("Button Reference")]
    public Button actionButton;

    [Header("Hover Effect")]
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;

    private bool isFlipped = false;
    private bool isDragging = false;
    private bool isAnimating = false;

    private Vector3 originalScale; // сохраняем исходный масштаб

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (actionButton == null)
            actionButton = GetComponentInChildren<Button>();

        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonClick);
        }

        if (backSide != null)
            backSide.SetActive(false);

        CalculateDragBounds();
        Invoke(nameof(DelayedSetScale), Time.deltaTime);
    }
    void DelayedSetScale()
    {
        originalScale = transform.localScale; // сохраняем исходный масштаб UI
        if(Random.value > .6f)
        {
            InstaFlip();
        }
    }

    void CalculateDragBounds()
    {
        RectTransform parentRect = transform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 parentSize = parentRect.rect.size;
        Vector2 size = rectTransform.rect.size;

        minDragBound = new Vector2(-parentSize.x / 2 + size.x / 2, -parentSize.y / 2 + size.y / 2);
        maxDragBound = new Vector2(parentSize.x / 2 - size.x / 2, parentSize.y / 2 - size.y / 2);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        Vector2 delta = eventData.delta / canvas.scaleFactor;
        Vector2 newPos = rectTransform.anchoredPosition + delta;

        newPos.x = Mathf.Clamp(newPos.x, minDragBound.x, maxDragBound.x);
        newPos.y = Mathf.Clamp(newPos.y, minDragBound.y, maxDragBound.y);

        rectTransform.anchoredPosition = newPos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        transform.SetAsLastSibling();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Вернуть масштаб к исходному при начале drag
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale, hoverDuration);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging || isAnimating) return;
        StartCoroutine(FlipAnimation());
    }
    public void InstaFlip()
    {
        isFlipped = !isFlipped;
        if (backSide != null)
            backSide.SetActive(isFlipped);
        var mail = MailManager.Instance.GetMailById(id);
        if (receiverText != null)
            receiverText.text = LocalizationManager.Instance.Get(mail.reciever);
        if (addressText != null)
            addressText.text = LocalizationManager.Instance.Get(mail.adress);
    }
    private IEnumerator FlipAnimation()
    {
        isAnimating = true;

        float t = 0f;
        Vector3 scale = transform.localScale;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(scale.x, 0f, t / flipDuration);
            transform.localScale = new Vector3(scaleX, scale.y, scale.z);
            yield return null;
        }
        transform.localScale = new Vector3(0f, scale.y, scale.z);

        isFlipped = !isFlipped;
        if (backSide != null)
            backSide.SetActive(isFlipped);

        if (isFlipped)
        {
            var mail = MailManager.Instance.GetMailById(id);
            if (mail != null)
            {
                if (receiverText != null)
                    receiverText.text = LocalizationManager.Instance.Get(mail.reciever);
                if (addressText != null)
                    addressText.text = LocalizationManager.Instance.Get(mail.adress);
            }

            if (actionButton != null)
                actionButton.gameObject.SetActive(false);
        }

        t = 0f;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(0f, scale.x, t / flipDuration);
            transform.localScale = new Vector3(scaleX, scale.y, scale.z);
            yield return null;
        }
        transform.localScale = scale;

        isAnimating = false;
    }

    private void OnActionButtonClick()
    {
        if (PlayerMailInventory.Instance != null)
        {
            Task newTask = new Task(recipient, address, id);
            PlayerMailInventory.Instance.AddMailToInventory(newTask);
            Destroy(gameObject);
        }
    }

    // ---------------- Hover с DOTween ----------------
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