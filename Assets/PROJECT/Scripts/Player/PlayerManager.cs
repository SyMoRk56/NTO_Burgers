using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    bool canMove = true;
    public bool CanMove { get { return canMove; } set { print("Set can move: " + value); canMove = value; } }

    public void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }
}
