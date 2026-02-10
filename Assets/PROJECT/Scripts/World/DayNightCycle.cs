using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
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
        if (Input.GetKeyDown(KeyCode.BackQuote)) // `
        {
            timeIndex++;
            if (timeIndex > 3)
                timeIndex = 0;

            SetTimeOfDay();
        }
    }

    void SetTimeOfDay()
    {
        switch (timeIndex)
        {
            case 0:
                RenderSettings.skybox = dawnSkybox;
                Debug.Log("Время суток: Рассвет");
                break;

            case 1:
                RenderSettings.skybox = daySkybox;
                Debug.Log("Время суток: День");
                break;

            case 2:
                RenderSettings.skybox = sunsetSkybox;
                Debug.Log("Время суток: Закат");
                break;

            case 3:
                RenderSettings.skybox = nightSkybox;
                Debug.Log("Время суток: Ночь");
                break;
        }

        // Обновляем освещение
        DynamicGI.UpdateEnvironment();
    }
    public int GetTimeIndex()
    {
        return timeIndex;
    }

    public void SetTimeIndex(int index)
    {
        timeIndex = Mathf.Clamp(index, 0, 3);
        SetTimeOfDay();
    }

}
