using UnityEngine;

public enum States
{
    idle,           // 0
    walk,           // 1  
    jump,           // 2
    carry_idle,     // 3
    carry_walk,     // 4
    carry_jump,     // 5
    fishing_bros,   // 6 ← ДОБАВИЛИ
    fishing_idle    // 7 ← ДОБАВИЛИ
}

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

    // === МЕТОДЫ ДЛЯ РЫБАЛКИ ===
    public void StartFishing()
    {
        State = States.fishing_bros;
        Debug.Log("Анимация: fishing_bros (6)");
    }

    public void FishingIdle()
    {
        State = States.fishing_idle;
        Debug.Log("Анимация: fishing_idle (7)");
    }

    public void EndFishing()
    {
        State = States.idle;
        Debug.Log("Анимация: idle (0)");
    }

    // === СТАРЫЕ МЕТОДЫ ===
    public void HeroIdleAnim(bool isCarrying = false)
    {
        State = isCarrying ? States.carry_idle : States.idle;
    }

    public void HeroWalkAnim(bool isCarrying = false)
    {
        State = isCarrying ? States.carry_walk : States.walk;
    }

    public void HeroJumpAnim(bool isCarrying = false)
    {
        State = isCarrying ? States.carry_jump : States.jump;
    }
}