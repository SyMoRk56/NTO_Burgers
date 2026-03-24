using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialSlideshowUI : MonoBehaviour
{
    public static TutorialSlideshowUI Instance { get; private set; }

    [Header("UI")]
    public RawImage videoDisplay;
    public Button skipButton;
    public CanvasGroup canvasGroup;

    [Header("Видео")]
    [Tooltip("Название файла в папке Resources без расширения. Например: TutorialVideo")]
    public string videoResourcePath = "TutorialVideo";

    [Header("Анимация")]
    [Range(0.1f, 1f)]
    public float fadeDuration = 0.35f;

    private VideoPlayer videoPlayer;
    private AudioSource videoAudio;
    private RenderTexture renderTexture;
    private bool isShowing = false;
    private bool initialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Video] Awake — Instance создан");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitVideoPlayer();
        gameObject.SetActive(false);
    }

    void InitVideoPlayer()
    {
        print("TUTORIAL INIT");
        if (initialized) return;

        videoAudio = gameObject.AddComponent<AudioSource>();
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudio);
        videoPlayer.loopPointReached += OnVideoFinished;

        initialized = true;
        Debug.Log("[Video] VideoPlayer инициализирован");
    }

    void Start()
    {
        skipButton?.onClick.AddListener(OnSkipClicked);
    }

    void Update()
    {
        if (!isShowing) return;
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ── Запуск ────────────────────────────────────────────

    public void ShowSlideshow()
    {
        Debug.Log($"[Video] ShowSlideshow. isShowing={isShowing}, initialized={initialized}");
        if (isShowing) return;

        // Убеждаемся что VideoPlayer создан
        InitVideoPlayer();

        // Загружаем видео из Resources
        VideoClip clip = Resources.Load<VideoClip>(videoResourcePath);
        if (clip == null)
        {
            Debug.LogWarning($"[Video] Видео не найдено: Resources/{videoResourcePath}\n" +
                             $"Убедись что файл лежит в Assets/Resources/ и имя указано без расширения.");
            // Пропускаем видео — туториал продолжается без него
            TutorialManager.Instance?.OnSlideshowFinished();
            return;
        }

        isShowing = true;
        BlockPlayer(true);

        // Создаём RenderTexture
        if (renderTexture != null) renderTexture.Release();
        renderTexture = new RenderTexture((int)clip.width, (int)clip.height, 0);
        videoPlayer.targetTexture = renderTexture;

        if (videoDisplay != null)
            videoDisplay.texture = renderTexture;
        else
            Debug.LogWarning("[Video] videoDisplay (RawImage) не назначен в инспекторе!");

        videoPlayer.clip = clip;

        // Активируем объект ПЕРЕД coroutine
        gameObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        Debug.Log($"[Video] Загружено: {clip.name} ({clip.width}x{clip.height}, {clip.length:F1}s)");
        StartCoroutine(FadeInThenPlay());
    }

    private IEnumerator FadeInThenPlay()
    {
        yield return StartCoroutine(Fade(0f, 1f));
        videoPlayer.Play();
        Debug.Log("[Video] Воспроизведение началось");
    }

    // ── Кнопка пропустить ─────────────────────────────────

    void OnSkipClicked()
    {
        Debug.Log("[Video] Пропустить");
        if (videoPlayer != null) videoPlayer.Stop();
        StartCoroutine(Fade(1f, 0f, FinishVideo));
    }

    // ── Видео закончилось ─────────────────────────────────

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[Video] Видео закончилось");
        StartCoroutine(Fade(1f, 0f, FinishVideo));
    }

    // ── Завершение ────────────────────────────────────────

    void FinishVideo()
    {
        Debug.Log("[Video] FinishVideo");
        isShowing = false;

        if (videoPlayer != null) videoPlayer.Stop();

        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }

        if (videoDisplay != null) videoDisplay.texture = null;

        gameObject.SetActive(false);
        BlockPlayer(false);

        TutorialManager.Instance?.OnSlideshowFinished();
    }

    // ── Блокировка игрока ─────────────────────────────────

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

    // ── Fade ──────────────────────────────────────────────

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