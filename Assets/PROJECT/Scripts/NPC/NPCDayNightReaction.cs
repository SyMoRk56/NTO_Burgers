using UnityEngine;
using System;

public class NPCDayNightCycle : MonoBehaviour
{
    public static event Action<int> OnTimeOfDayChanged;

    [Header("Skyboxes")]
    public Material dawnSkybox;
    public Material daySkybox;
    public Material sunsetSkybox;
    public Material nightSkybox;

    // 0 - рассвет, 1 - день, 2 - закат, 3 - ночь
    [SerializeField] private int timeIndex = 1;

    private void Start()
    {
        return;
        ApplyTimeOfDay();
    }

    private void Update()
    {
        // DEBUG: смена по `
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            timeIndex = (timeIndex + 1) % 4;
            ApplyTimeOfDay();
        }
    }

    private void ApplyTimeOfDay()
    {
        switch (timeIndex)
        {
            case 0:
                RenderSettings.skybox = dawnSkybox;
                Debug.Log("🌅 Рассвет");
                break;
            case 1:
                RenderSettings.skybox = daySkybox;
                Debug.Log("☀️ День");
                break;
            case 2:
                RenderSettings.skybox = sunsetSkybox;
                Debug.Log("🌇 Закат");
                break;
            case 3:
                RenderSettings.skybox = nightSkybox;
                Debug.Log("🌙 Ночь");
                break;
        }

        DynamicGI.UpdateEnvironment();
        OnTimeOfDayChanged?.Invoke(timeIndex);
    }

    // === ДЛЯ СЕЙВОВ ===
    public int GetTimeIndex()
    {
        return timeIndex;
    }

    public void SetTimeIndex(int index)
    {
        timeIndex = Mathf.Clamp(index, 0, 3);
        ApplyTimeOfDay();
    }
}
