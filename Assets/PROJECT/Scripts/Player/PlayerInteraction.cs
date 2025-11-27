using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Letter pickupedLetter;
    public PlayerManager manager;

    private void Update()
    {
        if (pickupedLetter != null)
        {
            pickupedLetter.transform.position = transform.position + transform.forward;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (pickupedLetter == null)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, GameConfig.interactionRange);

                foreach (var hit in hits)
                {
                    print("Hit: " + hit.name + " " + hit.tag);

                    if (hit.CompareTag("Dialog"))
                    {
                        hit.GetComponent<DialogueRunner>().StartDialogue(false);
                        return;
                    }

                    if (hit.CompareTag("Pickup"))
                    {
                        PickupObject(hit.gameObject);
                        return;
                    }

                    if (hit.TryGetComponent(out MailBox box))
                    {
                        box.Interact();
                        return;
                    }

                    // ✅ ВХОД В ДОМ
                    if (hit.TryGetComponent(out EnterToHouse enter))
                    {
                        enter.Interact();
                        return;
                    }
                }
            }
            else
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, GameConfig.interactionRange);

                // Сначала проверяем диалоги (для сдачи письма)
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Dialog"))
                    {
                        var dialog = hit.GetComponent<DialogueRunner>();
                        if (dialog.ownerName == pickupedLetter.recieverName)
                        {
                            // Сначала сохраняем ссылку на письмо
                            Letter letterToDeliver = pickupedLetter;

                            // Очищаем ссылку ДО начала диалога
                            pickupedLetter = null;

                            // Запускаем диалог и логику доставки
                            dialog.StartDialogue(true);
                            MailManager.Instance.SetDelivered(letterToDeliver.id, true);
                            TaskManager.Instance.NextTask();
                            Destroy(letterToDeliver.gameObject);
                            return;
                        }
                    }
                }

                // Если диалог не найден, проверяем дверь
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out EnterToHouse enter))
                    {
                        // Телепортируемся с письмом
                        enter.InteractWithLetter(pickupedLetter);
                        return;
                    }
                }

                // Если не у диалога и не у двери - дропаем письмо
                ReleaseObject();
            }
        }
    }

    public void ReleaseObject()
    {
        if (pickupedLetter != null)
        {
            // Включаем физику при дропе
            Rigidbody rb = pickupedLetter.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            pickupedLetter = null;
        }
    }

    public void PickupObject(GameObject go)
    {
        print("Pickup");
        pickupedLetter = go.GetComponent<Letter>();

        // Отключаем физику при поднятии
        if (pickupedLetter != null)
        {
            Rigidbody rb = pickupedLetter.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
}