using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractObject
{
    [SerializeField] public Vector3 positionOffset, rotationOffset;
    bool parented = false;
    public bool CheckInteract()
    {
        print("PickupItem check interact");
        SetParent(false);
        return true;
    }

    public void Interact()
    {
        SetParent(!parented);
    }

    private void SetParent(bool parented)
    {
        this.parented = parented;
        transform.SetParent(!parented ? null : PlayerManager.instance.hand);
        if (parented)
        {
            transform.localRotation = Quaternion.Euler(rotationOffset);
            transform.localPosition = positionOffset;
        }
       
        GetComponent<Collider>().enabled = !parented;
        GetComponent<Rigidbody>().isKinematic = parented;
    }

    public int InteractPriority()
    {
        return parented ? 30 : 0;
    }

    public void OnBeginInteract()
    {
    }

    public void OnEndInteract(bool success)
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
