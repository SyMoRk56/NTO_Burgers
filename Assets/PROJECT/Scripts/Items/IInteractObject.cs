using UnityEngine;

public interface IInteractObject
{
    public int InteractPriority();
    public bool CheckInteract();
    public void Interact();

    public void OnBeginInteract();

    public void OnEndInteract(bool success);

    public bool CheckDistance();
}
