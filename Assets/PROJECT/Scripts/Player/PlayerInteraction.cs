using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerManager manager;
    public AudioSource mailSource;
    public PlayerMovement mov;

    IEnumerator HoldInteract(IInteractObject interactObject)
    {
        PlayerManager.instance.CanMove = false;
        float r = 0;
        interactObject.OnBeginInteract();
        bool succes = false;
        while (Input.GetKey(KeyCode.E))
        {
            r+= Time.deltaTime;
            if (r > 1)
            {
                succes = true;
                break;
            }
            yield return null;
        }
        interactObject.OnEndInteract(succes);
        PlayerManager.instance.CanMove = true;
        if (succes) Interact(interactObject);


    }
    private void Update()
    {
        if (!manager.CanMove) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, GameConfig.interactionRange);
            Debug.Log($"=== ПОИСК ВЗАИМОДЕЙСТВИЙ ===");
            Debug.Log($"Найдено объектов в радиусе: {hits.Length}");

            bool interactionHandled = false;
            var interactables = hits.Select(c => c.GetComponent<IInteractObject>()).Where(i => i != null).ToArray();
            if (CheckInteract(interactables, out IInteractObject interactObject))
            {
                StartCoroutine(HoldInteract(interactObject));
            }
            // ПЕРВЫЙ ПРИОРИТЕТ: Почтовые ящики
            //Interact(hits, ref interactionHandled);
        }
    }

    private void Interact(IInteractObject interactObject)
    {
        interactObject.Interact();
        return;
    }

    public void PickupObject(GameObject go)
    {
        
        Debug.Log("Подобран объект: " + go.name);
    }
    public bool CheckInteract(IInteractObject[] hits, out IInteractObject interactObject)
    {
        print("Check interact");
        interactObject = null;
        hits.OrderByDescending((a) => a.InteractPriority());
        bool can = false;
        foreach (IInteractObject interact in hits)
        {
            if (interact.CheckInteract())
            {
                interactObject = interact;
                can = true;
                break;
            }
        }
        return can;
    }
    
}