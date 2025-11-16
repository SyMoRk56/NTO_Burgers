using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    public float mass = 70f;
    public float groundAcceleration = 50f;
    public float airAcceleration = 20f;
    public float friction = 8f;
    public float airControl = 0.3f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public float maxViewAngle = 85f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    private bool isGrounded = false;
    private bool isRunning = false;

    private Vector3 targetVelocity;
    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.mass = mass;

        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        GetInput();
        HandleLook();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        ApplyFriction();
    }

    void GetInput()
    {
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        isRunning = Keyboard.current.leftShiftKey.isPressed;

        lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;
    }

    void HandleLook()
    {
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxViewAngle, maxViewAngle);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookInput.x);
    }

    void HandleMovement()
    {
        Vector3 wishDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        float targetSpeed = GetTargetSpeed();
        targetVelocity = wishDir * targetSpeed;

        float acceleration = isGrounded ? groundAcceleration : airAcceleration;

        float massFactor = Mathf.Clamp(100f / mass, 0.5f, 2f);
        acceleration *= massFactor;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        currentVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = currentVelocity;

        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            float jumpPower = jumpForce * Mathf.Sqrt(mass / 70f);
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void ApplyFriction()
    {
        if (isGrounded && moveInput.magnitude < 0.1f)
        {
            Vector3 frictionVelocity = rb.linearVelocity;
            frictionVelocity.x *= 1f - friction * Time.fixedDeltaTime;
            frictionVelocity.z *= 1f - friction * Time.fixedDeltaTime;
            rb.linearVelocity = frictionVelocity;
        }
    }

    void CheckGrounded()
    {
        float rayLength = col.height / 2f + 0.1f;
        Vector3 rayStart = transform.position + col.center;

        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength);

        if (!isGrounded)
        {
            RaycastHit hit;
            if (Physics.SphereCast(rayStart, col.radius * 0.9f, Vector3.down, out hit, rayLength))
            {
                isGrounded = true;
            }
        }
    }

    float GetTargetSpeed()
    {
        float baseSpeed = walkSpeed;

        float massSpeedFactor = Mathf.Clamp(70f / mass, 0.7f, 1.3f);

        if (isRunning)
            baseSpeed = runSpeed;

        return baseSpeed * massSpeedFactor;
    }

    public void SetMass(float newMass)
    {
        mass = Mathf.Max(newMass, 1f); 
        rb.mass = mass;
    }

    public float GetCurrentSpeed()
    {
        return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
    }

    void OnDrawGizmosSelected()
    {
        if (col != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position + col.center;
            Gizmos.DrawRay(rayStart, Vector3.down * (col.height / 2f + 0.1f));
        }
    }
}