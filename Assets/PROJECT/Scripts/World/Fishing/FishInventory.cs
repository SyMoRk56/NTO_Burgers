using System.Collections.Generic;
using UnityEngine;

public class FishInventory : MonoBehaviour
{
    public static FishInventory instance; // Синглтон

    public Dictionary<FishScriptableObject, int> carriedFishes = new(); // Рыба и её количество

    private void Awake()
    {
        instance = this;

        // Загружаем все виды рыбы из Resources
        var fishes = Resources.LoadAll<FishScriptableObject>("");

        // Инициализируем словарь (по 0 каждой рыбы)
        foreach (var fish in fishes)
        {
            carriedFishes.Add(fish, 0);
        }
    }

    public void AddFishToInventory(FishScriptableObject fish, int c = 1)
    {
        // Получаем текущее количество
        carriedFishes.TryGetValue(fish, out var count);

        // Добавляем и ограничиваем снизу 0
        carriedFishes[fish] = (int)Mathf.Clamp(count + c, 0, Mathf.Infinity);

        // Вывод в лог (для отладки)
        foreach (var p in carriedFishes)
        {
            print(p.Key.name + ": " + p.Value);
        }
    }

    public void RemoveFishFromInventory(FishScriptableObject fish, int c = 1)
    {
        // Получаем текущее количество
        carriedFishes.TryGetValue(fish, out var count);

        // Уменьшаем (не ниже 0)
        carriedFishes[fish] = (int)Mathf.Clamp(count - c, 0, Mathf.Infinity);
    }

    public bool HasEnoughFish(FishScriptableObject fish, int count)
    {
        // Проверка — хватает ли рыбы
        return carriedFishes[fish] >= count;
    }

    public FishSaveData GetSaveData()
    {
        var saveData = new FishSaveData();

        List<int> fishes = new List<int>();

        // Сохраняем только значения (количество)
        foreach (var p in carriedFishes)
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

            // Берём ключи (виды рыб)
            var keys = new List<FishScriptableObject>(carriedFishes.Keys);

            // Загружаем значения по порядку
            foreach (var k in keys)
            {
                carriedFishes[k] = saveData.carriedFishes[i];
                i++;
            }
        }
    }
}