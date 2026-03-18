using System;
using UnityEngine;

[Serializable]
public struct Task
{
    public string recieverName;
    public string adress;
    public string id;
    public bool isStory;

    public Task(string recieverName, string adress, string id, bool isStory = false)
    {
        this.recieverName = recieverName;
        this.adress = adress;
        this.id = id;
        this.isStory = isStory;
    }
}