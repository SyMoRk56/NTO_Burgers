using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-6)]
public class SettingsSaveSystem : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    public static SettingsSaveSystem Instance;

    void Awake() => Instance = this;
    public string lang = "RU";
    public float masterVolume = 1;
    public float musicVolume = 1;
    public float dialoguesVolume = 1;
    public static float ssensitivity = .7f;
    public static float sshakeScale = .5f;
    public SettingsData GetData()
    {
        return new SettingsData()
        {
            lang = this.lang,
            masterVolume = this.masterVolume,
            musicVolume = this.musicVolume,
            dialoguesVolume = this.dialoguesVolume,
            sensitivity = SettingsSaveSystem.ssensitivity,
            shakeScale = SettingsSaveSystem.sshakeScale,

        };
    }

    public void LoadData(SettingsData data)
    {
        lang = data.lang;
        masterVolume = data.masterVolume;
        musicVolume = data.musicVolume;
        dialoguesVolume = data.dialoguesVolume;
        mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume+ .001f)*20);
        mixer.SetFloat("DialoguesVolume", Mathf.Log10(dialoguesVolume+.001f)*20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume+.001f)*20);
        ssensitivity = data.sensitivity;
        sshakeScale = data.shakeScale;
    }
}
