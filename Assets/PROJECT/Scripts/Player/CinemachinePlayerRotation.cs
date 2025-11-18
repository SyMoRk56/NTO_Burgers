using Unity.Cinemachine;
using UnityEngine;

public class CinemachinePlayerRotation : MonoBehaviour
{
    public Transform camera;
    public Transform forward;

    private void Update()
    {
        Vector3 camForward = camera.forward;
        camForward.y = 0;
        camForward.Normalize();

        forward.rotation = Quaternion.LookRotation(camForward);
    }
}
