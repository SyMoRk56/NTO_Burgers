using UnityEngine;

public class Bed : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public bool CheckInteract()
    {
        return PlayerMailInventory.Instance.carriedMails.Count == 0 && TaskManager.Instance.tasks.Count == 0;
    }

    public void Interact()
    {
        PlayerManager.instance.Day += 1;
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
