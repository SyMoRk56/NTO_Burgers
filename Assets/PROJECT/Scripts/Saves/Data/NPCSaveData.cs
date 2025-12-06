using System;

[Serializable]
public class NPCSaveData
{
    public string npcId;
    public float[] position;
    public int currentActionIndex;
    public string currentTargetName;
}
