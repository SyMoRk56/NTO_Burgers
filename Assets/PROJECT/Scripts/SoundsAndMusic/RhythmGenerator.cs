using System.Collections.Generic;
using UnityEngine;

public class RhythmGenerator : MonoBehaviour
{
    [Header("Drum Samples")]
    public DrumSample kick;
    public DrumSample snare;
    public DrumSample hat;
    public List<DrumSample> extraDrums;

    [Header("Rhythm Settings")]
    public RhythmPatternType pattern = RhythmPatternType.BasicRock;
    public int bars = 4;
    public float bpm = 120f;

    public List<DrumHit> GenerateRhythm()
    {
        List<DrumHit> hits = new List<DrumHit>();
        float beatDuration = 60f / bpm;

        for (int bar = 0; bar < bars; bar++)
        {
            switch (pattern)
            {
                case RhythmPatternType.FourOnFloor:
                    AddKick(hits, bar, 0);
                    AddKick(hits, bar, 1);
                    AddKick(hits, bar, 2);
                    AddKick(hits, bar, 3);
                    break;

                case RhythmPatternType.Backbeat:
                    AddKick(hits, bar, 0);
                    AddSnare(hits, bar, 1);
                    AddKick(hits, bar, 2);
                    AddSnare(hits, bar, 3);
                    break;

                case RhythmPatternType.BasicRock:
                    AddKick(hits, bar, 0);
                    AddSnare(hits, bar, 1);
                    AddKick(hits, bar, 2);
                    AddSnare(hits, bar, 3);

                    AddHatsEight(hits, bar);
                    break;

                case RhythmPatternType.House:
                    AddKick(hits, bar, 0);
                    AddKick(hits, bar, 1);
                    AddKick(hits, bar, 2);
                    AddKick(hits, bar, 3);

                    AddHatsEight(hits, bar);
                    break;

                case RhythmPatternType.RandomGroove:
                    GenerateRandomGroove(hits, bar);
                    break;
            }
        }

        // Переводим биты → секунды
        for (int i = 0; i < hits.Count; i++)
        {
            var h = hits[i];
            h.startTime *= beatDuration;
            hits[i] = h;
        }

        return hits;
    }

    private void AddKick(List<DrumHit> list, int bar, float beat)
    {
        if (kick != null)
            list.Add(new DrumHit(kick.clip, bar * 4 + beat));
    }

    private void AddSnare(List<DrumHit> list, int bar, float beat)
    {
        if (snare != null)
            list.Add(new DrumHit(snare.clip, bar * 4 + beat));
    }

    private void AddHat(List<DrumHit> list, int bar, float beat)
    {
        if (hat != null)
            list.Add(new DrumHit(hat.clip, bar * 4 + beat));
    }

    private void AddHatsEight(List<DrumHit> list, int bar)
    {
        for (int i = 0; i < 8; i++)
            AddHat(list, bar, i * 0.5f);
    }

    private void GenerateRandomGroove(List<DrumHit> list, int bar)
    {
        for (int i = 0; i < 8; i++)
        {
            float beat = i * 0.5f;

            if (Random.value < 0.4f) AddKick(list, bar, beat);
            if (Random.value < 0.4f) AddSnare(list, bar, beat);
            if (Random.value < 0.7f) AddHat(list, bar, beat);

            // случайная перкуссия
            if (extraDrums != null && extraDrums.Count > 0)
            {
                if (Random.value < 0.1f)
                {
                    var drum = extraDrums[Random.Range(0, extraDrums.Count)];
                    list.Add(new DrumHit(drum.clip, bar * 4 + beat));
                }
            }
        }
    }
}
