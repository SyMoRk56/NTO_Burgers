using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

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
    public AudioMixerGroup group;

    private List<AudioSource> sources = new List<AudioSource>();
    public bool playonstart = false;
    private void Start()
    {
        if(playonstart) Play();
    }
    public void Stop()
    {
        print("STOP");
        StopAllCoroutines();
        StopAll();
    }
    public void Play()
    {
        StopAll(); // защитное
        var pads = melodyGenerator.GeneratePads(); // ★ добавить
        StartCoroutine(PlayPads(pads));            // ★ добавить

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
    private IEnumerator PlayPads(List<PadNote> pads)
    {
        foreach (var p in pads)
        {
            yield return new WaitForSeconds(p.startTime);

            GameObject go = new GameObject("PadNote");
            AudioSource src = go.AddComponent<AudioSource>();
            //AudioReverbFilter f = go.AddComponent<AudioReverbFilter>();
            //f.reverbPreset = AudioReverbPreset.StoneCorridor;
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
                Destroy(go, p.length + 0.5f); // мягкое удаление
            }
        }
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
            go.transform.parent = transform;
            AudioSource src = go.AddComponent<AudioSource>();
            //AudioReverbFilter f = go.AddComponent<AudioReverbFilter>();
            //f.reverbPreset = AudioReverbPreset.StoneCorridor;
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


    private IEnumerator PlayBass(List<BassNote> notes)
    {
        yield break;
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
        yield break;
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
        yield break;
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
