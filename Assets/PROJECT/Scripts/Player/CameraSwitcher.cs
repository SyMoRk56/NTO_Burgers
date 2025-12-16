using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher Instance;
    [Header("References")]
    public Camera firstPersonCamera;
    public CinemachineCamera thirdPersonCamera;
    public CinemachinePlayerRotation rot;
    public SkinnedMeshRenderer renderer;

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

    public void SetCameraMode(bool firstPerson)
    {
        renderer.enabled = !firstPerson;
        if (firstPerson)
        {
            firstPersonCamera.gameObject.SetActive(true);
            thirdPersonCamera.Priority = 0;
            rot.enabled = false;
        }
        else
        {
            firstPersonCamera.gameObject.SetActive(false);
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