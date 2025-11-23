using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public GameObject letterPrefab;
    public static TaskManager Instance;
    public Queue<Task> tasks = new();
    private void Awake()
    {
        Instance = this;
    }
    public void AddTask(string recieverName, string adress)
    {
        tasks.Enqueue(new Task(recieverName, adress));
        StartCoroutine(GetPlayerNextLetter(tasks.Peek()));
    }
    public void NextTask()
    {
        if (tasks.Count == 0) return;
        TaskUI.Instance.SetTask(new("",""), -1);
        var task = tasks.Dequeue();
        StartCoroutine(GetPlayerNextLetter(task));
    }
    IEnumerator GetPlayerNextLetter(Task task)
    {
        yield return new WaitForSeconds(3);
        var letterGO = Instantiate(letterPrefab);
        var letter = letterGO.GetComponent<Letter>();
        letter.recieverName = task.recieverName;
        GameManager.Instance.GetPlayer().GetComponent<PlayerInteraction>().pickupedLetter = letter;
        TaskUI.Instance.SetTask(task, tasks.Count);
    }
}
public struct Task
{
    public string recieverName;
    public string adress;
    public Task(string recieverName, string adress)
    {
        this.recieverName = recieverName;
        this.adress = adress;
    }
}