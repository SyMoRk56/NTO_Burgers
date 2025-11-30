using UnityEngine;

[CreateAssetMenu(menuName = "MusicGen/Instrument Sample")]
public class InstrumentSample : ScriptableObject
{
    public string instrumentName;
    public AudioClip sample;
}

