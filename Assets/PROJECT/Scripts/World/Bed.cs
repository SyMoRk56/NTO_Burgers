using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bed : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public bool CheckInteract()
    {
        print("Bed check interact ");
        var taskmtasks = new List<Task>();
        foreach(var t in PlayerMailInventory.Instance.carriedMails)
        {
            print(t.adress);
            if(!t.adress.Contains("Tutorial")) taskmtasks.Add(t);
        }
        var taskmtasksm = new List<Task>();
        foreach (var t in TaskManager.Instance.tasks)
        {
            print(t.adress);
            if (!t.adress.Contains("Tutorial")) taskmtasksm.Add(t);
        }
        return taskmtasks.Count == 0 && taskmtasksm.Count==0;
    }

    public void Interact()
    {
        PlayerManager.instance.Day += 1;
        PlayerMailInventory.Instance.RemoveMailFromInventory("Tutorial_4") ;
        TaskManager.Instance.RemoveTask("Tutorial_4");
        FindFirstObjectByType<EnterToHouse>().Transition();
        if(PlayerManager.instance.Day == 4)
        {
            Invoke(nameof(LoadFinalScene), 1f);
        }
    }
    void LoadFinalScene()
    {
        SceneManager.LoadScene("Final");
    }
    public int InteractPriority()
    {
        return 0;
    }

    public void OnBeginInteract()
    {

    }

    public void OnEndInteract(bool success)
    {

    }
}
