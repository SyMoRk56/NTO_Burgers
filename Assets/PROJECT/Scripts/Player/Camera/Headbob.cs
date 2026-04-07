using Unity.Cinemachine;
using UnityEngine;

public class Headbob : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;

    [SerializeField] private float amplitude = 0.15f;
    [SerializeField] private float frequency = 1.0f;
    [SerializeField] private float smoothTime = 0.1f;

    [SerializeField] private Rigidbody characterController;
    [SerializeField] PlayerMovement movement;
    [SerializeField] private float velocityThreshold = 0.1f;

    private CinemachineBasicMultiChannelPerlin noise;
    private float currentAmplitude;
    private float currentFrequency;
    private Vector3 initialPosition;
    private float elapsedTime;
    private float currentVelocity;

    private void Start()
    {
        
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            initialPosition = virtualCamera.transform.localPosition;
        }
    }

    private void Update()
    {
        if (noise == null || characterController == null) return;

        bool isMoving = characterController.linearVelocity.magnitude > velocityThreshold && movement.isGrounded;

        float targetAmplitude = isMoving ? amplitude * SettingsSaveSystem.Instance.GetData().shakeScale * 2 : 0f;
        float targetFrequency = isMoving ? frequency : 0f;

        currentAmplitude = Mathf.SmoothDamp(currentAmplitude, targetAmplitude * (movement.isRunning ? movement.walkSpeed / movement.runSpeed : 1), ref currentVelocity, smoothTime);
        currentFrequency = Mathf.SmoothDamp(currentFrequency, targetFrequency * (movement.isRunning ? movement.runSpeed/movement.walkSpeed : 1), ref currentVelocity, smoothTime);

        noise.AmplitudeGain = currentAmplitude;
        noise.FrequencyGain = currentFrequency;
    }
}
