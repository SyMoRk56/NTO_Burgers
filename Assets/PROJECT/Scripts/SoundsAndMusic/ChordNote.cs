using UnityEngine;

[System.Serializable]
public struct ChordNote
{
    public AudioClip clip;
    public float pitch;
    public float startTime;

    public ChordNote(AudioClip clip, float pitch, float startTime)
    {
        this.clip = clip;
        this.pitch = pitch;
        this.startTime = startTime;
    }
}
