using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher Instance;
    [Header("References")]
    public Camera firstPersonCamera;
    public CinemachineCamera thirdPersonCamera;
    public CinemachinePlayerRotation rot;

    private bool isFirstPerson = false;

    void Start()
    {
        Instance = this;
        SetCameraMode(isFirstPerson);
    }

    public void Switch()
    {
        isFirstPerson = !isFirstPerson;
        SetCameraMode(isFirstPerson);
    }

    void SetCameraMode(bool firstPerson)
    {
        if (firstPerson)
        {
            // Enable first-person camera
            firstPersonCamera.gameObject.SetActive(true);

            // Disable Cinemachine
            thirdPersonCamera.Priority = 0;
            rot.enabled = false;
        }
        else
        {
            // Disable first-person camera
            firstPersonCamera.gameObject.SetActive(false);

            // Enable Cinemachine
            thirdPersonCamera.Priority = 10;
            rot.enabled = true;
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.U))
        {
            Switch();
        }
#endif
    }
}
