using Unity.Cinemachine;
using UnityEngine;

public class CinemachinePlayerRotation : MonoBehaviour
{
    public Transform camera;

    [Header("Rotation Settings")]
    public float moveRotateSpeed = 15f;   
    public float idleRotateSpeed = 4f;      
    public float idleDelay = 0.4f;          

    private float idleTimer = 0f;

    void Update()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 ||
                        Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
            idleTimer = 0f;
        else
            idleTimer += Time.deltaTime;

        Vector3 camForward = camera.forward;
        camForward.y = 0;
        camForward.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(camForward);

        if (isMoving)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRot, moveRotateSpeed * Time.deltaTime);
        }
        else if (idleTimer >= idleDelay)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRot,idleRotateSpeed * Time.deltaTime);
        }
    }
}
