using UnityEngine;

public class PlayerCollisionSound : MonoBehaviour
{
    public AudioClip grassSound;
    public AudioClip woodSound;
    public AudioClip stoneSound;

    public AudioSource audioSource;

         

    private void OnCollisionEnter(Collision collision)
    {
        float speed = (collision.relativeVelocity - new Vector3(0, collision.relativeVelocity.y) ).magnitude;
        if (speed < 7) return;

        AudioClip clipToPlay = null;
        print("PlaySound " + collision.transform.tag);
        if (collision.transform.CompareTag("Grass"))
            clipToPlay = grassSound;
        else if (collision.transform.CompareTag("Wood"))
            clipToPlay = woodSound;
        else if (collision.transform.CompareTag("Stone"))
            clipToPlay = stoneSound;
        print(clipToPlay == null);
        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay, 1);
    }
}
