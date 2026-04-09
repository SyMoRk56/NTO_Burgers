using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-1000)]
public class PlayerManager : MonoBehaviour
{
    public GameObject cinemachineCamera;

    bool canMove = true;
    public static PlayerManager instance;

    public Transform hand;

    int money, day;

    // Переменные для дистанции
    public float dist;
    private Vector3 lastPosition; // Позиция в прошлом кадре
    public int tabOpenCount;
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
        }
    }

    public int Money
    {
        get { return money; }
        set { money = value; }
    }

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
        instance = this;
    }

    private void Start()
    {
        SetupComponents();
        // Инициализируем стартовую позицию, чтобы не было скачка при старте
        lastPosition = transform.position;
    }

    public void ShowCursor(bool show)
    {
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
        CalculateDistance();
    }

    void CalculateDistance()
    {
        // Вычисляем расстояние между текущей и прошлой позицией
        float moveStep = Vector3.Distance(transform.position, lastPosition);

        // Добавляем к общему счетчику
        dist += moveStep;

        // Обновляем "прошлую" позицию для следующего кадра
        lastPosition = transform.position;
    }

    [SerializeField] FullScreenPassRendererFeature feature;

    public void SetThunder(bool set)
    {
        print("Set Thunder");
        StartCoroutine(ThunderMaterialInOut(set));
    }

    IEnumerator ThunderMaterialInOut(bool IN)
    {
        var mat = feature.passMaterial;
        float start = mat.GetFloat("_FogDensity");
        float target = IN ? 1 : 0f;
        float t = 0f;

        while (t < 3)
        {
            t += Time.deltaTime;
            float value = Mathf.Lerp(start, target, t / 3);
            mat.SetFloat("_FogDensity", value);
            yield return null;
        }

        mat.SetFloat("_FogDensity", target);
    }
}
