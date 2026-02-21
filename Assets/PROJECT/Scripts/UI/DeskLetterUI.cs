using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeskLetterUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 minDragBound;
    private Vector2 maxDragBound;

    public string recipient;
    public string address;
    public string id;

    [Header("Flip Animation")]
    public float flipDuration = 0.2f;

    [Header("Back Side")]
    public GameObject backSide;
    public TMP_Text receiverText;
    public TMP_Text addressText;

    [Header("Button Reference")]
    public Button actionButton;

    private bool isFlipped = false;
    private bool isDragging = false;
    private bool isAnimating = false;

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

        // Скрываем обратную сторону по умолчанию
        if (backSide != null)
            backSide.SetActive(false);

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
        if (isDragging || isAnimating) return;
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        isAnimating = true;

        // Сплющиваем до нуля
        float t = 0f;
        Vector3 originalScale = transform.localScale;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(originalScale.x, 0f, t / flipDuration);
            transform.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
            yield return null;
        }
        transform.localScale = new Vector3(0f, originalScale.y, originalScale.z);

        // Меняем содержимое в середине анимации
        isFlipped = !isFlipped;
        if (backSide != null)
            backSide.SetActive(isFlipped);

        if (isFlipped)
        {
            // Заполняем данными
            var mail = MailManager.Instance.GetMailById(id);
            if (mail != null)
            {
                if (receiverText != null)
                    receiverText.text = LocalizationManager.Instance.Get(mail.reciever);
                if (addressText != null)
                    addressText.text = LocalizationManager.Instance.Get(mail.adress);
            }
            // Скрываем кнопку на обратной стороне
            if (actionButton != null)
                actionButton.gameObject.SetActive(false);
        }

        // Растягиваем обратно
        t = 0f;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(0f, originalScale.x, t / flipDuration);
            transform.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
            yield return null;
        }
        transform.localScale = originalScale;

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
}