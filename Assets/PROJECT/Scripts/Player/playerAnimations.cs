using UnityEngine;

public class playerAnimations : MonoBehaviour
{    
    public Animator anim;
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }
    
    
    private States State
    {
        get { return (States)anim.GetInteger("hamsterState"); }
        set { anim.SetInteger("hamsterState", (int)value); }
    }

    public void HeroIdleAnim()
    {
        State = States.idle;
    }

    public void HeroWalkAnim() { print("SET WALK"); State = States.walk; }

    public void HeroJumpAnim() { State = States.jump; }

    public void DelaySetStateIdle()
    {

    }
    void S()
    {
        State = States.idle;
    }
}

public enum States
{
    idle,
    walk,
    jump
}