using Unity.Cinemachine;
using UnityEngine;

public class CinemachinePlayerRotation : MonoBehaviour
{
    public Transform camera;

    [Header("Rotation Settings")]
    public float moveRotateSpeed = 15f;      // при движении
    public float idleRotateSpeed = 4f;       // при простое
    public float idleDelay = 0.4f;           // через сколько сек после просто€ стартует автоповорот

    private float idleTimer = 0f;

    void Update()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 ||
                        Input.GetAxisRaw("Vertical") != 0;

        // —брасываем или увеличиваем таймер просто€
        if (isMoving)
            idleTimer = 0f;
        else
            idleTimer += Time.deltaTime;

        // ѕолучаем горизонтальное направление камеры
        Vector3 camForward = camera.forward;
        camForward.y = 0;
        camForward.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(camForward);

        if (isMoving)
        {
            // Ѕыстрый поворот при движении
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                moveRotateSpeed * Time.deltaTime
            );
        }
        else if (idleTimer >= idleDelay)
        {
            // ћедленный автоповорот при простое
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                idleRotateSpeed * Time.deltaTime
            );
        }
    }
}
