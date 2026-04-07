using UnityEngine;
using System.Collections.Generic;

public class ObjectStateManager : MonoBehaviour
{
    public static ObjectStateManager Instance;

    // Храним состояние объектов
    private Dictionary<string, bool> objectStates = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Загружаем состояния из PlayerPrefs при старте
            LoadFromPlayerPrefs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Пометить объект как активированный
    public void MarkObjectAsActivated(string objectId)
    {
        if (!objectStates.ContainsKey(objectId))
        {
            objectStates.Add(objectId, true);
        }
        else
        {
            objectStates[objectId] = true;
        }

        // Немедленно сохраняем в PlayerPrefs
        SaveToPlayerPrefs();

        Debug.Log($"Объект {objectId} отмечен как активированный");
    }

    // Проверить, активирован ли объект
    public bool IsObjectActivated(string objectId)
    {
        return objectStates.ContainsKey(objectId) && objectStates[objectId];
    }

    // Получить все состояния
    public Dictionary<string, bool> GetAllStates()
    {
        return new Dictionary<string, bool>(objectStates);
    }

    // Загрузить состояния
    public void LoadStates(Dictionary<string, bool> states)
    {
        objectStates.Clear();
        foreach (var kvp in states)
        {
            objectStates.Add(kvp.Key, kvp.Value);
        }

        // Сохраняем в PlayerPrefs для надежности
        SaveToPlayerPrefs();

        Debug.Log($"Загружено {objectStates.Count} состояний объектов");
    }

    // Очистить все состояния
    public void Clear()
    {
        objectStates.Clear();

        // Очищаем PlayerPrefs
        PlayerPrefs.DeleteKey("ObjectStates");
        PlayerPrefs.Save();

        Debug.Log("Состояния объектов очищены");
    }

    // Сохраняем в PlayerPrefs для надежности
    private void SaveToPlayerPrefs()
    {
        string data = "";
        foreach (var kvp in objectStates)
        {
            data += $"{kvp.Key}:{kvp.Value};";
        }

        PlayerPrefs.SetString("ObjectStates", data);
        PlayerPrefs.Save();

        Debug.Log($"Состояния сохранены в PlayerPrefs: {data}");
    }

    // Загружаем из PlayerPrefs
    private void LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("ObjectStates"))
        {
            string data = PlayerPrefs.GetString("ObjectStates");
            string[] entries = data.Split(';');

            foreach (string entry in entries)
            {
                if (!string.IsNullOrEmpty(entry))
                {
                    string[] parts = entry.Split(':');
                    if (parts.Length == 2)
                    {
                        string objectId = parts[0];
                        bool isActive = bool.Parse(parts[1]);

                        if (!objectStates.ContainsKey(objectId))
                        {
                            objectStates.Add(objectId, isActive);
                        }
                    }
                }
            }

            Debug.Log($"Загружено из PlayerPrefs: {objectStates.Count} объектов");
        }
        else
        {
            Debug.Log("Нет сохраненных состояний в PlayerPrefs");
        }
    }

    // Метод для отладки
    public void DebugStates()
    {
        Debug.Log("=== СОСТОЯНИЯ ОБЪЕКТОВ ===");
        foreach (var kvp in objectStates)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        Debug.Log("==========================");
    }
}