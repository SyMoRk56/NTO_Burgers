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

    private bool isFirstPerson = false; // текущее состояние камеры

    void Start()
    {
        Instance = this; // сохраняем ссылку
        SetCameraMode(isFirstPerson); // применяем начальный режим
    }

    public void Switch()
    {
        isFirstPerson = !isFirstPerson;
        SetCameraMode(isFirstPerson); // обновляем камеру
    }

    public void SetCameraMode(bool firstPerson)
    {
        renderer.enabled = !firstPerson; // скрываем модель в первом лице

        if (firstPerson)
        {
            firstPersonCamera.gameObject.SetActive(true); // включаем FPS камеру
            thirdPersonCamera.Priority = 0; // понижаем приоритет Cinemachine
            rot.enabled = false; // отключаем вращение игрока
        }
        else
        {
            firstPersonCamera.gameObject.SetActive(false); // выключаем FPS камеру
            thirdPersonCamera.Priority = 10; // делаем Cinemachine активной
            rot.enabled = true; // включаем вращение
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.U)) // тестовое переключение в редакторе
        {
            Switch();
        }
#endif
    }
}