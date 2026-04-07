using UnityEngine;
using TMPro;

public class TutorialHintUI : MonoBehaviour
{
    [Header("Hint объекты (каждый — Panel с текстом)")]
    public GameObject inventoryHint;      // "Нажми [Tab] чтобы открыть инвентарь"
    public GameObject letterHint;         // "Просмотри письма в инвентаре"
    public GameObject deliveryHint;       // "Доставь письмо: ..."
    public TextMeshProUGUI deliveryLabel;

    void Awake() => HideAll();

    public void ShowInventoryHint() { HideAll(); inventoryHint?.SetActive(true); }
    public void ShowLetterHint() { HideAll(); letterHint?.SetActive(true); }

    public void ShowDeliveryHint(string recipientId)
    {
        HideAll();
        if (deliveryHint != null)
        {
            deliveryHint.SetActive(true);
            if (deliveryLabel != null)
                deliveryLabel.text = $"Доставь письмо: {recipientId}";
        }
    }

    public void HideAll()
    {
        inventoryHint?.SetActive(false);
        letterHint?.SetActive(false);
        deliveryHint?.SetActive(false);
    }
}