using UnityEngine;

public class PlayerAnimatorEvents : MonoBehaviour
{
    public PlayerMovement m;
    public void Step()
    {
        m.PlayStepSound();
    }
}
