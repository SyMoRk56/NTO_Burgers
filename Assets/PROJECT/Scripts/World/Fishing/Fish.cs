using UnityEngine;

public class Fish : MonoBehaviour, IInteractObject
{
    public FishScriptableObject fish;
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public bool CheckInteract()
    {
        return !GameObject.Find("NPC - CAT").GetComponent<NPCController>().isGoingForFish;
    }

    public void Interact()
    {
        FishInventory.instance.AddFishToInventory(fish);
        Destroy(gameObject);
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
