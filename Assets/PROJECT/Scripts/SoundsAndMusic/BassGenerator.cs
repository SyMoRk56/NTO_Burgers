using System.Collections.Generic;
using UnityEngine;

public class BassGenerator : MonoBehaviour
{
    [Header("Bass Settings")]
    public BassSample bassSample;
    public BassPatternType bassPattern = BassPatternType.RootOnEachBeat;
    public int bars = 4;
    public float bpm = 120f;

    [Header("Scale/Chords")]
    public ScaleType scaleType = ScaleType.Major;
    public int rootNote = 48; // C3
    public ChordProgression progression;

    public List<BassNote> GenerateBass()
    {
        List<BassNote> notes = new List<BassNote>();
        float beatSeconds = 60f / bpm;

        int[] scale = MusicScales.GetScale(scaleType);

        for (int bar = 0; bar < bars; bar++)
        {
            int degree = progression.degrees[bar % progression.degrees.Count];
            int chordRoot = rootNote + scale[degree];

            switch (bassPattern)
            {
                case BassPatternType.RootOnEachBeat:
                    AddRootOnBeats(notes, bar, chordRoot);
                    break;

                case BassPatternType.RootAndFifth:
                    AddRootAndFifth(notes, bar, chordRoot);
                    break;

                case BassPatternType.WalkingBass:
                    AddWalkingBass(notes, bar, chordRoot, scale);
                    break;

                case BassPatternType.ArpTriad:
                    AddArpTriad(notes, bar, chordRoot, scale);
                    break;

                case BassPatternType.RandomGroove:
                    AddRandomBass(notes, bar, chordRoot, scale);
                    break;
            }
        }

        // Биты → секунды
        for (int i = 0; i < notes.Count; i++)
        {
            var n = notes[i];
            n.startTime *= beatSeconds;
            notes[i] = n;
        }

        return notes;
    }

    private void AddNote(List<BassNote> list, float beat, int midi)
    {
        return;
        if (list == null) return;
        float pitch = Mathf.Pow(2f, (midi - 60) / 12f);

        list.Add(new BassNote(bassSample.sample, pitch, beat));
    }

    private void AddRootOnBeats(List<BassNote> list, int bar, int root)
    {
        for (int i = 0; i < 4; i++)
            AddNote(list, bar * 4 + i, root);
    }

    private void AddRootAndFifth(List<BassNote> list, int bar, int root)
    {
        AddNote(list, bar * 4 + 0, root);
        AddNote(list, bar * 4 + 1, root + 7); // квинта
        AddNote(list, bar * 4 + 2, root);
        AddNote(list, bar * 4 + 3, root + 7);
    }

    private void AddWalkingBass(List<BassNote> list, int bar, int root, int[] scale)
    {
        for (int i = 0; i < 4; i++)
        {
            int step = scale[(i * 2) % scale.Length];
            AddNote(list, bar * 4 + i, root + step);
        }
    }

    private void AddArpTriad(List<BassNote> list, int bar, int root, int[] scale)
    {
        int[] triad = { 0, 2, 4 };
        for (int i = 0; i < triad.Length; i++)
            AddNote(list, bar * 4 + i, root + scale[triad[i]]);
    }

    private void AddRandomBass(List<BassNote> list, int bar, int root, int[] scale)
    {
        for (int i = 0; i < 4; i++)
        {
            int step = scale[Random.Range(0, scale.Length)];
            AddNote(list, bar * 4 + i, root + step);
        }
    }
}

public enum AudioTrackType
{
    Melody,
    Drum,
    Bass
}

