using DG.Tweening;
using UnityEngine;

public class Bush : MonoBehaviour
{
    public int flyoutTimes = 5;
    public float cooldown = 5;
    public Animator anim;
    float timer;
    int m_flyoutTimes;

    private void Update()
    {
        timer += Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if(timer > cooldown && m_flyoutTimes < flyoutTimes)
        {
            transform.DOShakeRotation(2, 10, fadeOut: true);
            m_flyoutTimes++;
            timer = 0;
            anim.Play("Anim");
            
        }
    }
}
