using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerManager manager;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, GameConfig.interactionRange);
            foreach(var hit in hits)
            {
                if (hit.CompareTag("Dialog"))
                {
                    print("StartDialogue");
                    hit.GetComponent<DialogueRunner>().StartDialogue();
                    break;
                }
            }
        }
    }
}
