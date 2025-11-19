using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip defaultMusic;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    public float fadeTime = 1.5f;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        // Запускаем дефолтную музыку при старте
        if (defaultMusic != null)
        {
            audioSource.clip = defaultMusic;
            audioSource.Play();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource.clip == clip) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeMusic(clip));
    }

    public void PlayDefault()
    {
        if (defaultMusic == null) return;
        PlayMusic(defaultMusic);
    }

    private System.Collections.IEnumerator FadeMusic(AudioClip newClip)
    {
        float startVolume = audioSource.volume;

        // fade out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        audioSource.clip = newClip;
        audioSource.Play();

        // fade in
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }
    }
}
