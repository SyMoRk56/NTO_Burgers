using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DialogueRunner))] // Гарантируем, что DialogueRunner есть на объекте
public class TutorialNPC : MonoBehaviour
{
    Transform player; // Ссылка на игрока — для поворота и проверки дистанции

    private void Start()
    {
        // Получаем трансформ игрока через менеджер (единая точка доступа)
        player = GameManager.Instance.GetPlayer().transform;
    }

    private void Update()
    {
        // Проверяем дистанцию до игрока — логика работает только в радиусе 5 единиц
        if (Vector3.Distance(transform.position, player.position) < 5)
        {
            // Вычисляем направление на игрока
            Vector3 direction = player.position - transform.position;

            // Обнуляем Y, чтобы НПЦ поворачивался только по горизонтали (не "кивал")
            direction.y = 0;

            // Если направление валидное — поворачиваемся в сторону игрока
            if (direction != Vector3.zero)
            {
                // Целевой поворот: смотрим на игрока, сохраняя вертикаль мира
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                // Плавный поворот (Slerp) — 5 ед/сек, независимо от частоты кадров
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
            }
        }
    }

    // Вызывается извне, когда игрок выходит из дома — запускаем диалог с небольшой задержкой
    public void OnPlayerExitedHouse()
    {
        StartCoroutine(OnPlayerExitedHouseDelayed());
    }

    // Задержка нужна, чтобы игрок успел полностью выйти из триггера/зоны перед стартом диалога
    IEnumerator OnPlayerExitedHouseDelayed()
    {
        yield return new WaitForSeconds(.5f);
        // Запускаем диалог: true — вероятно, означает "принудительно" или "без проверки условий"
        GetComponent<DialogueRunner>().StartDialogue(true);
    }
}