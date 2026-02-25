using Unity.Cinemachine;
using UnityEngine;

public class CameraSensitivity : MonoBehaviour
{
    public CinemachineInputAxisController freeLookCamera;

    void Update()
    {
        foreach (var c in freeLookCamera.Controllers)
        {
            float sign = Mathf.Sign(c.Input.Gain); // сохраняем знак
            float newSensitivity = (SettingsSaveSystem.ssensitivity + 0.1f) * 1.5f;

            c.Input.Gain = newSensitivity * sign; // возвращаем знак
        }
    }
}
