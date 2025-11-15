using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public int level = 1;
    public int health = 3;

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
    }

    public void LoadPlayer()
    {
        Player_Data data = SaveSystem.LoadPlayer();

        level = data.level;
        health = data.health;

        Vector3 position;
        position.x = data.position[0];
        position.y = data.position[1];
        position.z = data.position[2];
        transform.position = position;
    }
}
