using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerManager manager;
    public AudioSource mailSource;
    public PlayerMovement mov;
    public GameObject interactionCanvas;
    public Image interactionCircle;

    private void Start()
    {
        interactionCanvas.SetActive(false);
    }
    IEnumerator HoldInteract((Transform,IInteractObject) interactObjectPair)
    {
        yield return 0;
        Transform target = interactObjectPair.Item1;
        var interactObject = interactObjectPair.Item2;
        interactionCanvas.SetActive(true);
        PlayerManager.instance.CanMove = false;
        float r = 0;
        interactObject.OnBeginInteract();
        bool succes = false;
        //while (Input.GetKey(KeyCode.E))
        //{
        //    Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + new Vector3(0, .5f, 0));
        //    interactionCircle.transform.position = screenPos;
        //    interactionCircle.fillAmount = r;
        //    r += Time.deltaTime;
        //    if (r > 1)
        //    {
                
        //        succes = true;
        //        break;
        //    }
        //    yield return null;
        //}

        // обычный режим
        succes = true;
        // конец обычного режима
        interactionCircle.fillAmount = 0;
        interactionCanvas.SetActive(false);
        interactObject.OnEndInteract(succes);
        PlayerManager.instance.CanMove = true;
        if (succes) Interact(interactObject);


    }
    private void Update()
    {
        if (!manager.CanMove) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Если что-то в руке — взаимодействуем с этим
            if (PlayerManager.instance.hand.childCount != 0)
            {
                var interact = PlayerManager.instance.hand
                    .GetChild(0)
                    .GetComponent<IInteractObject>();

                interact?.Interact();
                return;
            }

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                GameConfig.innerInteractionRange,
                ~0,
                QueryTriggerInteraction.Collide
            );

            Debug.Log("=== ПОИСК ВЗАИМОДЕЙСТВИЙ ===");
            Debug.Log($"Найдено объектов в радиусе: {hits.Length}");

            // Формируем пары (Transform, IInteractObject)
            var interactables = hits
                .Select(c => (c.transform, c.GetComponent<IInteractObject>()))
                .Where(pair => pair.Item2 != null)
                .ToArray();

            if (CheckInteract(interactables, out var interactObject))
            {
                StartCoroutine(HoldInteract(interactObject));
            }
        }
    }


    private void Interact(IInteractObject interactObject)
    {
        interactObject.Interact();
        return;
    }

    public bool CheckInteract((Transform transform, IInteractObject interact)[] hits, out (Transform,IInteractObject) interactObject)
    {
        print($"Check interact {Time.time} {hits.Length}");

        interactObject = (null,null);

        // ВАЖНО: OrderBy возвращает новый IEnumerable
        var orderedHits = hits
            .OrderByDescending(h => h.interact.InteractPriority());

        foreach (var hit in orderedHits)
        {
            print($"{Time.time} {hit.transform.name}");

            if (hit.interact.CheckInteract())
            {
                interactObject = (hit.transform, hit.interact);
                return true;
            }
        }

        return false;
    }


}