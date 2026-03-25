using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SlideInteract : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }
    [Header("Slide")]
    public Transform slideStartPoint;

    [Header("Movement")]
    public float slideSpeed = 14f;
    public float turnSpeed = 120f;
    public float slowForwardSpeed = 3f;

    [Header("Timer")]
    public float prepPenaltyTime = 5f;

    private Rigidbody rb;
    private PlayerMovement movement;
    private PlayerManager manager;

    private bool isSliding = false;
    private bool insideStart = false;
    private bool usedOnce = false;

    private float slideTimer = 0f;
    private float lastY;

    public Canvas timerCanvas;
    public TMP_Text secondomer;
    private HashSet<GameObject> usedPrepObjects = new HashSet<GameObject>();

    // ======================================================
    void Start()
    {
        Debug.Log("🟢 SlideInteract.Start()");

        manager = PlayerManager.instance;
        movement = manager.playerMovement;
        rb = movement.GetComponent<Rigidbody>();

    }

    // ======================================================
    // INTERACT
    // ======================================================
    public int InteractPriority() => 10;

    public bool CheckInteract()
    {
        return insideStart && !isSliding && !usedOnce;
    }

    public void Interact()
    {
        Debug.Log("🔥 Interact() - Активация спуска!");
        StartSlide();
    }

    public void OnBeginInteract() { }
    public void OnEndInteract(bool success) { }

    // ======================================================
    // SLIDE
    // ======================================================
    void StartSlide()
    {
        Debug.Log("▶ SLIDE START");

        isSliding = true;
        usedOnce = true;

        slideTimer = 0f;
        usedPrepObjects.Clear();

        movement.enabled = false;

        if (rb != null)
        {
            rb.position = slideStartPoint.position;
            rb.rotation = slideStartPoint.rotation;
            rb.linearVelocity = Vector3.zero;
            lastY = rb.position.y;
        }
    }

    void StopSlide()
    {
        Debug.Log("■ SLIDE END");

        isSliding = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        movement.enabled = true;
    }

    // ======================================================
    // UPDATE
    // ======================================================
    void Update()
    {
        if (!isSliding) return;

        slideTimer += Time.deltaTime;

        // Здесь можно добавить логику для обновления таймера
        // через другой UI-менеджер, если нужно
        // Debug.Log($"Таймер: {slideTimer:F2}");
    }

    // ======================================================
    // MOVEMENT
    // ======================================================
    void FixedUpdate()
    {
        if (!isSliding || rb == null) return;

        float h = Input.GetAxis("Horizontal");
        movement.transform.Rotate(Vector3.up, h * turnSpeed * Time.fixedDeltaTime);

        if (!Physics.Raycast(rb.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(
            movement.transform.forward,
            hit.normal
        ).normalized;

        float deltaY = rb.position.y - lastY;
        float speed = slideSpeed;

        if (deltaY > 0.01f)
        {
            speed *= 0.3f;
        }
        else if (deltaY < -0.01f)
        {
            speed *= 1.5f;
        }

        if (rb.linearVelocity.magnitude < 5f && Input.GetKey(KeyCode.W))
        {
            speed = slowForwardSpeed;
        }

        rb.linearVelocity = forward * speed;
        lastY = rb.position.y;
    }

    // ======================================================
    // TRIGGERS
    // ======================================================
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🚪 ENTER {other.name} tag={other.tag}");

        if (other.CompareTag("SlideStart"))
        {
            insideStart = true;
            Debug.Log("✅ SlideStart ENTER");
            return;
        }

        if (isSliding && other.CompareTag("prep"))
        {
            if (usedPrepObjects.Contains(other.gameObject))
            {
                return;
            }

            usedPrepObjects.Add(other.gameObject);
            slideTimer += prepPenaltyTime;

            Debug.Log($"⛔ PREP HIT +{prepPenaltyTime}");
        }

        if (isSliding && other.CompareTag("SlideEnd"))
        {
            Debug.Log("🏁 SlideEnd");
            StopSlide();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SlideStart"))
        {
            insideStart = false;
            usedOnce = false;
            Debug.Log("🔓 SlideStart EXIT");
        }
    }
}