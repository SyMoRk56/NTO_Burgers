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
        bool b = GameManager.Instance.GetPlayer().GetComponent<PlayerInteraction>().pickupedLetter == null;
        print("add task: " + b);
        if (b)
            StartCoroutine(GetPlayerNextLetter(tasks[0])); // ������ Queue.Peek()
        else
        {
            TaskUI.Instance.SetTask(tasks[0], tasks.Count - 1);
        }
    }

    public void NextTask()
    {
        TaskUI.Instance.SetTask(new("", "", ""), 0);

        if (tasks.Count == 0) return;
        var task = tasks[0]; // ������ Queue.Dequeue()
        tasks.RemoveAt(0);

        StartCoroutine(GetPlayerNextLetter(task));
    }

    public IEnumerator GetPlayerNextLetter(Task task)
    {
        yield return new WaitForSeconds(3);

        var letterGO = Instantiate(letterPrefab);
        var letter = letterGO.GetComponent<Letter>();

        letter.recieverName = task.recieverName;
        letter.id = task.id;
        GameManager.Instance.GetPlayer()
            .GetComponent<PlayerInteraction>()
            .pickupedLetter = letter;

        TaskUI.Instance.SetTask(task, tasks.Count - 1);
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
