using System.Collections;
using UnityEngine;

public class WallMap : MonoBehaviour, IInteractObject
{
    public bool canPickup;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        if(FindFirstObjectByType<BagPickup>(FindObjectsInactive.Exclude) == null)
        {
            canPickup = true;
        }
    }
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public bool CheckInteract()
    {
        return canPickup;
    }

    public void Interact()
    {
        print("WAll map interact");
        if (TaskUI.Instance != null)
            TaskUI.Instance.SetHasBag(true);
        transform.position += new Vector3(0, 1000, 0);
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
