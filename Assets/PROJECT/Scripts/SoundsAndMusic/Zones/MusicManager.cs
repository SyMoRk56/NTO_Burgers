using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip defaultMusic;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    public float fadeTime = 1.5f;

    private MusicMixer activeMixer;

    private AudioClip currentClip;
    private MusicMixer currentMixer;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        currentClip = defaultMusic;

        if (defaultMusic != null)
        {
            audioSource.clip = defaultMusic;
            audioSource.Play();
        }
    }

    public void PlayMusic(AudioClip clip, MusicMixer mixer)
    {
        // --- NEW --- проверяем, не пытаемся ли мы запустить то же самое
        if (clip == currentClip && mixer == currentMixer)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeSwitch(clip, mixer));

        // Запоминаем активные параметры
        currentClip = clip;
        currentMixer = mixer;
    }

    public void PlayDefault()
    {
        if (defaultMusic == null) return;
        PlayMusic(defaultMusic, null);
    }

    private IEnumerator FadeSwitch(AudioClip newClip, MusicMixer newMixer)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        if (activeMixer != null)
        {
            activeMixer.StopAll();
            activeMixer = null;
        }

        if (newMixer != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            activeMixer = newMixer;
            newMixer.Play();
        }
        else
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }
    }
}
