using System.Collections;
using UnityEngine;

public class PlayerSaveSystem : MonoBehaviour
{
    public static PlayerSaveSystem Instance;
    private Transform player;

    IEnumerator Start()
    {
        Instance = this;
        while (GameManager.Instance.GetPlayer() == null)yield return null;
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
        print("PlayerSaveSystem:  get data" + player.transform.position);

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
        print("Set player position " + data.position.Length);
        if(data.position.Length == 3) StartCoroutine(SetPlayerPosDelay(data.position));
        else
        {
            player.GetComponent<PlayerMovement>().enabled = true;

        }

    }
    IEnumerator SetPlayerPosDelay(float[] pos)
    {
        player.position = new Vector3(
           pos[0],
           pos[1],
           pos[2]);
        print(pos[0]);
        print(pos[2]);
        print(player.position);
        yield return new WaitForSeconds(.3f);
        player.GetComponent<PlayerMovement>().enabled = true;
        yield break;
    }
}
