using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// Скрипт для показа видео-туториала или слайдшоу.
/// Поддерживает проигрывание видео из папки Resources, кнопку "Пропустить" и плавное появление/исчезновение.
/// </summary>
public class TutorialSlideshowUI : MonoBehaviour
{
    // ===== Singleton =====
    public static TutorialSlideshowUI Instance { get; private set; }

    [Header("UI Elements")]
    public RawImage videoDisplay;      // RawImage для отображения RenderTexture с видео
    public Button skipButton;          // Кнопка пропустить видео
    public CanvasGroup canvasGroup;    // CanvasGroup для анимации fade in/out

    [Header("Видео")]
    [Tooltip("Название файла в папке Resources без расширения. Например: TutorialVideo")]
    public string videoResourcePath = "TutorialVideo";

    [Header("Анимация")]
    [Range(0.1f, 1f)]
    public float fadeDuration = 0.35f; // Длительность плавного появления/исчезновения

    // ===== Внутренние переменные =====
    private VideoPlayer videoPlayer;   // VideoPlayer для воспроизведения видео
    private AudioSource videoAudio;    // AudioSource для аудио видео
    private RenderTexture renderTexture; // RenderTexture для вывода видео на RawImage
    private bool isShowing = false;    // Флаг отображения видео
    private bool initialized = false;  // Флаг инициализации VideoPlayer

    // ===== Инициализация Singleton и VideoPlayer =====
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Video] Awake — Instance создан");
        }
        else
        {
            initialized = false;
            Destroy(gameObject); // Удаляем дубликат
            return;
        }

        InitVideoPlayer();      // Инициализация VideoPlayer и AudioSource
        gameObject.SetActive(false); // Отключаем UI до вызова
    }

    /// <summary>
    /// Настройка VideoPlayer и AudioSource
    /// </summary>
    void InitVideoPlayer()
    {
        if (initialized) return;

        videoAudio = gameObject.AddComponent<AudioSource>();
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture; // Вывод на RenderTexture
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudio);

        // Событие окончания видео
        videoPlayer.loopPointReached += OnVideoFinished;

        initialized = true;
        Debug.Log("[Video] VideoPlayer инициализирован");
    }

    void Start()
    {
        // Можно подключить кнопку пропуска через инспектор
        // skipButton?.onClick.AddListener(OnSkipClicked);
    }

    void Update()
    {
        if (!isShowing) return;

        // Пропустить видео при нажатии любой клавиши
        if (Input.anyKeyDown)
            OnSkipClicked();

        // Разблокировать курсор если он заблокирован
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ===== Показ слайдшоу/видео =====
    public void ShowSlideshow()
    {
        if (isShowing) return;

        InitVideoPlayer(); // На всякий случай инициализируем VideoPlayer

        // Загружаем видео из папки Resources
        VideoClip clip = Resources.Load<VideoClip>(videoResourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"[Video] Видео не найдено: Resources/{videoResourcePath}");
            TutorialManager.Instance?.OnSlideshowFinished(); // Продолжаем туториал без видео
            return;
        }

        isShowing = true;
        BlockPlayer(true); // Блокируем игрока во время видео

        // Создаем RenderTexture для видео
        if (renderTexture != null) renderTexture.Release();
        renderTexture = new RenderTexture((int)clip.width, (int)clip.height, 0);
        videoPlayer.targetTexture = renderTexture;

        if (videoDisplay != null)
            videoDisplay.texture = renderTexture;
        else
            Debug.LogWarning("[Video] videoDisplay (RawImage) не назначен в инспекторе!");

        videoPlayer.clip = clip;

        gameObject.SetActive(true); // Активируем UI перед fade in
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        StartCoroutine(FadeInThenPlay());
    }

    /// <summary>
    /// Плавное появление видео перед стартом воспроизведения
    /// </summary>
    private IEnumerator FadeInThenPlay()
    {
        yield return StartCoroutine(Fade(0f, 1f));
        videoPlayer.Play();
    }

    // ===== Кнопка "Пропустить" =====
    void OnSkipClicked()
    {
        if (videoPlayer != null) videoPlayer.Stop();
        StartCoroutine(Fade(1f, 0f, FinishVideo));
    }

    // ===== Событие окончания видео =====
    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(Fade(1f, 0f, FinishVideo));
    }

    // ===== Завершение видео/слайдшоу =====
    void FinishVideo()
    {
        isShowing = false;

        if (videoPlayer != null) videoPlayer.Stop();

        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }

        if (videoDisplay != null) videoDisplay.texture = null;

        gameObject.SetActive(false);
        BlockPlayer(false); // Разблокируем игрока

        TutorialManager.Instance?.OnSlideshowFinished();
    }

    // ===== Блокировка игрока =====
    void BlockPlayer(bool block)
    {
        var player = GameManager.Instance?.GetPlayer();
        if (player == null) return;
        var pm = player.GetComponent<PlayerManager>();
        if (pm == null) return;

        pm.CanMove = !block;
        Cursor.lockState = block ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = block;
    }

    // ===== Плавный fade in/out =====
    IEnumerator Fade(float from, float to, System.Action onDone = null)
    {
        if (canvasGroup == null) { onDone?.Invoke(); yield break; }

        canvasGroup.alpha = from;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
        onDone?.Invoke();
    }
}