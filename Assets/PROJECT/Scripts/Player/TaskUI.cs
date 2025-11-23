using TMPro;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance;
    public GameObject task;
    public TMP_Text reciever;
    public TMP_Text adress;

    private void Awake()
    {
        Instance = this;
    }
    public void SetTask(Task task)
    {
        this.task.SetActive(true);
        if(task.recieverName == "" || task.adress == "")
        {
            this.task.SetActive(false);
            return;
        }
        reciever.text = task.recieverName;
        adress.text = task.adress;
    }
}
