using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChordPlayer : MonoBehaviour
{
    [Header("Генератор аккордов")]
    public ChordGenerator generator; // Генератор аккордов

    private List<ChordNote> chords;         // Список аккордов для воспроизведения
    private List<AudioSource> audioSources; // Активные аудиоисточники

    private void Start()
    {
        // Генерируем аккорды
        chords = generator.GenerateChords();
        audioSources = new List<AudioSource>();

        // Создаём отдельный AudioSource для каждого аккорда
        foreach (var chord in chords)
        {
            GameObject go = new GameObject("ChordNote");
            go.transform.parent = transform; // для удобства в иерархии
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;

            audioSources.Add(source);

            // Запускаем корутину для проигрывания каждого аккорда с задержкой
            StartCoroutine(PlayChord(source, chord));
        }
    }

    // Корутина для проигрывания аккорда с учётом времени старта
    private IEnumerator PlayChord(AudioSource src, ChordNote note)
    {
        // Ждём времени старта
        yield return new WaitForSeconds(note.startTime);

        // Назначаем клип и питч
        src.clip = note.clip;
        src.pitch = note.pitch;

        // Проигрываем ноту
        src.Play();
    }
}