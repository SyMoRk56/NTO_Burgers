using System.Collections.Generic;
using UnityEngine;

public class MusicGenerator : MonoBehaviour
{
    [Header("Instruments")]
    public List<InstrumentSample> instruments = new List<InstrumentSample>();

    [Header("Melody Settings")]
    public MelodyPattern melodyPattern = MelodyPattern.OneNote;
    public float bpm = 120f;
    public int bars = 4;
    public float restChance = 0.15f; // 15% шанс паузы

    [Header("Pitch Settings")]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    [Header("Scale Settings")]
    public ScaleType scaleType = ScaleType.Major;
    public int rootNote = 60; // 60 = C4

    [Header("Chord Progression")]
    public ChordProgression progression;

    // Паттерн мелодии для каждого бара
    private MelodyPattern[] barPatterns;

    private void Start()
    {
        GenerateEverything();
    }

    // -------------------------------------------------------------------------
    // GENERATION ENTRY
    // -------------------------------------------------------------------------

    public void GenerateEverything()
    {
        GenerateDynamicProgression();
        GenerateBarPatterns();
    }



    // -------------------------------------------------------------------------
    // DYNAMIC PROGRESSION
    // -------------------------------------------------------------------------

    public void GenerateDynamicProgression()
    {
        int[] majorDegrees = { 0, 1, 2, 3, 4, 5 };     // I ii iii IV V vi
        int[] minorDegrees = { 0, 2, 3, 4, 5 };        // i III iv v VI VII

        List<int> pool = scaleType == ScaleType.Major
            ? new List<int>(majorDegrees)
            : new List<int>(minorDegrees);

        progression.degrees = new List<int>();

        // Первый бар — тоника
        progression.degrees.Add(0);

        for (int i = 1; i < bars; i++)
        {
            int previous = progression.degrees[i - 1];
            int next = GetMusicalNextDegree(previous, pool);
            progression.degrees.Add(next);
        }
    }

    private int GetMusicalNextDegree(int prev, List<int> pool)
    {
        Dictionary<int, int[]> transitions = new Dictionary<int, int[]>()
        {
            { 0, new[]{ 3, 4, 5 } },  // I → IV, V, vi
            { 1, new[]{ 4, 5, 0 } },  // ii → V, vi, I
            { 2, new[]{ 5, 0 } },     // iii → vi, I
            { 3, new[]{ 4, 5, 0 } },  // IV → V, vi, I
            { 4, new[]{ 5, 0 } },     // V → vi, I
            { 5, new[]{ 3, 0 } }      // vi → IV, I
        };

        if (transitions.ContainsKey(prev))
        {
            var arr = transitions[prev];
            return arr[Random.Range(0, arr.Length)];
        }

        return pool[Random.Range(0, pool.Count)];
    }



    // -------------------------------------------------------------------------
    // BAR MELODY PATTERNS
    // -------------------------------------------------------------------------

    public void GenerateBarPatterns()
    {
        barPatterns = new MelodyPattern[bars];

        for (int i = 0; i < bars; i++)
        {
            // Паттерны 1–3: TwoNotes, Triad, RandomSequence
            barPatterns[i] = (MelodyPattern)Random.Range(1, 3);
        }
    }



    // -------------------------------------------------------------------------
    // MELODY GENERATION
    // -------------------------------------------------------------------------

    public List<Note> GenerateMelody()
    {
        float secondsPerBeat = 60f / bpm;
        float beat = 0f;

        int[] scale = MusicScales.GetScale(scaleType);
        List<Note> melody = new List<Note>();

        for (int bar = 0; bar < bars; bar++)
        {
            int chordDegree = progression.degrees[bar % progression.degrees.Count];
            int chordRoot = rootNote + scale[chordDegree];

            MelodyPattern pattern = barPatterns[bar];

            switch (pattern)
            {
                case MelodyPattern.OneNote:
                    AddPatternNotes(melody, beat, chordRoot, 1);
                    break;

                case MelodyPattern.TwoNotes:
                    AddPatternNotes(melody, beat, chordRoot, 2);
                    break;

                case MelodyPattern.Triad:
                    AddTriad(melody, beat, chordRoot, scale);
                    break;

                case MelodyPattern.RandomSequence:
                    int count = Random.Range(1, 8);
                    AddPatternNotes(melody, beat, chordRoot, count);
                    break;
            }

            beat += 1f;
        }

        // Переводим биты → секунды
        for (int i = 0; i < melody.Count; i++)
        {
            var n = melody[i];
            n.startTime *= secondsPerBeat;
            melody[i] = n;
        }

        return melody;
    }



