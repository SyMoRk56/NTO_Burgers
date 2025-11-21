using UnityEngine;
using System.Collections.Generic;

public class ChordGenerator : MonoBehaviour
{
    public AudioClip pianoSample;

    public int bpm = 120;
    public int bars = 4;

    public ScaleType scaleType = ScaleType.Major;
    public int rootNote = 60; // C4

    public ChordProgression progression;

    public Vector2 pitchMultiplyRange = new Vector2(0.98f, 1.02f);

    public enum ChordType { Triad, Seventh }
    public ChordType chordType = ChordType.Triad;

    public List<ChordNote> GenerateChords()
    {
        var chords = new List<ChordNote>();
        float beatSeconds = 60f / bpm;
        int[] scale = MusicScales.GetScale(scaleType);

        for (int bar = 0; bar < bars; bar++)
        {
            int degree = progression.degrees[bar % progression.degrees.Count];
            print(scale.Length);
            print(degree);
            int chordRootMidi = rootNote + scale[degree];

            // Генерация аккорда
            var notes = BuildChord(chordRootMidi, scale);

            // Добавляем каждую ноту аккорда
            foreach (int midi in notes)
            {
                float pitch = Mathf.Pow(2f, (midi - 60) / 12f)
                              * Random.Range(pitchMultiplyRange.x, pitchMultiplyRange.y);

                chords.Add(new ChordNote(pianoSample, pitch, bar * 4f * beatSeconds));
            }
        }
        return chords;
    }

    private List<int> BuildChord(int rootMidi, int[] scale)
    {
        // В мажоре/миноре интервалы внутри шкалы → индексы через ступени
        int third = scale[(2 % scale.Length)];
        int fifth = scale[(4 % scale.Length)];
        int seventh = scale[(6 % scale.Length)];

        var chord = new List<int>
        {
            rootMidi,
            rootMidi + (third),
            rootMidi + (fifth)
        };

        if (chordType == ChordType.Seventh)
            chord.Add(rootMidi + seventh);

        return chord;
    }
}
