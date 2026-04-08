using UnityEngine;

public class Freecam : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float fastSpeedMultiplier = 3f;

    [Header("Look")]
    public float lookSensitivity = 2f;
    public bool lockCursor = true;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        yaw = rot.y;
        pitch = rot.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= fastSpeedMultiplier;

        float x = Input.GetAxis("Horizontal");     // A/D
        float z = Input.GetAxis("Vertical");       // W/S
        float y = 0;

        if (Input.GetKey(KeyCode.E)) y += 1;       // Up
        if (Input.GetKey(KeyCode.Q)) y -= 1;       // Down

        Vector3 direction = transform.TransformDirection(new Vector3(x, y, z));
        transform.position += direction * speed * Time.deltaTime;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
