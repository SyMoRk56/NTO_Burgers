using UnityEngine;

public class PlayerMetrics : MonoBehaviour
{
    public static PlayerMetrics Instance;
    public int tabMenuOpens;

    public int steps;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterStep()
    {
        steps++;
    }
    public void RegisterTabOpen()
    {
        tabMenuOpens++;
    }

    public MetricsData GetSaveData()
    {
        return new MetricsData
        {
            steps = this.steps,
            tabMenuOpens = this.tabMenuOpens
        };
    }

    public void LoadData(MetricsData data)
    {
        if (data == null) return;
        steps = data.steps;
        tabMenuOpens = data.tabMenuOpens;
    }
}