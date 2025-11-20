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
    public LayerMask groundLayer;

    // AFK System
    [Header("AFK System")]
    public float afkTimeThreshold = 5f;
    public ParticleSystem afkParticles;
    public AudioSource afkAudio;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    public bool isGrounded = false;
    public bool isRunning = false;
    private bool jumpRequested = false;

    private Vector3 targetVelocity;
    private Vector3 currentVelocity;

    // AFK variables
    private float afkTimer = 0f;
    private bool isAfk = false;
    private Vector3 lastPosition;
    private Vector2 lastLookInput;

    public PlayerManager manager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // AFK initialization
        lastPosition = transform.position;
        lastLookInput = Vector2.zero;

        // Ensure particles are stopped at start and set correct position
        if (afkParticles != null)
        {
            // Set local position to (0, 2, 0) relative to player
            afkParticles.transform.localPosition = new Vector3(0f, 2f, 0f);
            afkParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        if (!manager.CanMove)
        {
            // If movement is disabled, turn off AFK
            if (isAfk) StopAFK();
            return;
        }

        GetInput();
        HandleLook();
        HandleAFK();

        // Save current values for next frame
        lastPosition = transform.position;
        lastLookInput = lookInput;

        // Jump request handling - moved from original position to ensure it's always checked
        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (!manager.CanMove) return;

        CheckGrounded();
        HandleMovement();
        ApplyFriction();

        // Execute jump in FixedUpdate for physics consistency
        if (jumpRequested && isGrounded)
        {
            ExecuteJump();
        }
        jumpRequested = false;
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

    void HandleAFK()
    {
        // Check movement and input more precisely
        bool isMoving = (Vector3.Distance(transform.position, lastPosition) > 0.001f);
        bool isLooking = (lookInput != Vector2.zero);
        bool hasKeyboardInput = moveInput != Vector2.zero;
        bool anyKeyPressed = Keyboard.current.anyKey.isPressed ||
                           Mouse.current.leftButton.isPressed ||
                           Mouse.current.rightButton.isPressed ||
                           Mouse.current.delta.ReadValue() != Vector2.zero;

        bool hasInput = isMoving || isLooking || hasKeyboardInput || anyKeyPressed;

        if (hasInput)
        {
            afkTimer = 0f;
            if (isAfk)
            {
                StopAFK();
            }
        }
        else
        {
            afkTimer += Time.deltaTime;

            if (afkTimer >= afkTimeThreshold && !isAfk)
            {
                StartAFK();
            }
        }

        // Debug information (can remove after testing)
        if (Time.frameCount % 60 == 0) // Output once per second
        {
            Debug.Log($"AFK Timer: {afkTimer:F1}, IsAFK: {isAfk}, HasInput: {hasInput}");
        }
    }

    void StartAFK()
    {
        isAfk = true;

        if (afkParticles != null)
        {
            // Ensure particles are at correct position relative to player
            afkParticles.transform.localPosition = new Vector3(0f, 2f, 1.5f);
            afkParticles.Play();
            Debug.Log("AFK VFX started at local position: " + afkParticles.transform.localPosition);
        }
        else
        {
            Debug.LogError("AFK Particles reference is null!");
        }

        if (afkAudio != null)
        {
            afkAudio.Play();
        }

        Debug.Log("Player went AFK");
    }

    void StopAFK()
    {
        isAfk = false;

        if (afkParticles != null)
        {
            afkParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (afkAudio != null)
        {
            afkAudio.Stop();
        }

        Debug.Log("Player woke up");
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
        float rayLength = col.height * 0.6f + 0.2f;
        Vector3 rayStart = transform.position + Vector3.up * (col.height * 0.5f - col.radius);

        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayLength, groundLayer);

        if (!isGrounded)
        {
            Vector3 offset = transform.right * col.radius * 0.8f;
            isGrounded = Physics.Raycast(rayStart + offset, Vector3.down, rayLength, groundLayer) ||
                        Physics.Raycast(rayStart - offset, Vector3.down, rayLength, groundLayer);
        }

        if (!isGrounded)
        {
            RaycastHit hit;
            if (Physics.SphereCast(rayStart, col.radius * 0.9f, Vector3.down, out hit, rayLength, groundLayer))
            {
                isGrounded = true;
            }
        }
    }

    void ExecuteJump()
    {
        float jumpPower = jumpForce * Mathf.Sqrt(mass / 70f);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
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

    // AFK status property (might be useful)
    public bool IsAfk => isAfk;

    void OnDrawGizmosSelected()
    {
        if (col != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            float rayLength = col.height * 0.6f + 0.2f;
            Vector3 rayStart = transform.position + Vector3.up * (col.height * 0.5f - col.radius);
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayLength);

            Vector3 offset = transform.right * col.radius * 0.8f;
            Gizmos.DrawLine(rayStart + offset, (rayStart + offset) + Vector3.down * rayLength);
            Gizmos.DrawLine(rayStart - offset, (rayStart - offset) + Vector3.down * rayLength);
        }
    }
}