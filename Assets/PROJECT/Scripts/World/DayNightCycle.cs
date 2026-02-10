using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
{
    public static event Action<int> OnTimeOfDayChanged;

    public Material dawnSkybox;
    public Material daySkybox;
    public Material sunsetSkybox;
    public Material nightSkybox;

    private int timeIndex = 0;

    void Start()
    {
        SetTimeOfDay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            timeIndex = (timeIndex + 1) % 4;
            SetTimeOfDay();
        }
    }

    void SetTimeOfDay()
    {
        switch (timeIndex)
        {
            case 0:
                RenderSettings.skybox = dawnSkybox;
                break;
            case 1:
                RenderSettings.skybox = daySkybox;
                break;
            case 2:
                RenderSettings.skybox = sunsetSkybox;
                break;
            case 3:
                RenderSettings.skybox = nightSkybox;
                break;
        }

        DynamicGI.UpdateEnvironment();

        // 🔥 уведомляем всех NPC
        OnTimeOfDayChanged?.Invoke(timeIndex);
    }

    public int GetTimeIndex() => timeIndex;

    public void SetTimeIndex(int index)
    {
        timeIndex = Mathf.Clamp(index, 0, 3);
        SetTimeOfDay();
    }
}
