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

    public playerAnimations animScript;

    public float idleTimeThreshold = 60f; // 1 минута
    public ParticleSystem idleVFX; // Ссылка на VFX компонент
    public Transform vfxSpawnPoint; // Точка появления VFX (опционально)

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

    public PlayerManager manager;

    public Transform forwardVector;

    // Переменные для отслеживания бездействия
    private float idleTimer = 0f;
    private bool isIdleVFXActive = false;
    private Vector3 lastPosition;
    private Vector2 lastLookInput;

    public AudioSource step, jump;

    [Header("Footstep Sounds")]
    public AudioClip[] grassSteps;
    public AudioClip[] woodSteps;
    public AudioClip[] stoneSteps;

    public float stepInterval = 0.45f; // задержка между шагами
    private float stepTimer = 0f;

    private Terrain terrain;
    private TerrainData terrainData;
    public AudioClip defaultStepSound;

    [Header("Состояние переноски")]
    public bool isCarrying = false; // Глобальная переменная для отслеживания переноски

    void Start()
    {
        terrain = Terrain.activeTerrain;
        if (terrain != null)
            terrainData = terrain.terrainData;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animScript = GetComponent<playerAnimations>();

        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Инициализация отслеживания бездействия
        lastPosition = transform.position;
        lastLookInput = Vector2.zero;

        // Выключаем VFX на старте
        if (idleVFX != null && idleVFX.isPlaying)
            idleVFX.Stop();
    }

    void Update()
    {
        if (!manager.CanMove)
        {
            animScript.HeroIdleAnim(isCarrying);
            ResetIdleTimer();
            return;
        }

        GetInput();
        HandleLook();
        UpdateIdleTimer();

        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
            ResetIdleTimer(); // Сброс таймера при прыжке
        }
    }

    private void OnDisable()
    {
        animScript.HeroIdleAnim(isCarrying);
    }

    void FixedUpdate()
    {
        if (!manager.CanMove || !GameManager.Instance.isGameGoing)
        {
            ResetIdleTimer();
            return;
        }

        CheckGrounded();
        HandleMovement();
        ApplyFriction();

        if (jumpRequested && isGrounded)
        {
            ExecuteJump();
            // Обновляем анимацию прыжка с учетом переноски
            if (animScript != null)
            {
                animScript.HeroJumpAnim(isCarrying);
            }
        }
        jumpRequested = false;
    }

    void GetInput()
    {
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed)
        {
            moveInput.y += 1;
            animScript.HeroWalkAnim(isCarrying);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            moveInput.y -= 1;
            animScript.HeroWalkAnim(isCarrying);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            moveInput.x -= 1;
            animScript.HeroWalkAnim(isCarrying);
        }

        if (Keyboard.current.dKey.isPressed)
        {
            moveInput.x += 1;
            animScript.HeroWalkAnim(isCarrying);
        }

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        isRunning = Keyboard.current.leftShiftKey.isPressed;

        lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;

        // Если не двигаемся и не прыгаем - idle анимация
        if (moveInput.magnitude < 0.1f && isGrounded && !jumpRequested)
        {
            animScript.HeroIdleAnim(isCarrying);
        }
    }

    void HandleLook()
    {
        //xRotation -= lookInput.y;
        //xRotation = Mathf.Clamp(xRotation, -maxViewAngle, maxViewAngle);

        //cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //transform.Rotate(Vector3.up * lookInput.x);
    }

    void HandleMovement()
    {
        Vector3 wishDir = (forwardVector.forward * moveInput.y + forwardVector.right * moveInput.x).normalized;

        float targetSpeed = GetTargetSpeed();
        targetVelocity = wishDir * targetSpeed;

        float acceleration = isGrounded ? groundAcceleration : airAcceleration;

        float massFactor = Mathf.Clamp(100f / mass, 0.5f, 2f);
        acceleration *= massFactor;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        currentVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = currentVelocity;

        animScript.anim.SetFloat("MoveSpeed", !isRunning ? 1 : runSpeed / walkSpeed);
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
        jump.PlayScheduled(.5f);
    }

    float GetTargetSpeed()
    {
        float baseSpeed = walkSpeed;

        float massSpeedFactor = Mathf.Clamp(70f / mass, 0.7f, 1.3f);

        if (isRunning)
            baseSpeed = runSpeed;

        return baseSpeed * massSpeedFactor;
    }

    public void PlayStepSound()
    {
        if (!isGrounded)
            return;

        // Проверка поверхности под ногами
        if (TryGetGroundCollider(out Collider ground))
        {
            // Если это Terrain
            if (ground is TerrainCollider)
            {
                AudioClip clip = GetFootstepSound();
                if (clip != null)
                {
                    step.PlayOneShot(clip, 1f);
                    return;
                }
            }
        }

        // Если НЕ террейн — играем другой звук
        if (defaultStepSound != null)
            step.PlayOneShot(defaultStepSound, 1f);
    }

    private bool TryGetGroundCollider(out Collider col)
    {
        col = null;

        float rayLength = 2 * 0.6f + 0.3f;
        Vector3 start = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayLength))
        {
            col = hit.collider;
            return true;
        }

        return false;
    }

    private AudioClip GetFootstepSound()
    {
        int tex = GetMainTexture(transform.position);

        AudioClip[] targetArray = null;

        switch (tex)
        {
            case 1: // Sand / Wood
                targetArray = woodSteps;
                break;

            case 2: // Grass
                targetArray = grassSteps;
                break;

            case 3: // Stone
                targetArray = stoneSteps;
                break;

            // 0 — пустой террейн: можно возвращать null или подставлять дефолтный звук
            case 0:
            default:
                return null;
        }

        if (targetArray == null || targetArray.Length == 0)
            return null;

        return targetArray[Random.Range(0, targetArray.Length)];
    }

    private int GetMainTexture(Vector3 worldPos)
    {
        if (terrainData == null) return 0;

        Vector3 terrainPos = worldPos - terrain.transform.position;

        float x = terrainPos.x / terrainData.size.x;
        float z = terrainPos.z / terrainData.size.z;

        int mapX = Mathf.Clamp((int)(x * terrainData.alphamapWidth), 0, terrainData.alphamapWidth - 1);
        int mapZ = Mathf.Clamp((int)(z * terrainData.alphamapHeight), 0, terrainData.alphamapHeight - 1);

        float[,,] splatmap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        int best = 0;
        float max = 0f;

        for (int i = 0; i < splatmap.GetLength(2); i++)
        {
            if (splatmap[0, 0, i] > max)
            {
                best = i;
                max = splatmap[0, 0, i];
            }
        }

        return best;
    }

    // Методы для отслеживания бездействия и управления VFX
    void UpdateIdleTimer()
    {
        // Проверяем движение и вращение
        bool isMoving = moveInput != Vector2.zero ||
                       transform.position != lastPosition ||
                       lookInput != Vector2.zero;

        if (isMoving)
        {
            ResetIdleTimer();
        }
        else
        {
            idleTimer += Time.deltaTime;

            animScript.HeroIdleAnim(isCarrying);

            if (idleTimer >= idleTimeThreshold && !isIdleVFXActive)
            {
                ActivateIdleVFX();
            }
        }

        // Сохраняем текущие значения для следующего кадра
        lastPosition = transform.position;
        lastLookInput = lookInput;
    }

    void ResetIdleTimer()
    {
        idleTimer = 0f;

        if (isIdleVFXActive)
        {
            DeactivateIdleVFX();
        }
    }

    void ActivateIdleVFX()
    {
        if (idleVFX != null)
        {
            // Устанавливаем позицию VFX если указана точка спавна
            if (vfxSpawnPoint != null)
            {
                idleVFX.transform.position = vfxSpawnPoint.position;
            }
            else
            {
                idleVFX.transform.position = transform.position + Vector3.up * 0.5f;
            }

            idleVFX.Play();
            isIdleVFXActive = true;

            Debug.Log("Idle VFX activated - player has been inactive for " + idleTimeThreshold + " seconds");
        }
    }

    void DeactivateIdleVFX()
    {
        if (idleVFX != null && idleVFX.isPlaying)
        {
            idleVFX.Stop();
            isIdleVFXActive = false;
        }
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
            float rayLength = col.height * 0.6f + 0.2f;
            Vector3 rayStart = transform.position + Vector3.up * (col.height * 0.5f - col.radius);
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayLength);

            Vector3 offset = transform.right * col.radius * 0.8f;
            Gizmos.DrawLine(rayStart + offset, (rayStart + offset) + Vector3.down * rayLength);
            Gizmos.DrawLine(rayStart - offset, (rayStart - offset) + Vector3.down * rayLength);
        }
    }
}