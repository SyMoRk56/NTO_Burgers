using UnityEngine;

[DefaultExecutionOrder(-6)]
public class SettingsSaveSystem : MonoBehaviour
{
    public static SettingsSaveSystem Instance;

    void Awake() => Instance = this;
    public string lang;

    public SettingsData GetData()
    {
        return new SettingsData()
        {
            lang = this.lang
        };
    }

    public void LoadData(SettingsData data)
    {
        lang = data.lang;
    }
}
