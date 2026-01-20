using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] position;
    public bool hasBag; // ДОБАВЛЕНО: наличие сумки у игрока
    public bool collectedAdditionalLetters;
    public bool complitedMainIslandMainTasks;
}