using UnityEngine;

public class PlayerSaveSystem : MonoBehaviour
{
    public static PlayerSaveSystem Instance;
    private Transform player;

    void Awake() => Instance = this;

    public PlayerData GetData()
    {
        return new PlayerData
        {
            position = new float[]
            {
                player.position.x,
                player.position.y,
                player.position.z,
            }
        };
    }

    public void LoadData(PlayerData data)
    {
        player = GameManager.Instance.GetPlayer().transform;
        player.position = new Vector3(data.position[0], data.position[1], data.position[2]);
    }
}

