using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public GameObject popup;
    public SphereCollider trigger;
    public DeskInteraction deskInteraction; // Опционально, только для объектов с UI

    private Image popupImage;
    private bool playerInRange = false;

    private void Start()
    {
        trigger.radius = GameConfig.interactionRange;

        // Получаем компонент Image из popup
        popupImage = popup.GetComponent<Image>();
        if (popupImage == null)
        {
            Debug.LogWarning("Popup doesn't have an Image component!");
            return;
        }

        // Скрываем popup полностью при старте
        SetPopupAlpha(0f);
        popup.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Player entered {gameObject.name} trigger");

            // Для объектов со столом: показываем popup только если UI закрыт
            // Для объектов без стола (NPC и др.): всегда показываем popup
            if (deskInteraction != null)
            {
                if (!deskInteraction.IsCanvasOpen)
                {
                    ShowPopup();
                }
            }
            else
            {
                // NPC и другие объекты без DeskInteraction
                ShowPopup();
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
            Debug.Log($"Player exited {gameObject.name} trigger");
            HidePopup();

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
    }

    // Метод для скрытия popup
    public void HidePopup()
    {
        SetPopupAlpha(0f);
        popup.SetActive(false);
        Debug.Log($"HidePopup called for {gameObject.name}");
    }

    // Метод для показа popup
    public void ShowPopup()
    {
        SetPopupAlpha(1f);
        popup.SetActive(true);
        Debug.Log($"ShowPopup called for {gameObject.name}");
    }

    // Метод для обновления видимости popup (вызывается из DeskInteraction при закрытии UI)
    public void UpdatePopupVisibility()
    {
        if (playerInRange)
        {
            if (deskInteraction != null)
            {
                // Для стола: показываем только если UI закрыт
                if (!deskInteraction.IsCanvasOpen)
                {
                    ShowPopup();
                }
                else
                {
                    HidePopup();
                }
            }
            else
            {
                // Для NPC: всегда показываем если игрок в зоне
                ShowPopup();
            }
        }
        else
        {
            HidePopup();
        }
    }
}