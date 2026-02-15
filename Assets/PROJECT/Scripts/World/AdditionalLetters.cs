using System.Collections.Generic;
using UnityEngine;

public class AdditionalLetters : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }
    public bool hasMails = true;
    public MailCatalog mails;
    public bool CheckInteract()
    {
        if (PlayerMailInventory.Instance.carriedMails.Count == 0) return true;
        return hasMails && !PlayerMailInventory.Instance.carriedMails[0].recieverName.Contains("Tutorial");
    }

    public void Interact()
    {
        foreach (var task in mails.mails)
        {
            PlayerMailInventory.Instance.AddMailToInventory(new (task.reciever, task.adress, task.id));
            hasMails = false;
        }
    }

    public int InteractPriority()
    {
        return 1;
    }

    public void OnBeginInteract()
    {
        
    }

    public void OnEndInteract(bool success)
    {
        
    }

    
}
