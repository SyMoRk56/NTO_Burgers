using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-1)]
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    [Tooltip("Default language code (e.g. RU, EN)")]
    public string defaultLanguage = "RU";

    public event Action OnLanguageChanged;

    private Dictionary<string, Dictionary<string, string>> table;

    public string CurrentLanguage { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        LoadCSV();

        LoadLanguageFromSettings();
    }

    private void LoadCSV()
    {
        table = CSVLoader.LoadLocalizationCSV("Localization/Localization");

        if (table == null)
            Debug.LogError("Localization table is NULL!");
        else
            Debug.Log($"Localization loaded: {table.Count} keys");
    }

    private void LoadLanguageFromSettings()
    {
        // Если SettingsSaveSystem ещё не готов — подстрахуемся.
        if (SettingsSaveSystem.Instance == null)
        {
            // Установим дефолт (без сохранения) и выйдем — другой код может вызвать SetLanguage позже.
            CurrentLanguage = defaultLanguage;
            Debug.LogWarning("SettingsSaveSystem not ready — using default language: " + CurrentLanguage);
            return;
        }

        var settings = SettingsSaveSystem.Instance.GetData();

        // Если в settings еще нет языка — положим дефолт
        if (settings == null)
        {
            CurrentLanguage = defaultLanguage;
            Debug.LogWarning("Settings data is null — using default language: " + CurrentLanguage);
            return;
        }

        if (string.IsNullOrEmpty(settings.lang))
            settings.lang = defaultLanguage;

        SetLanguage(settings.lang);

        Debug.Log("Loaded language: " + CurrentLanguage);
    }

    /// <summary>
    /// Установить текущий язык. Сохраняет выбор в Settings через SettingsSaveManager.
    /// Вызывает OnLanguageChanged.
    /// </summary>
    public void SetLanguage(string lang)
    {
        if (string.IsNullOrEmpty(lang))
        {
            Debug.LogWarning("Attempt to set empty language, ignoring.");
            return;
        }

        CurrentLanguage = lang;

        // Попробуем сохранить в settings (если доступно)
        if (SettingsSaveSystem.Instance != null)
        {
            var settings = SettingsSaveSystem.Instance.GetData() ?? new SettingsData();
            settings.lang = lang;
            SettingsSaveSystem.Instance.LoadData(settings);

            if (SettingsSaveManager.Instance != null)
            {
                SettingsSaveManager.Instance.SaveSettings();
            }
            else
            {
                Debug.LogWarning("SettingsSaveManager.Instance is null — language not persisted to disk.");
            }
        }
        else
        {
            Debug.LogWarning("SettingsSaveSystem.Instance is null — language not persisted to settings.");
        }

        OnLanguageChanged?.Invoke();

        Debug.Log("Language changed to: " + lang);
    }

    /// <summary>
    /// Возвращает перевод по ключу; если ключ или перевод отсутствуют, возвращает ключ и лог предупреждения.
    /// </summary>
    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "";

        if (table == null)
        {
            Debug.LogWarning("Localization table is null. Key: " + key);
            return key;
        }

        if (!table.ContainsKey(key))
        {
            Debug.LogWarning("Missing localization key: " + key);
            return key;
        }

        // Нормализуем язык в нижний регистр - в CSV заголовки обычно en/ru
        string langCode = (CurrentLanguage ?? defaultLanguage).ToLower();

        if (!table[key].ContainsKey(langCode))
        {
            Debug.LogWarning($"Missing translation for {key} in language {CurrentLanguage} (looking for '{langCode}').");
            return key;
        }

        return table[key][langCode];
    }

    /// <summary> Алиас для совместимости с кодом, который раньше вызывал Get(...) </summary>
    public string Get(string key) => GetText(key);

    private void Update()
    {
        // Горячие клавиши для теста локализации во время разработки
        if (Input.GetKeyDown(KeyCode.F1))
            SetLanguage("EN");

        if (Input.GetKeyDown(KeyCode.F2))
            SetLanguage("RU");
    }
}
