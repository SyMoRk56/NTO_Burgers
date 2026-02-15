using UnityEngine;

public class IslandTransfer : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }
    [Header("Teleport settings")]
    public float interactRadius = 2f;

    private Transform mainIslandPoint;
    private Transform secondIslandPoint;

    private Transform player;

    private bool playerInside = false;

    void Start()
    {
        player = PlayerManager.instance.transform;

        GameObject main = GameObject.FindGameObjectWithTag("main_island");
        GameObject second = GameObject.FindGameObjectWithTag("second_island");

        if (main != null) mainIslandPoint = main.transform;
        if (second != null) secondIslandPoint = second.transform;
    }

    // ====================================================
    // IInteractObject
    // ====================================================

    public int InteractPriority()
    {
        return 5;
    }

    public bool CheckInteract()
    {
        return playerInside;
    }

    public void Interact()
    {
        Teleport();
    }

    public void OnBeginInteract() { }

    public void OnEndInteract(bool success) { }

    // ====================================================
    // Trigger
    // ====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    // ====================================================
    // Teleport logic
    // ====================================================

    void Teleport()
    {
        if (player == null) return;

        float distToMain = Vector3.Distance(player.position, mainIslandPoint.position);
        float distToSecond = Vector3.Distance(player.position, secondIslandPoint.position);

        Transform target =
            distToMain < distToSecond
                ? secondIslandPoint
                : mainIslandPoint;

        CharacterTeleport(target.position, target.rotation);
    }

    void CharacterTeleport(Vector3 pos, Quaternion rot)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        rb.rotation = rot;
    }
}
