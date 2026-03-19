using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    public GameObject cinemachineCamera;
    bool canMove = true;
    public static PlayerManager instance;
    public Transform hand;
    public bool CanMove { get { return canMove; } set { print("Set can move: " + value); cinemachineCamera.GetComponent<CinemachineInputAxisController>().enabled = value; canMove = value; GetComponentInChildren<CameraController>().enabled = value; } }

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        SetupComponents();
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
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.I))
        {
            SetThunder(true);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SetThunder(false);
        }
#endif
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
            print("SET " + value);
            yield return null;
        }

        mat.SetFloat("_FogDensity", target);
    }
}
