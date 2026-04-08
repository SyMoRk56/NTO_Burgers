using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MusicMixer : MonoBehaviour
{
    [Header("Генераторы")]
    public MusicGenerator melodyGenerator;   // генератор мелодии и пэдов
    public BassGenerator bassGenerator;      // генератор баса
    public RhythmGenerator rhythmGenerator;  // генератор ритма/барабанов
    public ChordGenerator chordGenerator;    // генератор аккордов

    [Header("Громкость")]
    [Range(0f, 1f)] public float melodyVolume = 1f;
    [Range(0f, 1f)] public float bassVolume = 1f;
    [Range(0f, 1f)] public float chordsVolume = 1f;
    [Range(0f, 1f)] public float drumsVolume = 1f;
    public AudioMixerGroup group; // микшерная группа для всех аудиоисточников

    private List<AudioSource> sources = new List<AudioSource>(); // активные источники звука
    public bool playonstart = false;

    private void Start()
    {
        if (playonstart) Play(); // если включено, стартуем автоматически
    }

    // Останавливает всю музыку
    public void Stop()
    {
        print("STOP");
        StopAllCoroutines(); // останавливаем все корутины
        StopAll();           // удаляем все источники
    }

    // Основной метод запуска музыки
    public void Play()
    {
        StopAll(); // защита от наложения старых источников

        // --- Генерация пэдов ---
        var pads = melodyGenerator.GeneratePads();
        StartCoroutine(PlayPads(pads));

        // --- Генерация основной музыки ---
        var melody = melodyGenerator.GenerateMelody();
        var bass = bassGenerator.GenerateBass();
        var drums = rhythmGenerator.GenerateRhythm();
        var chords = chordGenerator.GenerateChords();

        // --- Проигрывание каждого слоя ---
        StartCoroutine(PlayMelody(melody));
        StartCoroutine(PlayBass(bass));
        StartCoroutine(PlayChords(chords));
        StartCoroutine(PlayDrums(drums));
    }

    // Корутина для проигрывания пэдов
    private IEnumerator PlayPads(List<PadNote> pads)
    {
        foreach (var p in pads)
        {
            yield return new WaitForSeconds(p.startTime);

            GameObject go = new GameObject("PadNote");
            AudioSource src = go.AddComponent<AudioSource>();

            src.playOnAwake = false;
            src.volume = melodyGenerator.padVolume;
            src.pitch = p.pitch;
            src.outputAudioMixerGroup = group;

            sources.Add(src);

            if (p.clip != null)
            {
                src.clip = p.clip;
                src.loop = false;
                src.Play();
                Destroy(go, p.length + 0.5f); // удаляем объект после завершения ноты
            }
        }
    }

    // Удаление всех активных аудиоисточников
    public void StopAll()
    {
        foreach (var s in sources)
        {
            if (s) Destroy(s.gameObject);
        }
        sources.Clear();
    }

    // Корутина для проигрывания мелодии
    private IEnumerator PlayMelody(List<Note> notes)
    {
        float startTime = Time.time;

        foreach (var n in notes)
        {
            float targetTime = startTime + n.startTime;

            while (Time.time < targetTime)
                yield return null;

            GameObject go = new GameObject("MelodyNote");
            go.transform.parent = transform;
            AudioSource src = go.AddComponent<AudioSource>();

            src.outputAudioMixerGroup = group;
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

    // Корутина для проигрывания баса
    private IEnumerator PlayBass(List<BassNote> notes)
    {
        yield break; // временно отключено

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

    // Корутина для проигрывания аккордов
    private IEnumerator PlayChords(List<ChordNote> notes)
    {
        yield break; // временно отключено

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

    // Корутина для проигрывания ударных
    private IEnumerator PlayDrums(List<DrumHit> hits)
    {
        yield break; // временно отключено

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