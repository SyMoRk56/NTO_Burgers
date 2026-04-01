using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    public List<Task> tasks = new();
    public MailCatalog mailCatalog;

    private void Awake()
    {
        if (Instance == this)
            Destroy(gameObject);
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
        try
        {
            var m = mailCatalog.mails[PlayerManager.instance.Day];
            foreach (var n in m)
            {
                tasks.Add(new Task(n.reciever, n.adress, n.id, n.isStory));
                print(n.reciever);
            }
            tasks.Reverse();
            foreach (var n in tasks)
            {
                print(n.recieverName);
            }
        }
        catch
        {

        }
    }
    public void UpdateDailyTasks()
    {
        tasks = new();
        var m = mailCatalog.mails[PlayerManager.instance.Day];
        foreach (var n in m)
        {
            tasks.Add(new Task(n.reciever, n.adress, n.id, n.isStory));
        }
        tasks.Reverse();
    }
    public void AddTask(string recieverName, string adress, string id, bool isStory)
    {
        if (!tasks.Exists(task => task.id == id))
        {
            tasks.Add(new Task(recieverName, adress, id, isStory));
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
        string tutorialName = "Tutorial_4";

        int removed = tasks.RemoveAll(task => task.id == taskId);
        print("RemoveTask");
        if (removed > 0)
        {
            Debug.Log($"✓ Задание с ID {taskId} удалено");
            Debug.Log($"  Осталось заданий: {tasks.Count}");
            if (tasks.Count == 0)
            {
                AddTask(tutorialName, tutorialName, tutorialName, true); PlayerMailInventory.Instance.AddMailToInventory(new Task(tutorialName, tutorialName, tutorialName, true));

            }
        }
        else
        {
            Debug.LogWarning($"Задание с ID {taskId} не найдено для удаления!");
            if (tasks.Count == 0)
            {
                AddTask(tutorialName, tutorialName, tutorialName, true); PlayerMailInventory.Instance.AddMailToInventory(new Task(tutorialName, tutorialName, tutorialName, true));

            }
        }
    }

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