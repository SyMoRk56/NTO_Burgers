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

                foreach (var hit in hits)
                {
                    print("Hit: " + hit.name + " " + hit.tag);

                    if (hit.CompareTag("Dialog"))
                    {
                        var dialog = hit.GetComponent<DialogueRunner>();
                        if (dialog.ownerName == pickupedLetter.recieverName)
                        {
                            dialog.StartDialogue(true);
                            TaskManager.Instance.NextTask();
                            Destroy(pickupedLetter.gameObject);
                        }
                        return;
                    }
                }

                ReleaseObject();
            }
        }
    }

    public void ReleaseObject()
    {
        pickupedLetter = null;
    }

    public void PickupObject(GameObject go)
    {
        print("Pickup");
        pickupedLetter = go.GetComponent<Letter>();
    }
}
