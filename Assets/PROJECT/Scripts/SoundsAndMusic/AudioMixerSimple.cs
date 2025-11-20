using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicMixer : MonoBehaviour
{
    [Header("Generators")]
    public MusicGenerator melodyGenerator;
    public BassGenerator bassGenerator;
    public RhythmGenerator rhythmGenerator;
    public ChordGenerator chordGenerator;

    [Header("Volumes")]
    [Range(0f, 1f)] public float melodyVolume = 1f;
    [Range(0f, 1f)] public float bassVolume = 1f;
    [Range(0f, 1f)] public float chordsVolume = 1f;
    [Range(0f, 1f)] public float drumsVolume = 1f;

    private List<AudioSource> sources = new List<AudioSource>();

    private void Start()
    {
        Play();
    }
    public void Play()
    {
        StopAll(); // защитное

        // --- Генерация ---
        var melody = melodyGenerator.GenerateMelody();
        var bass = bassGenerator.GenerateBass();
        var drums = rhythmGenerator.GenerateRhythm();
        var chords = chordGenerator.GenerateChords();

        // --- Создание аудиоисточников ---
        StartCoroutine(PlayMelody(melody));
        StartCoroutine(PlayBass(bass));
        StartCoroutine(PlayChords(chords));
        StartCoroutine(PlayDrums(drums));
    }

    public void StopAll()
    {
        foreach (var s in sources)
        {
            if (s) Destroy(s.gameObject);
        }
        sources.Clear();
    }

    private IEnumerator PlayMelody(List<Note> notes)
    {
        float startTime = Time.time;

        foreach (var n in notes)
        {
            // Ждём, пока не придёт время этой ноты
            float targetTime = startTime + n.startTime;

            while (Time.time < targetTime)
                yield return null;

            // Создаём источник
            GameObject go = new GameObject("MelodyNote");
            AudioSource src = go.AddComponent<AudioSource>();

            src.playOnAwake = false;
            src.volume = melodyVolume;

            sources.Add(src);

            if (n.clip != null)
            {
                src.clip = n.clip;
                src.pitch = n.pitch;
                src.Play();
            }
        }
    }


    private IEnumerator PlayBass(List<BassNote> notes)
    {
        foreach (var n in notes)
        {
            GameObject go = new GameObject("BassNote");
            AudioSource src = go.AddComponent<AudioSource>();

            src.playOnAwake = false;
            src.volume = bassVolume;

            sources.Add(src);

            yield return new WaitForSeconds(n.startTime);

            if (n.clip != null)
            {
                src.clip = n.clip;
                src.pitch = n.pitch;
                src.Play();
            }
        }
    }

    private IEnumerator PlayChords(List<ChordNote> notes)
    {
        foreach (var n in notes)
        {
            GameObject go = new GameObject("ChordNote");
            AudioSource src = go.AddComponent<AudioSource>();

            src.playOnAwake = false;
            src.volume = chordsVolume;

            sources.Add(src);

            yield return new WaitForSeconds(n.startTime);

            if (n.clip != null)
            {
                src.clip = n.clip;
                src.pitch = n.pitch;
                src.Play();
            }
        }
    }

    private IEnumerator PlayDrums(List<DrumHit> hits)
    {
        foreach (var h in hits)
        {
            GameObject go = new GameObject("DrumHit");
            AudioSource src = go.AddComponent<AudioSource>();

            src.playOnAwake = false;
            src.volume = drumsVolume;

            sources.Add(src);

            yield return new WaitForSeconds(h.startTime);

            if (h.clip != null)
            {
                src.clip = h.clip;
                src.pitch = 1f;
                src.Play();
            }
        }
    }
}
