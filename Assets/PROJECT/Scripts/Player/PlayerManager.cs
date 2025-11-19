using Unity.Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject cinemachineCamera;
    bool canMove = true;
    public bool CanMove { get { return canMove; } set { print("Set can move: " + value); cinemachineCamera.GetComponent<CinemachineInputAxisController>().enabled = value; canMove = value; } }

    public void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }
}
