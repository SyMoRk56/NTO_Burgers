using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Простой конвертер AudioClip -> Minecraft-подобная мелодия
public class MinecraftMelodyGenerator : MonoBehaviour
{
    [Header("Один звук ноты")]
    public AudioClip baseNote;

    [Header("Настройки мелодии")]
    public int melodyLength = 32;
    public float noteDuration = 0.3f;

    // частоты нот мажорной гаммы
    private float[] freqs = { 261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 440.00f, 493.88f };
    private string[] names = { "C", "D", "E", "F", "G", "A", "B" };

    void Start()
    {
        var melody = GenerateMelody(melodyLength);
        StartCoroutine(SpawnNoteObjects(melody));
    }

    List<int> GenerateMelody(int length)
    {
        List<int> result = new List<int>();
        int last = Random.Range(0, 7);

        for (int i = 0; i < length; i++)
        {
            int next = last + Random.Range(-1, 2); // плавные шаги
            next = Mathf.Clamp(next, 0, 6);

            if ((i + 1) % 4 == 0)
            {
                int[] stable = { 0, 2, 4 }; // C E G
                next = stable[Random.Range(0, stable.Length)];
            }

            result.Add(next);
            last = next;
        }

        return result;
    }

    System.Collections. IEnumerator SpawnNoteObjects(List<int> melody)
    {
        float baseFreq = 261.63f; // частота baseNote (если это C)

        foreach (int noteIndex in melody)
        {
            GameObject noteObj = new GameObject("Note_" + names[noteIndex]);

            AudioSource src = noteObj.AddComponent<AudioSource>();
            src.clip = baseNote;

            float pitch = freqs[noteIndex] / baseFreq;
            src.pitch = pitch;

            src.Play();

            // уничтожаем объект после проигрывания
            Destroy(noteObj, noteDuration + 0.1f);

            yield return new WaitForSeconds(noteDuration);
        }
    }
}