    // -------------------------------------------------------------------------
    // PATTERN HELPERS
    // -------------------------------------------------------------------------

    private void AddPatternNotes(List<Note> melody, float beatStart, int chordRoot, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float time = beatStart + i * (1f / count);

            if (Random.value < restChance)
            {
                melody.Add(new Note(null, 1f, time, true));
                continue;
            }

            AddMusicalNote(melody, time, chordRoot);
        }
    }

    private void AddTriad(List<Note> melody, float beatStart, int chordRoot, int[] scale)
    {
        int[] triadSteps = { 0, 2, 4 }; // корень, терция, квинта

        for (int i = 0; i < triadSteps.Length; i++)
        {
            float time = beatStart + i * 0.33f;

            if (Random.value < restChance)
            {
                melody.Add(new Note(null, 1f, time, true));
                continue;
            }

            int pitchMidi = chordRoot + scale[triadSteps[i] % scale.Length];
            AddMusicalNote(melody, time, pitchMidi);
        }
    }

    private void AddMusicalNote(List<Note> melody, float startBeat, int midiNote)
    {
        var instrument = instruments[Random.Range(0, instruments.Count)];

        float pitch = Mathf.Pow(2f, (midiNote - 60) / 12f);
        pitch *= Random.Range(pitchRange.x, pitchRange.y);

        // -----------------------------------------------------
        // ANTI-REPEAT FILTER — чтобы ноты не повторялись
        // -----------------------------------------------------
        if (melody.Count > 0)
        {
            Note last = melody[melody.Count - 1];

            // Проверяем только ноты, не паузы
            if (!last.isRest && Mathf.Abs(last.pitch - pitch) < 0.0001f)
            {
                // Если нота совпала — смещаем на полтона вверх или вниз
                int direction = Random.value < 0.5f ? -1 : 1;
                midiNote += direction;

                // Пересчёт pitch
                pitch = Mathf.Pow(2f, (midiNote - 60) / 12f);
                pitch *= Random.Range(pitchRange.x, pitchRange.y);
            }
        }

        melody.Add(new Note(instrument.sample, pitch, startBeat));
    }

}


[CreateAssetMenu(menuName = "MusicGen/Drum Sample")]
public class DrumSample : ScriptableObject
{
    public string drumName;     // kick, snare, hat...
    public AudioClip clip;
}

public enum RhythmPatternType
{
    FourOnFloor,      // бочка в каждую четверть
    Backbeat,         // классика: бочка 1 и 3, снейр 2 и 4
    BasicRock,        // бочка + снейр + хэты восьмыми
    House,            // бочка каждый удар, хэты восьмыми
    RandomGroove      // хаотично но красиво
}
[System.Serializable]
public struct DrumHit
{
    public AudioClip clip;
    public float startTime;

    public DrumHit(AudioClip clip, float startTime)
    {
        this.clip = clip;
        this.startTime = startTime;
    }
}

[CreateAssetMenu(menuName = "MusicGen/Bass Sample")]
public class BassSample : ScriptableObject
{
    public string bassName;
    public AudioClip sample;
}
public enum BassPatternType
{
    RootOnEachBeat,    // корневая нота на каждый удар
    RootAndFifth,      // корень + квинта
    WalkingBass,       // "ходящий" бас
    ArpTriad,          // арпеджио трезвучия
    RandomGroove       // случайно, но внутри аккорда
}
[System.Serializable]
public struct BassNote
{
    public AudioClip clip;
    public float pitch;
    public float startTime;

    public BassNote(AudioClip clip, float pitch, float startTime)
    {
        this.clip = clip;
        this.pitch = pitch;
        this.startTime = startTime;
    }
}
