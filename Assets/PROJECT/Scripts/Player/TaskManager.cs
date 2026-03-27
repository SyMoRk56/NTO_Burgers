using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // Активные задания (письма которые игрок УЖЕ взял со стола или получил через туториал)
    public List<Task> tasks = new();

    // mailCatalog оставляем для обратной совместимости, но НЕ заполняем tasks из него
    public MailCatalog mailCatalog;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✓ TaskManager инициализирован");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Не заполняем tasks из каталога — этим занимается DailyMailScheduler
        tasks = new List<Task>();
    }

    public void AddTask(string recieverName, string adress, string id)
    {
        if (!tasks.Exists(t => t.id == id))
        {
            tasks.Add(new Task(recieverName, adress, id));
            Debug.Log($"✓ Добавлено задание: {recieverName} -> {adress} (ID: {id})");
        }
        else
        {
            Debug.LogWarning($"Задание с ID {id} уже существует!");
        }
    }

    public void RemoveTask(string taskId)
    {
        int removed = tasks.RemoveAll(t => t.id == taskId);
        if (removed > 0)
            Debug.Log($"✓ Задание с ID {taskId} удалено. Осталось: {tasks.Count}");
        else
            Debug.LogWarning($"Задание с ID {taskId} не найдено!");
    }

    public void DebugState()
    {
        Debug.Log($"=== TaskManager: {tasks.Count} заданий ===");
        foreach (var t in tasks)
            Debug.Log($" - {t.recieverName} -> {t.adress} (ID: {t.id})");
    }
}