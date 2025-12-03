using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Default Music")]
    public AudioClip defaultClip;
    public MusicMixer defaultMixer;

    private AudioSource audioSource;

    // текущие активные
    private AudioClip currentClip;
    private MusicMixer currentMixer;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = .1f;

        if (defaultMixer != null) PlayMusic(defaultMixer);
        else if (defaultClip != null) PlayMusic(defaultClip);
    }

    // -----------------------------
    //      Публичные методы
    // -----------------------------

    public void PlayMusic(AudioClip clip)
    {
        if (clip == currentClip)
            return;

        SwitchInstant(clip, null);
    }

    public void PlayMusic(MusicMixer mixer)
    {
        if (mixer == currentMixer)
            return;

        SwitchInstant(null, mixer);
    }

    public void PlayDefault()
    {
        if (defaultMixer != null) PlayMusic(defaultMixer);
        else if (defaultClip != null) PlayMusic(defaultClip);
    }

    // -----------------------------
    //      Переключение без fade
    // -----------------------------
    private void SwitchInstant(AudioClip newClip, MusicMixer newMixer)
    {
        // стоп старого миксера
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
    }
}
