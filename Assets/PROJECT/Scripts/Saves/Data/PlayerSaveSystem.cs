using UnityEngine;

public class PlayerSaveSystem : MonoBehaviour
{
    public static PlayerSaveSystem Instance;
    private Transform player;

    void Awake()
    {
        Instance = this;
        player = GameManager.Instance.GetPlayer()?.transform;
    }

    public PlayerData GetData()
    {
        if (player == null)
            player = GameManager.Instance.GetPlayer()?.transform;

        if (player == null)
        {
            Debug.LogWarning("Player not found during saving!");
            return null;
        }

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
        if (data == null) return;

        if (player == null)
            player = GameManager.Instance.GetPlayer()?.transform;

        if (player == null)
        {
            Debug.LogError("Player cannot be assigned during load!");
            return;
        }

        player.position = new Vector3(
            data.position[0],
            data.position[1],
            data.position[2]);
    }
}
