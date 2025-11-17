using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 15f;
    public float maxBackwardSpeed = 5f;
    public float acceleration = 2f;
    public float deceleration = 3f;

    [Header("Steering Settings")]
    public float movingSteeringForce = 2f;
    public float stationarySteeringForce = 5f;
    public float driftFactor = 0.5f;

    [Header("Engine Settings")]
    public float enginePower = 50f;
    public float reversePower = 20f;

    [Header("References")]
    public Transform centerOfMass;
    public Transform[] thrustPoints;

    private Rigidbody rb;
    private Vector2 input;
    private bool isInWater = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (centerOfMass != null)
            rb.centerOfMass = centerOfMass.localPosition;
    }

    void Update()
    {
        GetInput();
    }

    void GetInput()
    {
        input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y += 1;
        if (Keyboard.current.sKey.isPressed) input.y -= 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;

        input = Vector2.ClampMagnitude(input, 1f);
    }

    void FixedUpdate()
    {
        if (!isInWater) return;

        ApplyMovement();
        ApplySteering();
        ApplyWaterPhysics();
    }

    void ApplyMovement()
    {
        if (Mathf.Abs(input.y) > 0.1f)
        {
            float power = input.y > 0 ? enginePower : reversePower;
            Vector3 forceDirection = transform.forward * input.y;

            foreach (Transform thrustPoint in thrustPoints)
            {
                rb.AddForceAtPosition(forceDirection * power * Time.fixedDeltaTime,
                                    thrustPoint.position, ForceMode.Acceleration);
            }
        }
    }

    void ApplySteering()
    {
        if (Mathf.Abs(input.x) > 0.1f)
        {
            float currentVelocity = rb.linearVelocity.magnitude;
            float steeringForce;

            if (currentVelocity < 1f)
            {
                steeringForce = stationarySteeringForce * input.x;
                rb.AddTorque(0, steeringForce, 0, ForceMode.Acceleration);
            }
            else
            {
                steeringForce = movingSteeringForce * input.x * currentVelocity * 0.1f;
                rb.AddTorque(0, steeringForce, 0, ForceMode.Acceleration);

                if (Mathf.Abs(input.y) > 0.1f)
                {
                    Vector3 driftForce = transform.right * (input.x * driftFactor * currentVelocity * 0.3f);
                    rb.AddForce(driftForce, ForceMode.Acceleration);
                }
            }
        }
    }

    void ApplyWaterPhysics()
    {
        Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized,
            Vector3.up
        );

        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 2f);

        if (input.y < -0.1f)
        {
            Vector3 resistance = -rb.linearVelocity * 0.3f;
            rb.AddForce(resistance, ForceMode.Acceleration);
        }
    }

    void OnDrawGizmos()
    {
        if (thrustPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in thrustPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.2f);
                    Gizmos.DrawRay(point.position, transform.forward * 1f);
                }
            }
        }

        if (centerOfMass != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(centerOfMass.position, 0.3f);
        }
    }
}