using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]

public class Player_Data
{
    public int level;
    public int health;
    public float[] position;

    public Player_Data(Player player)
    {
        level = player.level;
        health = player.health;

        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;

    }
}
