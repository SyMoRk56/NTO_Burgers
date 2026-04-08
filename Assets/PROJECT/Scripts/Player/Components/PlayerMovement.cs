using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f; // Скорость ходьбы
    public float runSpeed = 8f;  // Скорость бега
    public float jumpForce = 7f; // Сила прыжка
    public float mass = 70f;     // Масса игрока
    public float groundAcceleration = 50f; // Ускорение на земле
    public float airAcceleration = 20f;    // Ускорение в воздухе
    public float friction = 8f;  // Трение
    public float airControl = 0.3f; // Контроль в воздухе
    public float mouseSensitivity = 2f; // Чувствительность мыши
    public Transform cameraTransform;
    public float maxViewAngle = 85f;
    public LayerMask groundLayer;

    public PlayerAnimations animScript;

    public float idleTimeThreshold = 60f; // Время до idle VFX
    public ParticleSystem idleVFX;
    public Transform vfxSpawnPoint;

    private Rigidbody rb;
    private CapsuleCollider col;
    public Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    public bool isGrounded = false;
    public bool isRunning = false;
    private bool jumpRequested = false;

    public Vector3 targetVelocity;
    public Vector3 currentVelocity;
    public bool tutorialWalkCompleted = false;

    public PlayerManager manager;

    public Transform forwardVector; // Направление движения

    private float idleTimer = 0f;
    private bool isIdleVFXActive = false;
    private Vector3 lastPosition;
    private Vector2 lastLookInput;

    public AudioSource step, jump;

    [Header("Footstep Sounds")]
    public AudioClip[] grassSteps;
    public AudioClip[] woodSteps;
    public AudioClip[] stoneSteps;

    public float stepInterval = 0.45f;
    private float stepTimer = 0f;

    public Terrain terrain;
    private TerrainData terrainData;
    public AudioClip defaultStepSound;

    [Header("Состояние переноски")]
    public bool isCarrying = false;

    [Header("Рыбалка")]
    public bool isFishing = false;

    public PhysicsMaterial mat;

    public float animSpeed = 1;

    void Start()
    {
        terrainData = terrain.terrainData; // Данные террейна

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        animScript = GetComponent<PlayerAnimations>();

        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastPosition = transform.position;
        lastLookInput = Vector2.zero;

        if (idleVFX != null && idleVFX.isPlaying)
            idleVFX.Stop();

    }

    void Update()
    {
        HandleLook(); // Обработка мыши

        if (!manager.CanMove)
        {
            if (!isFishing)
            {
                animScript.HeroIdleAnim(isCarrying);
            }

            ResetIdleTimer();

            moveInput = Vector2.zero;
            targetVelocity = Vector3.zero;
            currentVelocity = Vector3.zero;

            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            if (!isFishing) return;
            return;
        }

        GetInput(); // Получаем ввод

        // Туториал: первый шаг
        if (!tutorialWalkCompleted && moveInput.magnitude > 0)
        {
            if (PlayerMailInventory.Instance != null &&
                PlayerMailInventory.Instance.carriedMails.Count > 0 &&
                PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_0")
            {
                TaskManager.Instance.RemoveTask("Tutorial_0");
                PlayerMailInventory.Instance.RemoveFirstMail();
                tutorialWalkCompleted = true;
            }
        }

        UpdateIdleTimer(); // idle логика

        if (manager.CanMove)
            if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
                ResetIdleTimer();
            }
    }

    private void OnDisable()
    {
        if (!isFishing)
        {
            animScript.HeroIdleAnim(isCarrying);
        }
    }

    void FixedUpdate()
    {
        // Блок движения
        if ((!manager.CanMove || !GameManager.Instance.isGameGoing) && !isFishing)
        {
            ResetIdleTimer();
            return;
        }

        CheckGrounded();   // Проверка земли
        HandleMovement();  // Движение
        ApplyFriction();   // Трение

        if (jumpRequested && isGrounded)
        {
            ExecuteJump();

            if (animScript != null && !isFishing)
            {
                animScript.HeroJumpAnim(isCarrying);
            }
        }

        jumpRequested = false;
    }

    void GetInput()
    {
        if (isFishing)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = Vector2.zero;

        // WASD ввод
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

        // Настройка трения
        mat.staticFriction = 0;
        if (Mathf.Abs(moveInput.x) + Mathf.Abs(moveInput.y) == 0 && isGrounded)
        {
            mat.staticFriction = .34f;
        }

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        isRunning = Keyboard.current.leftShiftKey.isPressed;

        lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.1f;

        if (moveInput.magnitude < 0.1f && isGrounded && !jumpRequested && !isFishing)
        {
            animScript.HeroIdleAnim(isCarrying);
        }
    }

    void HandleLook()
    {
    }

    void HandleMovement()
    {
        if (isFishing)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // Направление движения относительно камеры
        Vector3 wishDir = (forwardVector.forward * moveInput.y + forwardVector.right * moveInput.x).normalized;

        float targetSpeed = GetTargetSpeed();
        targetVelocity = wishDir * targetSpeed;

        float acceleration = isGrounded ? groundAcceleration : airAcceleration;

        float massFactor = Mathf.Clamp(100f / mass, 0.5f, 2f);
        acceleration *= massFactor;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        currentVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = currentVelocity;

        animScript.anim.SetFloat("MoveSpeed", (!isRunning ? 1 : runSpeed / walkSpeed) * animSpeed);
    }

    void ApplyFriction()
    {
        if (isGrounded && moveInput.magnitude < 0.1f && !isFishing)
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

        // Основной рейкаст вниз
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

        jump.PlayScheduled(.5f); // Звук прыжка
    }

    float GetTargetSpeed()
    {
        float baseSpeed = walkSpeed;

        float massSpeedFactor = Mathf.Clamp(70f / mass, 0.7f, 1.3f);

        if (isRunning)
            baseSpeed = runSpeed;

        return baseSpeed * massSpeedFactor;
    }

    public void StartFishing()
    {
        isFishing = true;
        isCarrying = true;
    }

    public void EndFishing()
    {
        isFishing = false;
        isCarrying = false;
        animScript.EndFishing();
    }

    public void PlayStepSound()
    {
        if (!isGrounded)
            return;

        if (TryGetGroundCollider(out Collider ground))
        {
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

        // Выбор звука по типу поверхности
        switch (tex)
        {
            case 1:
                targetArray = woodSteps;
                break;
            case 2:
                targetArray = grassSteps;
                break;
            case 3:
                targetArray = stoneSteps;
                break;
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

        // Определяем доминирующую текстуру
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

    void UpdateIdleTimer()
    {
        if (isFishing)
        {
            ResetIdleTimer();
            return;
        }

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
                ActivateIdleVFX(); // Запуск VFX
            }
        }

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