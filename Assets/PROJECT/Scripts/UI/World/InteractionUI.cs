using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public GameObject popup;
    public SphereCollider trigger;

    private void Start()
    {
        trigger.radius = GameConfig.interactionRange;
        popup.SetActive(false);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popup.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popup.SetActive(false);
        }
    }
}
