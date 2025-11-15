using UnityEngine;
using System;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public string defaultLanguage = "RU";

    public event Action OnLanguageChanged;

    private Dictionary<string, Dictionary<string, string>> table;

    public string CurrentLanguage { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        CurrentLanguage = PlayerPrefs.GetString("Language", defaultLanguage);

        LoadCSV();
    }

    private void LoadCSV()
    {
        table = CSVLoader.LoadLocalizationCSV("Localization/Localization");

        if (table == null)
            Debug.LogError("Localization table is NULL!");
    }

    public void SetLanguage(string lang)
    {
        CurrentLanguage = lang;
        PlayerPrefs.SetString("Language", lang);
        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (!table.ContainsKey(key))
        {
            Debug.LogWarning("Missing localization key: " + key);
            return key;
        }

        if (!table[key].ContainsKey(CurrentLanguage.ToLower()))
        {
            Debug.LogWarning($"Missing translation for {key} in language {CurrentLanguage}");
            return key;
        }

        return table[key][CurrentLanguage.ToLower()];
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetLanguage("EN");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetLanguage("RU");
        }
    }
}
