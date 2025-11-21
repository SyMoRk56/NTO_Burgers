using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-6)]
public class SettingsSaveSystem : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    public static SettingsSaveSystem Instance;

    void Awake() => Instance = this;
    public string lang;
    public float masterVolume;
    public SettingsData GetData()
    {
        return new SettingsData()
        {
            lang = this.lang,
            masterVolume = this.masterVolume
        };
    }

    public void LoadData(SettingsData data)
    {
        lang = data.lang;
        masterVolume = data.masterVolume;
        mixer.SetFloat("Volume", Mathf.Log10(masterVolume)*20);
    }
}
