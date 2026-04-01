using Unity.Cinemachine;
using UnityEngine;

public class CameraSensitivity : MonoBehaviour
{
    public CinemachineInputAxisController freeLookCamera;
    void Update()
    {
        int i = 0;
        foreach (var c in freeLookCamera.Controllers)
        {
            c.Input.Gain = (SettingsSaveSystem.ssensitivity + .1f) * 1.5f;
            if(i == 1)
            {
                c.Input.Gain *= -1;
            }
            i++;
            
        }
    }
}
