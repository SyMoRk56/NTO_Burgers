using UnityEngine;

public class CinemachinePlayerRotation : MonoBehaviour
{
    public GameObject camera;
    public Transform forward;

    private void Update()
    {
        forward.rotation = Quaternion.Euler(new Vector3(0, camera.transform.eulerAngles.y, 0));
    }
}
