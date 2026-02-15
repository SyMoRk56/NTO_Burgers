using UnityEngine;

public class Appletree : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }
    public GameObject[] apple;
    public AudioSource audioSource;

    public bool CheckInteract()
    {
        return !apple[0].activeSelf;
    }

    public void Interact()
    {
        if (!apple[0].activeSelf) return;
        GameManager.Instance.GetPlayer().GetComponentInChildren<Animator>().SetTrigger("AppleTree");
        foreach(var anim in apple)
        {
            anim.SetActive(false);
        }
        audioSource.Play();
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
