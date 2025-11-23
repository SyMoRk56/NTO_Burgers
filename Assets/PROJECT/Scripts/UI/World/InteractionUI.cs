using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public GameObject popup;
    public SphereCollider trigger;
    public DeskInteraction deskInteraction; // Добавляем ссылку на скрипт стола

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

            // Уведомляем скрипт стола о том, что игрок вошел в зону
            if (deskInteraction != null)
            {
                deskInteraction.PlayerEntered();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popup.SetActive(false);

            // Уведомляем скрипт стола о том, что игрок вышел из зоны
            if (deskInteraction != null)
            {
                deskInteraction.PlayerExited();
            }
        }
    }
}