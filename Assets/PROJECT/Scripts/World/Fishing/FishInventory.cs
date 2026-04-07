using System.Collections.Generic;
using UnityEngine;

public class FishInventory : MonoBehaviour
{
    public static FishInventory instance;

    public Dictionary<FishScriptableObject, int> carriedFishes = new();

    private void Awake()
    {
        instance = this;
        var fishes = Resources.LoadAll<FishScriptableObject>("");
        foreach(var fish in fishes)
        {
            carriedFishes.Add(fish, 0);
        }
    }
    public void AddFishToInventory(FishScriptableObject fish, int c = 1)
    {
        carriedFishes.TryGetValue(fish, out var count);
        carriedFishes[fish] = (int)Mathf.Clamp(count + c, 0, Mathf.Infinity);
        foreach(var p in carriedFishes)
        {
            print(p.Key.name + ": " +  p.Value);
        }
    }
    public void RemoveFishFromInventory(FishScriptableObject fish, int c = 1)
    {
        carriedFishes.TryGetValue(fish, out var count);
        carriedFishes[fish] = (int)Mathf.Clamp(count - c, 0, Mathf.Infinity);
    }
    public bool HasEnoughFish(FishScriptableObject fish, int count)
    {
        return carriedFishes[fish] >= count;
    }
    public FishSaveData GetSaveData()
    {
        var saveData = new FishSaveData();
        List<int> fishes = new List<int>();
        foreach(var p in carriedFishes)
        {
            fishes.Add(p.Value);
        }
        saveData.carriedFishes = fishes;
        return saveData;
    }
    public void LoadSaveData(FishSaveData saveData)
    {
        if (saveData != null)
        {
            int i = 0;
            var keys = new List<FishScriptableObject>(carriedFishes.Keys); // replace KeyType

            foreach (var k in keys)
            {
                carriedFishes[k] = saveData.carriedFishes[i];
                i++;
            }
        }
    }
}
