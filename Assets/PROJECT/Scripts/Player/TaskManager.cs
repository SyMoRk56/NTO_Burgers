using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public GameObject letterPrefab;
    public static TaskManager Instance;
    public Queue<Task> tasks;
    private void Awake()
    {
        Instance = this;
    }
    public void AddTask(string recieverName, string adress, string id)
    {
        tasks.Enqueue(new Task(recieverName, adress, id));
        StartCoroutine(GetPlayerNextLetter(tasks.Peek()));
    }
    public void NextTask()
    {
        TaskUI.Instance.SetTask(new("","", ""));
        var task = tasks.Dequeue();
        StartCoroutine(GetPlayerNextLetter(task));
    }
    public IEnumerator GetPlayerNextLetter(Task task)
    {
        yield return new WaitForSeconds(3);
        var letterGO = Instantiate(letterPrefab);
        var letter = letterGO.GetComponent<Letter>();
        letter.recieverName = task.recieverName;
        GameManager.Instance.GetPlayer().GetComponent<PlayerInteraction>().pickupedLetter = letter;
        TaskUI.Instance.SetTask(task);
    }
}
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