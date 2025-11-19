using UnityEngine;
using DG.Tweening;
using System.Collections;
public class LocationMusicManager : MonoBehaviour
{
    public static LocationMusicManager Instance;
    void Awake()
    {
        Instance = this;
    }
    public AudioSource source;
    public void PlayMusic(AudioClip clip)
    {
        StopAllCoroutines();
        StartCoroutine(PlayMusicCoroutine(clip));
    }
    IEnumerator PlayMusicCoroutine(AudioClip clip)
    {
        source.Stop();
        source.DOFade(0, 2f);
        yield return new WaitForSeconds(2);
        source.DOFade(1, 2f);
        source.clip = clip;
        source.Play();
    }
}
