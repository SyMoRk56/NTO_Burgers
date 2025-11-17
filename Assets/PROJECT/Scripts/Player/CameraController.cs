using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Vector3 cameraOffset = new Vector3(0, 2, -4);
    public float cameraSmoothness = 5f;

    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        Cursor.lockState = CursorLockMode.Locked;

        currentY = 15f;
    }

    void LateUpdate()
    {
        if (player == null) return;

        HandleCameraRotation();
        HandleCameraPosition();
    }

    void HandleCameraRotation()
    {
        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
    }

    void HandleCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = player.position + rotation * cameraOffset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, cameraSmoothness * Time.deltaTime);

        transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }

    public void ResetCamera()
    {
        currentX = player.eulerAngles.y;
        currentY = 15f;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}