using UnityEngine;

public class LocationMusic : MonoBehaviour
{
    Transform player;
    public float dist;
    public AudioClip clip;
    void Start()
    {
        player = GameManager.Instance.GetPlayer().transform;
    }
    void Update()
    {
        if(Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(transform.position.x, transform.position.z)) < dist)
        {
            LocationMusicManager.Instance.PlayMusic(clip);
        }
    }
    
}
