using UnityEngine;

public class Appletree : MonoBehaviour, IInteractObject
{
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
        throw new System.NotImplementedException();
    }

    public void OnEndInteract(bool succes)
    {
        throw new System.NotImplementedException();
    }
}
