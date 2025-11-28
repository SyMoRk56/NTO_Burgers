using System;
using UnityEngine;

[Serializable]
public struct Task
{
    public string recieverName;
    public string adress;
    public string id;

    public Task(string recieverName, string adress, string id)
    {
        this.recieverName = recieverName;
        this.adress = adress;
        this.id = id;
    }
}