using UnityEngine;

[CreateAssetMenu(
    fileName = "FishData",
    menuName = "Fishing/Fish"
)]
public class FishScriptableObject : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] float minSpeed = 40f;
    [SerializeField] float maxSpeed = 120f;
    [SerializeField] float directionChangeIntervalMin = 0.2f;
    [SerializeField] float directionChangeIntervalMax = 0.8f;
    public float fishingTime = 5f;
    [Header("Spawn")]
    [SerializeField] int weight = 1; // ВЕС ВЫПАДЕНИЯ

    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxSpeed;
    public float DirectionChangeIntervalMin => directionChangeIntervalMin;
    public float DirectionChangeIntervalMax => directionChangeIntervalMax;
    public int Weight => Mathf.Max(0, weight);

    public GameObject prefab;
    public Sprite sprite;
}
