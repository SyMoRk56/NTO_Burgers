using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-1000)]
public class PlayerManager : MonoBehaviour
{
    public GameObject cinemachineCamera;

    bool canMove = true; // Флаг движения
    public static PlayerManager instance; // Синглтон

    public Transform hand; // Точка для предметов в руке

    int money, day;

    // День с автосохранением и обновлением задач
    public int Day
    {
        get { return day; }
        set
        {
            if (day < value)
            {
                day = value;
                SaveGameManager.Instance.SaveAuto(true);
                TaskManager.Instance.UpdateDailyTasks();
            }
            ;
        }
    }

    public int Money
    {
        get { return money; }
        set { money = value; }
    }

    // Управление возможностью движения
    public bool CanMove
    {
        get { return canMove; }
        set
        {
            print("Set can move: " + value);

            if (cinemachineCamera != null)
                cinemachineCamera.GetComponent<CinemachineInputAxisController>().enabled = value;

            canMove = value;

            var cc = GetComponentInChildren<CameraController>();
            if (cc != null) cc.enabled = value;
        }
    }

    private void Awake()
    {
        instance = this; // Инициализация синглтона
    }

    private void Start()
    {
        SetupComponents(); // Поиск компонентов
    }

    public void ShowCursor(bool show)
    {
        // Управление курсором
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    #region Components
    public CameraSwitcher cameraSwitcher;
    public Face playerFace;
    public PlayerAnimations playerAnimations;
    public PlayerInteraction playerInteraction;
    public PlayerMailInventory playerMailInventory;
    public PlayerMovement playerMovement;

    void SetupComponents()
    {
        // Получаем ссылки на компоненты
        cameraSwitcher = FindFirstObjectByType<CameraSwitcher>();
        playerFace = GetComponentInChildren<Face>();
        playerAnimations = GetComponentInChildren<PlayerAnimations>();
        playerInteraction = GetComponentInChildren<PlayerInteraction>();
        playerMailInventory = FindFirstObjectByType<PlayerMailInventory>();
        playerMovement = GetComponentInChildren<PlayerMovement>();

        print("Components setuped");
    }
    #endregion

    private void Update()
    {
    }

    [SerializeField] FullScreenPassRendererFeature feature;

    public void SetThunder(bool set)
    {
        print("Set Thunder");
        StartCoroutine(ThunderMaterialInOut(set)); // Запуск эффекта
    }

    IEnumerator ThunderMaterialInOut(bool IN)
    {
        var mat = feature.passMaterial;

        float start = mat.GetFloat("_FogDensity");
        float target = IN ? 1 : 0f;

        float t = 0f;

        // Плавное изменение параметра
        while (t < 3)
        {
            t += Time.deltaTime;

            float value = Mathf.Lerp(start, target, t / 3);
            mat.SetFloat("_FogDensity", value);

            print("SET " + value);

            yield return null;
        }

        mat.SetFloat("_FogDensity", target); // Финальное значение
    }
}