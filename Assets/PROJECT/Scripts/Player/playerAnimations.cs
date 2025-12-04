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

    // Добавляем перегрузку методов с параметром isCarrying
    public void HeroIdleAnim(bool isCarrying = false)
    {
        if (isCarrying)
            State = States.carry_idle;
        else
            State = States.idle;
    }

    public void HeroWalkAnim(bool isCarrying = false)
    {
        print("SET WALK");
        if (isCarrying)
            State = States.carry_walk;
        else
            State = States.walk;
    }

    public void HeroJumpAnim(bool isCarrying = false)
    {
        if (isCarrying)
            State = States.carry_jump;
        else
            State = States.jump;
    }

    public void DelaySetStateIdle()
    {

    }

    void S()
    {
        State = States.idle;
    }
}

// Расширяем enum для поддержки анимаций с переноской
public enum States
{
    idle,
    walk,
    jump,
    carry_idle,    // Стоит с объектом
    carry_walk,    // Идет с объектом
    carry_jump     // Прыгает с объектом
}