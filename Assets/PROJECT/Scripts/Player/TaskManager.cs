using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public GameObject letterPrefab;
    public static TaskManager Instance;

    public List<Task> tasks = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddTask(string recieverName, string adress, string id)
    {
        tasks.Add(new Task(recieverName, adress, id));
        bool hasNoLetter = GameManager.Instance.GetPlayer().GetComponent<PlayerInteraction>().pickupedLetter == null;
        print("add task: " + hasNoLetter);
        if (hasNoLetter)
            StartCoroutine(GetPlayerNextLetter(tasks[0]));
        else
        {
            // Исправлено: передаем корректное количество оставшихся писем
            TaskUI.Instance.SetTask(tasks[0], Mathf.Max(0, tasks.Count - 1));
        }
    }

    public void NextTask()
    {
        // Удаляем выполненную задачу
        if (tasks.Count > 0)
        {
            tasks.RemoveAt(0);
        }

        // Показываем следующую задачу или скрываем UI если задач нет
        if (tasks.Count > 0)
        {
            StartCoroutine(GetPlayerNextLetter(tasks[0]));
        }
        else
        {
            TaskUI.Instance.HideTask();
        }
    }

    public IEnumerator GetPlayerNextLetter(Task task)
    {
        yield return new WaitForSeconds(3);

        // Проверяем, что у игрока все еще нет письма
        if (GameManager.Instance.GetPlayer().GetComponent<PlayerInteraction>().pickupedLetter == null)
        {
            var letterGO = Instantiate(letterPrefab);
            var letter = letterGO.GetComponent<Letter>();

            letter.recieverName = task.recieverName;
            letter.id = task.id;
            GameManager.Instance.GetPlayer()
                .GetComponent<PlayerInteraction>()
                .pickupedLetter = letter;
        }

        // Показываем задачу с корректным количеством оставшихся писем
        TaskUI.Instance.SetTask(task, Mathf.Max(0, tasks.Count - 1));
    }
}

[Serializable]
public struct Task
{
    public string recieverName;
    public string adress;
    public string id;

    public Task(string recieverName, string adress, string id)
    {
        this.recieverName = recieverName;
        this.adress = adress;
        this.id = id;
    }
}