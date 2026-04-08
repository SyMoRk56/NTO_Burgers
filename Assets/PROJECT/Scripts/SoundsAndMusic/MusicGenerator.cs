using System.Collections.Generic;
using UnityEngine;

public class MusicGenerator : MonoBehaviour
{
    [Header("Инструменты")]
    public List<InstrumentSample> instruments = new List<InstrumentSample>();

    [Header("Пэды (мягкий синт, амбиент)")]
    public List<InstrumentSample> padInstruments = new List<InstrumentSample>();   // ★ ДОБАВЛЕНО

    [Header("Настройки пэдов")]
    public float padVolume = 0.6f;       // громкость пэдов
    public float padLength = 4f;         // длительность ноты пэда в секундах
    public bool generatePads = true;     // включить / выключить генерацию пэдов

    [Header("Настройки мелодии")]
    public MelodyPattern melodyPattern = MelodyPattern.OneNote; // паттерн мелодии
    public float bpm = 120f;            // темп
    public int bars = 4;                // количество тактов
    public float restChance = 0.15f;    // шанс паузы вместо ноты

    [Header("Настройки тона")]
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f); // диапазон случайного изменения питча

    [Header("Настройки гаммы")]
    public ScaleType scaleType = ScaleType.Major;  // тип гаммы
    public int rootNote = 60;                       // базовая нота (MIDI)

    [Header("Аккордовая последовательность")]
    public ChordProgression progression;

    private MelodyPattern[] barPatterns; // паттерны для каждого такта

    private void Start()
    {
        GenerateEverything(); // генерируем сразу всю музыку
    }

    // Генерирует аккордовую прогрессию, паттерны и пэды
    public void GenerateEverything()
    {
        GenerateDynamicProgression(); // динамическая прогрессия аккордов
        GenerateBarPatterns();        // паттерны мелодии по тактам
        var padsd = GeneratePads();   // генерация пэдов
    }

    // Генерация аккордовой прогрессии по шкале
    public void GenerateDynamicProgression()
    {
        int[] majorDegrees = { 0, 1, 2, 3, 4, 5 };
        int[] minorDegrees = { 0, 2, 3, 4, 5 };

        List<int> pool = scaleType == ScaleType.Major
            ? new List<int>(majorDegrees)
            : new List<int>(minorDegrees);

        progression.degrees = new List<int>();
        progression.degrees.Add(0); // первый аккорд всегда тоника

        for (int i = 1; i < bars; i++)
        {
            int previous = progression.degrees[i - 1];
            int next = GetMusicalNextDegree(previous, pool); // выбираем следующий аккорд
            progression.degrees.Add(next);
        }
    }

    // Выбирает следующий аккорд на основе текущего
    private int GetMusicalNextDegree(int prev, List<int> pool)
    {
        Dictionary<int, int[]> transitions = new Dictionary<int, int[]>()
        {
            { 0, new[]{ 3, 4, 5 } },
            { 1, new[]{ 4, 5, 0 } },
            { 2, new[]{ 5, 0 } },
            { 3, new[]{ 4, 5, 0 } },
            { 4, new[]{ 5, 0 } },
            { 5, new[]{ 3, 0 } }
        };

        if (transitions.ContainsKey(prev))
        {
            var arr = transitions[prev];
            return arr[Random.Range(0, arr.Length)];
        }

        return pool[Random.Range(0, pool.Count)];
    }

    // Генерация паттернов мелодии для каждого такта
    public void GenerateBarPatterns()
    {
        barPatterns = new MelodyPattern[bars];

        for (int i = 0; i < bars; i++)
        {
            barPatterns[i] = MelodyPattern.OneNote; // по умолчанию одна нота

            if (Random.value < 0.2f)
                barPatterns[i] = MelodyPattern.TwoNotes; // иногда две ноты
        }
    }

    // Генерация мелодии в виде списка нот
    public List<Note> GenerateMelody()
    {
        float secondsPerBeat = 60f / bpm;
        float beat = 0f;

        int[] scale = MusicScales.GetScale(scaleType);
        List<Note> melody = new List<Note>();

        for (int bar = 0; bar < bars; bar++)
        {
            int chordDegree = progression.degrees[bar];
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

        // Преобразуем биты в секунды
        for (int i = 0; i < melody.Count; i++)
        {
            var n = melody[i];
            n.startTime *= secondsPerBeat;
            melody[i] = n;
        }

        return melody;
    }

    // Генерация пэдов (длинные мягкие ноты)
    public List<PadNote> GeneratePads()
    {
        List<PadNote> pads = new List<PadNote>();

        if (!generatePads || padInstruments.Count == 0)
            return pads;

        float secondsPerBeat = 60f / bpm;
        int[] scale = MusicScales.GetScale(scaleType);

        for (int bar = 0; bar < bars; bar++)
        {
            int degree = progression.degrees[bar];
            int midi = rootNote + scale[degree] - 7; // транспонируем для мягкого звучания

            float pitch = Mathf.Pow(2f, (midi - 60) / 12f);

            var inst = padInstruments[Random.Range(0, padInstruments.Count)];

            pads.Add(new PadNote(
                inst.sample,
                pitch,
                bar * secondsPerBeat, // старт ноты
                padLength              // длина ноты
            ));
        }

        return pads;
    }

    // Добавление нот по паттерну
    private void AddPatternNotes(List<Note> melody, float beatStart, int chordRoot, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float time = beatStart + i * (1f / count);

            if (Random.value < restChance)
            {
                melody.Add(new Note(null, 1f, time, true)); // пауза
                continue;
            }

            AddMusicalNote(melody, time, chordRoot);
        }
    }

    // Добавление триады аккорда
    private void AddTriad(List<Note> melody, float beatStart, int chordRoot, int[] scale)
    {
        int[] triadSteps = { 0, 2, 4 };

        for (int i = 0; i < triadSteps.Length; i++)
        {
            float time = beatStart + i * 0.3333f;

            if (Random.value < restChance)
            {
                melody.Add(new Note(null, 1f, time, true)); // пауза
                continue;
            }

            int midi = chordRoot + scale[triadSteps[i] % scale.Length];
            AddMusicalNote(melody, time, midi);
        }
    }

    // Добавление отдельной музыкальной ноты
    private void AddMusicalNote(List<Note> melody, float startBeat, int midiNote)
    {
        var instrument = instruments[Random.Range(0, instruments.Count)];

        float pitch = Mathf.Pow(2f, (midiNote - 60) / 12f);
        pitch *= Random.Range(pitchRange.x, pitchRange.y);

        // Избегаем одинаковых последовательных нот
        if (melody.Count > 0)
        {
            Note last = melody[melody.Count - 1];

            if (!last.isRest && Mathf.Abs(last.pitch - pitch) < 0.0001f)
            {
                int direction = Random.value < 0.5f ? -1 : 1;
                midiNote += direction;

                pitch = Mathf.Pow(2f, (midiNote - 60) / 12f);
                pitch *= Random.Range(pitchRange.x, pitchRange.y);
            }
        }

        melody.Add(new Note(instrument.sample, pitch, startBeat));
    }
}


// Структура для пэдов
[System.Serializable]
public struct PadNote
{
    public AudioClip clip;
    public float pitch;
    public float startTime;
    public float length;

    public PadNote(AudioClip clip, float pitch, float startTime, float length)
    {
        this.clip = clip;
        this.pitch = pitch;
        this.startTime = startTime;
        this.length = length;
    }
}


// Примеры классов для барабанов и баса
[CreateAssetMenu(menuName = "MusicGen/Drum Sample")]
public class DrumSample : ScriptableObject
{
    public string drumName;     // kick, snare, hat
    public AudioClip clip;
}

public enum RhythmPatternType
{
    FourOnFloor,      // бочка на каждую четверть
    Backbeat,         // классическая бочка/снейр
    BasicRock,        // бочка + снейр + хэты
    House,            // бочка на каждый удар, хэты восьмыми
    RandomGroove      // хаотичный но ритмичный
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
    RootOnEachBeat,    // корень на каждый удар
    RootAndFifth,      // корень + квинта
    WalkingBass,       // "ходящий" бас
    ArpTriad,          // арпеджио трезвучия
    RandomGroove       // случайный ритм внутри аккорда
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