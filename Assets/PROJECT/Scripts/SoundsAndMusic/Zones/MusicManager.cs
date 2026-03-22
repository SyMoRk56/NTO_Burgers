using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Default Music")]
    public AudioClip defaultClip;
    public MusicMixer defaultMixer;

    [Header("Fade Settings")]
    public float fadeDuration = 3f;

    [Header("Audio")]
    public AudioMixerGroup group;

    private AudioSource audioSource;

    // текущие активные
    private AudioClip currentClip;
    private MusicMixer currentMixer;

    // цель перехода (ВАЖНО для защиты от спама)
    private AudioClip targetClip;
    private MusicMixer targetMixer;

    private Coroutine transitionCoroutine;

    private void Awake()
    {
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0.1f;
        audioSource.outputAudioMixerGroup = group;

        if (defaultMixer != null) PlayMusic(defaultMixer);
        else if (defaultClip != null) PlayMusic(defaultClip);
    }

    // -----------------------------
    //      Публичные методы
    // -----------------------------

    public void PlayMusic(AudioClip clip)
    {
        // защита от постоянных вызовов
        if (clip == currentClip || clip == targetClip)
            return;

        SwitchSmooth(clip, null);
    }

    public void PlayMusic(MusicMixer mixer)
    {
        if (mixer == currentMixer || mixer == targetMixer)
            return;

        SwitchSmooth(null, mixer);
    }

    public void PlayDefault()
    {
        if (defaultMixer != null) PlayMusic(defaultMixer);
        else if (defaultClip != null) PlayMusic(defaultClip);
    }

    // -----------------------------
    //      Плавное переключение
    // -----------------------------

    private void SwitchSmooth(AudioClip newClip, MusicMixer newMixer)
    {
        targetClip = newClip;
        targetMixer = newMixer;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(FadeTransition(newClip, newMixer));
    }

    private IEnumerator FadeTransition(AudioClip newClip, MusicMixer newMixer)
    {
        float time = 0f;
        float startVolume = audioSource.volume;

        // FADE OUT
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;

        // остановка старого миксера
        if (currentMixer != null)
            currentMixer.Stop();

        // переключение
        if (newClip != null)
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }
        else if (newMixer != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            newMixer.Play();
        }

        currentClip = newClip;
        currentMixer = newMixer;

        // FADE IN
        time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;

        // переход завершён
        targetClip = null;
        targetMixer = null;
    }
}