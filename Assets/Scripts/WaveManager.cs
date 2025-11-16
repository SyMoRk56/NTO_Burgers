using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    public float amplitude = 1f;  
    public float wavelength = 2f; 
    public float speed = 1f;      
    private float frequency;       

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        frequency = 2 * Mathf.PI / wavelength;
    }

    public float GetWaveHeight(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        float waveX = Mathf.Sin(x * frequency + Time.time * speed) * amplitude;
        float waveZ = Mathf.Sin(z * frequency * 0.8f + Time.time * speed * 0.7f) * (amplitude * 0.5f);

        return waveX + waveZ;
    }
}
