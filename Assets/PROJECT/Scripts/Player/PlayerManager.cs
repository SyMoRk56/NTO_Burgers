using Unity.Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject cinemachineCamera;
    bool canMove = true;
    public static PlayerManager instance;
    public Transform hand;
    public bool CanMove { get { return canMove; } set { print("Set can move: " + value); cinemachineCamera.GetComponent<CinemachineInputAxisController>().enabled = value; canMove = value; } }

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
}
