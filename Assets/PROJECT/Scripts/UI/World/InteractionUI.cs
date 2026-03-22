using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    public GameObject outerPopup;   // первый объект
    public GameObject innerPopup;   // второй объект
    public SphereCollider trigger;
    public DeskUI deskInteraction; // опционально

    [Header("Radii")]
    public float innerRadius = 3, outerRadius = 7;
    public float innerRadiusMultiplier = 1, outerRadiusMultiplier = 1;
    private bool playerInRange = false;
    private Transform player;
    IInteractObject io;
    public bool needToLockRotation;
    private void Start()
    {
        io = GetComponentInParent<IInteractObject>();
        innerRadius = GameConfig.innerInteractionRange * innerRadiusMultiplier;
        outerRadius = GameConfig.outerInteractionRange * outerRadiusMultiplier;
        trigger.isTrigger = true;
        trigger.radius = outerRadius;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (transform.parent == null)
        {
            transform.localScale = Vector3.one;
            return;
        }

        Vector3 parentScale = transform.parent.lossyScale;

        transform.localScale = new Vector3(
            Vector3.one.x / parentScale.x,
            Vector3.one.y / parentScale.y,
            Vector3.one.z / parentScale.z
        );

        print(transform.lossyScale);
        HideAllPopups();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        Debug.Log($"Player entered {gameObject.name}");

        if (deskInteraction != null)
            deskInteraction.PlayerEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        Debug.Log($"Player exited {gameObject.name}");

        HideAllPopups();

        if (deskInteraction != null)
            deskInteraction.PlayerExited();
    }

    private void Update()
    {
        if(needToLockRotation)transform.rotation = Quaternion.identity;
        if (!playerInRange || player == null)
        {
            HideAllPopups();
            return;
        }

        // если стол и UI открыт — ничего не показываем
        if (deskInteraction != null && deskInteraction.IsCanvasOpen)
        {
            HideAllPopups();
            return;
        }

        // если сейчас нельзя взаимодействовать
        if (!GetComponentInParent<IInteractObject>().CheckInteract())
        {
            HideAllPopups();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= innerRadius && io.CheckInteract())
        {
            print("Inner" + innerRadius);
            ShowInnerPopup();
        }
        else if(distance <= outerRadius || (distance <= innerRadius && !io.CheckInteract()))
        {
            print("Outer " + outerRadius);
            ShowOuterPopup();
        }
    }

    // ===== УПРАВЛЕНИЕ POPUP =====
    public bool CheckDistance()
    {
        if (playerInRange)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < innerRadius) return true;
        }
        return false;
    }
    private void ShowOuterPopup()
    {
        if (!outerPopup.activeSelf)
            Debug.Log("Outer popup active");

        outerPopup.SetActive(true);
        innerPopup.SetActive(false);
    }

    private void ShowInnerPopup()
    {
        if (!innerPopup.activeSelf)
            Debug.Log("Inner popup active");

        innerPopup.SetActive(true);
        outerPopup.SetActive(false);
    }

    private void HideAllPopups()
    {
        outerPopup.SetActive(false);
        innerPopup.SetActive(false);
    }

    // ===== ТВОИ МЕТОДЫ (СОХРАНЕНЫ) =====

    public void HidePopup()
    {
        HideAllPopups();
    }

    public void ShowPopup()
    {
        // не используется напрямую, логика теперь через радиус
    }

    public void UpdatePopupVisibility()
    {
        if (!playerInRange)
        {
            HideAllPopups();
            return;
        }
    }
}
