using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-1)]
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

        LoadCSV();

        LoadLanguageFromSettings();
    }

    private void LoadCSV()
    {
        table = CSVLoader.LoadLocalizationCSV("Localization/Localization");

        if (table == null)
            Debug.LogError("Localization table is NULL!");
    }
    private void LoadLanguageFromSettings()
    {
        print(SettingsSaveSystem.Instance == null);
        var settings = SettingsSaveSystem.Instance.GetData();
        print(settings.lang);
        if (string.IsNullOrEmpty(settings.lang))
            settings.lang = defaultLanguage;

        SetLanguage(settings.lang);

        Debug.Log("Loaded language: " + CurrentLanguage);
    }
    public void SetLanguage(string lang)
    {
        CurrentLanguage = lang;

        // Сохраняем в Settings.json
        var settings = SettingsSaveSystem.Instance.GetData();
        settings.lang = lang;
        SettingsSaveSystem.Instance.LoadData(settings);
        SettingsSaveManager.Instance.SaveSettings();

        OnLanguageChanged?.Invoke();

        Debug.Log("Language changed to: " + lang);
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
            SetLanguage("EN");

        if (Input.GetKeyDown(KeyCode.F2))
            SetLanguage("RU");
    }
}
