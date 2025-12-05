using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    public List<Task> tasks = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✓ TaskManager инициализирован");
        }
        else
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }
        tasks = new();
        var m = Resources.LoadAll<MailCatalog>("");
        foreach(var n in m[0].mails)
        {
            tasks.Add(new Task(n.reciever, n.adress, n.id));
        }
    }

    public void AddTask(string recieverName, string adress, string id)
    {
        // Проверяем, нет ли уже такого задания
        if (!tasks.Exists(task => task.id == id))
        {
            tasks.Add(new Task(recieverName, adress, id));
            Debug.Log($"✓ Добавлено задание: {recieverName} -> {adress} (ID: {id})");
            Debug.Log($"  Всего заданий: {tasks.Count}");
        }
        else
        {
            Debug.LogWarning($"Задание с ID {id} уже существует!");
        }
    }

    public void RemoveTask(string taskId)
    {
        int removed = tasks.RemoveAll(task => task.id == taskId);
        if (removed > 0)
        {
            Debug.Log($"✓ Задание с ID {taskId} удалено");
            Debug.Log($"  Осталось заданий: {tasks.Count}");
        }
        else
        {
            Debug.LogWarning($"Задание с ID {taskId} не найдено для удаления!");
        }
    }

    // Метод для проверки состояния
    public void DebugState()
    {
        Debug.Log($"=== СОСТОЯНИЕ TASKMANAGER ===");
        Debug.Log($"Всего заданий: {tasks.Count}");
        foreach (var task in tasks)
        {
            Debug.Log($" - {task.recieverName} -> {task.adress} (ID: {task.id})");
        }
    }
}