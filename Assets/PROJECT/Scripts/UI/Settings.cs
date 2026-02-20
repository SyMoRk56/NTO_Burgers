using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


public class Settings : MonoBehaviour
{

    Resolution[] resolution;
    public TMP_Dropdown langDropdown;
    public Slider masterVolumeSlider, musicVolumeSlider, dialoguesVolumeSlider, sensitivitySlider, shakeScaleSlider;
    void Start()
    {
        langDropdown.SetValueWithoutNotify(LocalizationManager.Instance.CurrentLanguage == "RU" ? 0 : 1);
        List<string> options = new List<string>();
        resolution = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolution.Length; i++)
        {
            string option = resolution[i].width + " x " + resolution[i].height + " " + resolution[i].refreshRate + "Hz";
            options.Add(option);
            if (resolution[i].width == Screen.currentResolution.width && resolution[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        LoadSettings(currentResolutionIndex);
    }
    
    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolution[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void ExitSettings()
    {
        SceneManager.LoadScene("menu");
    }
    
    public void SaveSettings()
    {
        var data = SettingsSaveSystem.Instance.GetData();
        data.masterVolume = masterVolumeSlider.value;
        data.dialoguesVolume = dialoguesVolumeSlider.value;
        data.musicVolume = musicVolumeSlider.value;
        data.sensitivity = sensitivitySlider.value;
        data.shakeScale = shakeScaleSlider.value;
        SettingsSaveSystem.Instance.LoadData(data);
        SettingsSaveManager.Instance.SaveSettings();
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        var data = SettingsSaveSystem.Instance.GetData();
        masterVolumeSlider.SetValueWithoutNotify(data.masterVolume);
        dialoguesVolumeSlider.SetValueWithoutNotify(data.dialoguesVolume);
        musicVolumeSlider.SetValueWithoutNotify(data.musicVolume);
        sensitivitySlider.SetValueWithoutNotify(data.sensitivity);
        shakeScaleSlider.SetValueWithoutNotify(data.shakeScale);
    }
    public void SetLang()
    {
        LocalizationManager.Instance.SetLanguage(langDropdown.value == 0 ? "RU" : "EN");
    }
    public void OnEnable()
    {
        FindFirstObjectByType<MenuCamera>().enabled = false;
        FindFirstObjectByType<FishWater>().enabled = false;
    }
    void OnDisable()
    {
        FindFirstObjectByType<MenuCamera>().enabled = true;
        FindFirstObjectByType<FishWater>().enabled = true;
    }
}

