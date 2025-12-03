using UnityEngine;
using Unity.Cinemachine;

public class RotateToPlayer : MonoBehaviour
{
    GameObject obj;
    private void Start()
    {
        obj = FindFirstObjectByType<CinemachineCamera>().gameObject;
    }
    private void Update()
    {
        Vector3 dir = transform.position - obj.transform.position;
        Vector3 rot = Vector3.RotateTowards(transform.forward, dir, Time.deltaTime * 1000, 0f);
        
        transform.rotation = Quaternion.LookRotation(rot);
    }
    private void FixedUpdate()
    {
        obj = !CameraSwitcher.Instance.renderer.enabled ? CameraSwitcher.Instance.firstPersonCamera.gameObject : CameraSwitcher.Instance.thirdPersonCamera.gameObject;
    }
}
