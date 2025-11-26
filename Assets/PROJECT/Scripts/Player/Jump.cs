using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Jumps : MonoBehaviour
{
    public float jumpForce = 7f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded = false;

    public playerAnimations animScript;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animScript = GetComponent<playerAnimations>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            animScript.HeroJumpAnim();
            DoJump();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // fallback
        {
            DoJump();
        }
    }

    private void DoJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false; // сразу сбросим, чтобы не прыгать в воздухе
    }

    private void OnCollisionStay(Collision collision)
    {
        // проверяем слой
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0)
            return;

        foreach (ContactPoint c in collision.contacts)
        {
            if (c.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            isGrounded = false;
        }
    }
}
