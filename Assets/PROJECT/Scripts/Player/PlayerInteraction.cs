using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerManager manager;
    public AudioSource mailSource;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, GameConfig.interactionRange);
            Debug.Log($"=== ПОИСК ВЗАИМОДЕЙСТВИЙ ===");
            Debug.Log($"Найдено объектов в радиусе: {hits.Length}");

            bool interactionHandled = false;

            // ПЕРВЫЙ ПРИОРИТЕТ: Почтовые ящики
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out MailBox box))
                {
                    Debug.Log($"✓ ВЗАИМОДЕЙСТВИЕ С ПОЧТОВЫМ ЯЩИКОМ");
                    Debug.Log($"  Ящик: {box.name}");
                    Debug.Log($"  Адрес ящика: '{box.mailboxAddress}'");

                    // Проверяем состояние инвентаря перед взаимодействием
                    if (PlayerMailInventory.Instance != null)
                    {
                        var allMails = PlayerMailInventory.Instance.GetAllMails();
                        Debug.Log($"  Писем в инвентаре: {allMails.Count}");
                        foreach (var mail in allMails)
                        {
                            Debug.Log($"    - {mail.recieverName} -> '{mail.adress}'");
                        }
                    }

                    box.Interact();
                    interactionHandled = true;
                    mailSource.Play();
                    break;
                }
            }

            if (interactionHandled) return;

            // ВТОРОЙ ПРИОРИТЕТ: Остальные взаимодействия
            foreach (var hit in hits)
            {
                Debug.Log($"Объект: {hit.name} (тег: {hit.tag})");

                if (hit.CompareTag("Dialog"))
                {
                    Debug.Log("Взаимодействие с диалогом");
                    hit.GetComponent<DialogueRunner>().StartDialogue(false);
                    interactionHandled = true;
                    break;
                }

                if (hit.CompareTag("Pickup"))
                {
                    Debug.Log("Взаимодействие с пикапом");
                    PickupObject(hit.gameObject);
                    interactionHandled = true;
                    break;
                }

                if (hit.TryGetComponent(out EnterToHouse enter))
                {
                    Debug.Log("Вход в дом");
                    enter.Interact();
                    interactionHandled = true;
                    break;
                }
            }

            if (!interactionHandled)
            {
                Debug.Log("✗ Ни один объект не обработан");
            }
        }
    }

    public void PickupObject(GameObject go)
    {
        Debug.Log("Подобран объект: " + go.name);
    }
}