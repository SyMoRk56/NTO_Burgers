using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public GameObject popup;
    public SphereCollider trigger;
    public DeskInteraction deskInteraction;

    private Image popupImage;
    private bool playerInRange = false;

    private void Start()
    {
        if(trigger != null)
        trigger.radius = GameConfig.interactionRange;

        // Получаем компонент Image из popup
        popupImage = popup.GetComponent<Image>();
        if (popupImage == null)
        {
            Debug.LogWarning("Popup doesn't have an Image component!");
            return;
        }

        // Устанавливаем начальную прозрачность
        SetPopupAlpha(0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Показываем popup только если UI панель стола закрыта
            if (deskInteraction != null && !deskInteraction.IsCanvasOpen)
            {
                SetPopupAlpha(1f);
            }

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
            playerInRange = false;

            // При выходе всегда скрываем popup
            SetPopupAlpha(0f);

            // Уведомляем скрипт стола о том, что игрок вышел из зоны
            if (deskInteraction != null)
            {
                deskInteraction.PlayerExited();
            }
        }
    }

    // Метод для установки прозрачности popup
    private void SetPopupAlpha(float alpha)
    {
        if (popupImage != null)
        {
            Color color = popupImage.color;
            color.a = alpha;
            popupImage.color = color;
        }

        // Управляем активностью всего GameObject
        popup.SetActive(alpha > 0);
    }

    // Метод для скрытия popup (вызывается при открытии UI панели)
    public void HidePopup()
    {
        SetPopupAlpha(0f);
    }

    // Метод для показа popup (вызывается при закрытии UI панели, если игрок в зоне)
    public void ShowPopup()
    {
        if (playerInRange)
        {
            SetPopupAlpha(1f);
        }
    }
}