using UnityEngine;
using System.Collections;

public class firstfloor : MonoBehaviour
{
    [Header("Настройки телепортации")]
    public Transform targetPosition; // Перетащи сюда объект, куда перемещать игрока
    public bool useTrigger = true; // Использовать ли триггер для обнаружения касания

    [Header("Настройки Canvas")]
    public Canvas loadingCanvas; // Перетащи сюда Canvas который будет включаться/выключаться
    public float delayBeforeTeleport = 0.25f; // Задержка перед телепортацией

    private bool isTeleporting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Если объект должен быть триггером, добавляем коллайдер
        if (useTrigger && GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            GetComponent<Collider>().isTrigger = true;
        }

        // Выключаем Canvas при старте если он назначен
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Если не используем триггер, можно использовать другие методы ввода
        if (!useTrigger && Input.GetMouseButtonDown(0) && !isTeleporting)
        {
            // Проверяем клик по объекту
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                StartCoroutine(TeleportWithCanvas());
            }
        }
    }

    // Корутина для телепортации с Canvas
    IEnumerator TeleportWithCanvas()
    {
        if (isTeleporting) yield break;

        isTeleporting = true;

        // Включаем Canvas
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(true);
        }

        // Ждем указанную задержку
        yield return new WaitForSeconds(delayBeforeTeleport);

        // Выполняем телепортацию
        TeleportPlayer();

        // Выключаем Canvas
        if (loadingCanvas != null)
        {
            loadingCanvas.gameObject.SetActive(false);
        }

        isTeleporting = false;
    }

    // Метод для телепортации игрока
    void TeleportPlayer()
    {
        if (targetPosition != null)
        {
            // Находим игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Перемещаем игрока к целевой позиции
                player.transform.position = targetPosition.position;
                Debug.Log("Игрок перемещен к " + targetPosition.name);
            }
            else
            {
                Debug.LogWarning("Игрок не найден! Убедитесь, что у игрока установлен тег 'Player'");
            }
        }
        else
        {
            Debug.LogWarning("Target Position не назначен! Перетащите объект-цель в инспекторе");
        }
    }

    // Автоматическая телепортация при входе в триггер
    void OnTriggerEnter(Collider other)
    {
        if (useTrigger && other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportWithCanvas());
        }
    }

    // Визуализация в редакторе (видна только в сцене)
    void OnDrawGizmos()
    {
        if (targetPosition != null)
        {
            // Рисуем линию от этого объекта к цели
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition.position);

            // Рисуем значок у целевой позиции
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(targetPosition.position, Vector3.one * 0.5f);
        }

        // Подсвечиваем этот объект
        Gizmos.color = useTrigger ? Color.blue : Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>() ? GetComponent<Collider>().bounds.size : Vector3.one);
    }
}