using UnityEngine;
using System;
using System.Collections;

[DefaultExecutionOrder(1000)]
public class DayNightCycle : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        RenderSettings.skybox = daySkybox;
        RenderSettings.ambientIntensity = 2.0f;
        RenderSettings.reflectionIntensity = 1.0f;
        yield return new WaitForEndOfFrame();
        RenderSettings.ambientIntensity = 1.0f;

    }
    //public static event Action<int> OnTimeOfDayChanged;

    //[Header("Time")]
    //[Range(0, 3)]
    //[SerializeField] private int timeIndex = 1; // по умолчанию день

    //[Header("Skyboxes")]
    //[SerializeField] private Material sunriseSkybox;
    [SerializeField] private Material daySkybox;
    //[SerializeField] private Material sunsetSkybox;
    //[SerializeField] private Material nightSkybox;

    //[Header("Sun")]
    //[SerializeField] private Light sunLight; // назначается вручную через тег Sun

    //private const string SAVE_KEY = "TimeOfDayIndex";

    //private void Awake()
    //{

    //    return;
    //    // Ищем солнце по тегу Sun
    //    if (sunLight == null)
    //    {
    //        GameObject sunObj = GameObject.FindGameObjectWithTag("Sun");
    //        if (sunObj != null) sunLight = sunObj.GetComponent<Light>();

    //        if (sunLight == null || sunLight.type != LightType.Directional)
    //            Debug.LogError("DayNightCycle: Объект с тегом 'Sun' не найден или не Directional Light!");
    //    }
    //    return;
    //    // Загружаем время из сохранения или ставим день по умолчанию
    //    timeIndex = PlayerPrefs.GetInt(SAVE_KEY, 1);

    //    ApplyTime();
    //    OnTimeOfDayChanged?.Invoke(timeIndex);
    //}

    //private void Update()
    //{
    //    return;
    //    // Для теста переключаем время на ` (backquote)
    //    if (Input.GetKeyDown(KeyCode.BackQuote))
    //    {
    //        timeIndex = (timeIndex + 1) % 4;
    //        ApplyTime();
    //        SaveTime();
    //        OnTimeOfDayChanged?.Invoke(timeIndex);
    //    }
    //}

    //// ================= CORE =================
    //private void ApplyTime()
    //{
    //    return;
    //    switch (timeIndex)
    //    {
    //        case 0: // рассвет
    //            SetSun(-5f, new Color(1f, 0.5f, 0.3f), 0.2f); // солнце чуть ниже горизонта
    //            RenderSettings.skybox = sunriseSkybox;          // рассветный skybox
    //            RenderSettings.ambientLight = new Color(0.6f, 0.4f, 0.3f);
    //            break;

    //        case 1: // день
    //            SetSun(60f, new Color(1f, 0.95f, 0.8f), 1f);   // солнце высоко
    //            RenderSettings.skybox = daySkybox;             // дневной skybox
    //            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);
    //            break;

    //        case 2: // закат
    //            SetSun(-5f, new Color(1f, 0.4f, 0.2f), 0.3f); // солнце чуть ниже горизонта
    //            RenderSettings.skybox = sunsetSkybox;          // закатный skybox
    //            RenderSettings.ambientLight = new Color(0.5f, 0.35f, 0.25f);
    //            break;

    //        case 3: // ночь
    //            SetSun(-45f, new Color(0.05f, 0.05f, 0.15f), 0f); // солнце глубоко под горизонтом
    //            RenderSettings.skybox = nightSkybox;                // ночной skybox
    //            RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.08f); // очень тёмный
    //            break;
    //    }








    //    DynamicGI.UpdateEnvironment();
    //}

    //private void SetSun(float angleX, Color color, float intensity)
    //{
    //    if (sunLight == null) return;

    //    sunLight.transform.rotation = Quaternion.Euler(angleX, 170f, 0f);
    //    sunLight.color = color;
    //    sunLight.intensity = intensity;
    //    RenderSettings.sun = sunLight;
    //}

    //// ================= SAVE / LOAD =================
    //private void SaveTime()
    //{
    //    PlayerPrefs.SetInt(SAVE_KEY, timeIndex);
    //    PlayerPrefs.Save();
    //}

    //public int GetTimeIndex() => timeIndex;

    //public void SetTimeIndex(int index)
    //{
    //    timeIndex = Mathf.Clamp(index, 0, 3);
    //    ApplyTime();
    //    SaveTime();
    //    OnTimeOfDayChanged?.Invoke(timeIndex);
    //}
}
