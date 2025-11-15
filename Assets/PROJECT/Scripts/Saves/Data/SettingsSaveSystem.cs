using UnityEngine;

public class SettingsSaveSystem : MonoBehaviour
{
    public static SettingsSaveSystem Instance;

    void Awake() => Instance = this;

    public SettingsData GetData()
    {
        return new SettingsData()
        {

        };
    }

    public void LoadData(SettingsData data)
    {
        
    }
}
