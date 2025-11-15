using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class SettingsData
{
    
}

public class SettingsSaveSystem
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
