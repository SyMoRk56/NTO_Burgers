using UnityEngine;

public class Footstep : MonoBehaviour
{
    public AudioClip grassSound, woodSound, stoneSound;
    public Transform rl, ll;
    public AudioSource source;
    public void PlayFootstepSound(bool right)
    {
        if(Physics.Raycast(rl.transform.position, rl.transform.position-new Vector3(0, -1, 0), out var hit))
        {
            if (hit.transform.CompareTag("Grass"))
            {
                source.PlayOneShot(grassSound);
            }
            if (hit.transform.CompareTag("Wood"))
            {
                source.PlayOneShot(woodSound);
            }
            if (hit.transform.CompareTag("Stone"))
            {
                source.PlayOneShot(stoneSound);
            }
        }
    }
}
