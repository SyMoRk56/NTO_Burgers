using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-6)]
public class SettingsSaveSystem : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    public static SettingsSaveSystem Instance;

    void Awake() => Instance = this;
    public string lang;
    public float masterVolume = 1;
    public float musicVolume = 1;
    public float dialoguesVolume = 1;
    public SettingsData GetData()
    {
        return new SettingsData()
        {
            lang = this.lang,
            masterVolume = this.masterVolume,
            musicVolume = this.musicVolume,
            dialoguesVolume = this.dialoguesVolume
        };
    }

    public void LoadData(SettingsData data)
    {
        lang = data.lang;
        masterVolume = data.masterVolume;
        musicVolume = data.musicVolume;
        dialoguesVolume = data.dialoguesVolume;
        mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume)*20);
        mixer.SetFloat("DialoguesVolume", Mathf.Log10(dialoguesVolume)*20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume)*20);
    }
}
