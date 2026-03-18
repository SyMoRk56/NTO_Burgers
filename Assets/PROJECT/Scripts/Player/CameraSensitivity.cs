using Unity.Cinemachine;
using UnityEngine;

public class CameraSensitivity : MonoBehaviour
{
    public CinemachineInputAxisController freeLookCamera;
    void Update()
    {
        foreach (var c in freeLookCamera.Controllers)
        {
            c.Input.Gain = (SettingsSaveSystem.ssensitivity + .1f) * 1.5f;
        }
    }
}
