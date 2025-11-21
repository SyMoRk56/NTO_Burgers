using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMusic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public enum ScaleType
{
    Major,
    Minor
}
public static class MusicScales
{
    // Мажор: 0, 2, 4, 5, 7, 9, 11
    private static readonly int[] MAJOR = { 0, 2, 4, 5, 7, 9, 11 };

    // Минор: 0, 2, 3, 5, 7, 8, 10
    private static readonly int[] MINOR = { 0, 2, 3, 5, 7, 8, 10 };

    public static int[] GetScale(ScaleType type)
    {
        return type == ScaleType.Major ? MAJOR : MINOR;
    }
}
[System.Serializable]
public class ChordProgression
{
    public string name;
    public List<int> degrees = new List<int>();

    // Пример: I–V–vi–IV → 0, 4, 5, 3
}
public struct Note
{
    public AudioClip clip;
    public float pitch;
    public float startTime;
    public bool isRest;

    public Note(AudioClip clip, float pitch, float startTime, bool isRest = false)
    {
        this.clip = clip;
        this.pitch = pitch;
        this.startTime = startTime;
        this.isRest = isRest;
    }
}

[CreateAssetMenu(menuName = "MusicGen/Instrument Sample")]
public class InstrumentSample : ScriptableObject
{
    public string instrumentName;
    public AudioClip sample;
}

