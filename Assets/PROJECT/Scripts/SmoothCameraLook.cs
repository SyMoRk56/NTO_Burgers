using UnityEngine;
using System.Collections;

public class SmoothCameraLook : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public float rotationSpeed = 5f;

    private Vector3 velocity = Vector3.zero;
    private Quaternion targetRotation;

    void Update()
    {
        if (target != null)
        {
            // Плавное перемещение позиции
            transform.position = Vector3.SmoothDamp(transform.position,
                target.position, ref velocity, smoothTime);

            // Плавное вращение к цели
            Vector3 direction = target.position - transform.position;
            if (direction != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}