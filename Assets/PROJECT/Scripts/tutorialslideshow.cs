using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система 1 — Слайдшоу при старте НОВОГО слота сохранения.
/// 
/// Настройка в сцене:
/// 1. Canvas → Panel (весь экран) → добавь этот компонент.
/// 2. Добавь CanvasGroup на панель.
/// 3. Назначь в инспекторе: slideImage, nextButton, skipButton.
/// 4. Заполни массив slides[] спрайтами в нужном порядке.
/// 5. SaveGameManager вызовет ShowSlideshow() при создании нового слота.
/// </summary>
public class TutorialSlideshowUI : MonoBehaviour
{
    public static TutorialSlideshowUI Instance { get; private set; }

    [Header("UI")]
    public Image slideImage;
    public TextMeshProUGUI slideCaption;   // Опционально
    public Button nextButton;
    public Button skipButton;
    public TextMeshProUGUI pageCounter;    // "1 / 5"
    public CanvasGroup canvasGroup;

    [Header("Слайды")]
    public Sprite[] slides;
    public string[] captions;              // Опционально

    [Header("Анимация")]
    [Range(0.1f, 1f)]
    public float fadeDuration = 0.35f;

    private int currentIndex = 0;
    private bool isShowing = false;

    // ── Singleton ──────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        gameObject.SetActive(false);
    }

    void Start()
    {
        nextButton?.onClick.AddListener(OnNextClicked);
        skipButton?.onClick.AddListener(OnSkipClicked);
    }

    // ── Запуск ────────────────────────────────────────────

    /// <summary>Вызывается SaveGameManager при создании нового слота.</summary>
    public void ShowSlideshow()
    {
        if (isShowing) return;

        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[Slideshow] Массив slides пуст! Заполни слайды в инспекторе.");
            FinishSlideshow();
            return;
        }

        isShowing = true;
        currentIndex = 0;

        // Блокируем управление на время слайдшоу
        BlockPlayer(true);

        gameObject.SetActive(true);

        if (canvasGroup != null) canvasGroup.alpha = 0f;

        StartCoroutine(FadeIn(() => ShowSlide(0)));
    }

    // ── Навигация по слайдам ───────────────────────────────

    void ShowSlide(int index)
    {
        currentIndex = index;
        slideImage.sprite = slides[index];

        // Подпись
        if (slideCaption != null)
        {
            bool has = captions != null && index < captions.Length
                       && !string.IsNullOrEmpty(captions[index]);
            slideCaption.gameObject.SetActive(has);
            if (has) slideCaption.text = captions[index];
        }

        // Счётчик страниц
        if (pageCounter != null)
            pageCounter.text = $"{index + 1} / {slides.Length}";

        // На последнем слайде кнопка "Далее" → "Играть"
        if (nextButton != null)
        {
            var lbl = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null)
                lbl.text = (index == slides.Length - 1) ? "Играть" : "Далее →";
        }
    }

    void OnNextClicked()
    {
        if (currentIndex < slides.Length - 1)
            StartCoroutine(CrossfadeTo(currentIndex + 1));
        else
            StartCoroutine(FadeOut(FinishSlideshow));
    }

    void OnSkipClicked()
    {
        StartCoroutine(FadeOut(FinishSlideshow));
    }

    void FinishSlideshow()
    {
        isShowing = false;
        gameObject.SetActive(false);

        BlockPlayer(false);

        // Уведомляем TutorialManager
        TutorialManager.Instance?.OnSlideshowFinished();

        Debug.Log("[Slideshow] Завершено");
    }

    // ── Блокировка игрока ──────────────────────────────────

    void BlockPlayer(bool block)
    {
        var player = GameManager.Instance?.GetPlayer();
        if (player == null) return;
        var pm = player.GetComponent<PlayerManager>();
        if (pm == null) return;
        pm.ShowCursor(block);
        pm.CanMove = !block;
    }

    // ── Анимации ──────────────────────────────────────────

    IEnumerator CrossfadeTo(int nextIndex)
    {
        yield return FadeImageAlpha(0f, fadeDuration * 0.5f);
        ShowSlide(nextIndex);
        yield return FadeImageAlpha(1f, fadeDuration * 0.5f);
    }

    IEnumerator FadeIn(System.Action onDone = null)
    {
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        onDone?.Invoke();
    }

    IEnumerator FadeOut(System.Action onDone = null)
    {
        if (canvasGroup != null)
        {
            float t = fadeDuration;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        onDone?.Invoke();
    }

    IEnumerator FadeImageAlpha(float target, float duration)
    {
        if (slideImage == null) yield break;
        Color c = slideImage.color;
        float start = c.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(start, target, t / duration);
            slideImage.color = c;
            yield return null;
        }
        c.a = target;
        slideImage.color = c;
    }
}