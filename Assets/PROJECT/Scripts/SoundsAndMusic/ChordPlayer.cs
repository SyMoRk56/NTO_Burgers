using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChordPlayer : MonoBehaviour
{
    public ChordGenerator generator;
    private List<ChordNote> chords;
    private List<AudioSource> audioSources;

    void Start()
    {
        chords = generator.GenerateChords();
        audioSources = new List<AudioSource>();

        foreach (var chord in chords)
        {
            GameObject go = new GameObject("ChordNote");
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;

            audioSources.Add(source);

            StartCoroutine(PlayChord(source, chord));
        }
    }

    IEnumerator PlayChord(AudioSource src, ChordNote note)
    {
        yield return new WaitForSeconds(note.startTime);

        src.clip = note.clip;
        src.pitch = note.pitch;
        src.Play();
    }
}
