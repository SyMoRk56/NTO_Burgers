using UnityEngine;

[System.Serializable]
public class PlayerData : MonoBehaviour
{
    public float[] position;
}
public class PlayerSaveSystem
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
        player.position = new Vector3(data.position[0], data.position[1], data.position[2]);
    }
}

