using UnityEngine;

public class RotateToPlayer : MonoBehaviour
{
    GameObject obj;
    private void Start()
    {
        obj = GameManager.Instance.GetPlayer().transform.GetChild(0).gameObject;
    }
    private void Update()
    {
        Vector3 dir = transform.position - obj.transform.position;
        Vector3 rot = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * 1000, 0f);

        transform.rotation = Quaternion.LookRotation(rot);
    }
}
