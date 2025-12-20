using UnityEngine;

public class Bobber : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public float buoyancyForce = 1f;
    public float waveFrequency = 1f;
    public float waveAmplitude = 0.1f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Базовая плавучесть
        float wave = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rb.AddForce(Vector3.up * (buoyancyForce + wave), ForceMode.Acceleration);

        // Случайные колебания для клёва
        if (Random.Range(0, 100) < 30)
        {
            rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }
    }

    public float GetMovementMagnitude()
    {
        return rb.linearVelocity.magnitude;
    }
}