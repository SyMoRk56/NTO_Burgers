using UnityEngine;

public class Appletree : MonoBehaviour
{
    public GameObject[] apple;
    public AudioSource audioSource;
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
}
