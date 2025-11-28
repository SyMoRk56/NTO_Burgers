using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


public class Settings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolution;
    public TMP_Dropdown langDropdown;
    public Slider volumeSlider;
    void Start()
    {
        langDropdown.SetValueWithoutNotify(LocalizationManager.Instance.CurrentLanguage == "RU" ? 0 : 1);
        resolutionDropdown.ClearOptions();
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

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
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
        //PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        var data = SettingsSaveSystem.Instance.GetData();
        //data.resolution = resolutionDropdown.value;
        data.masterVolume = volumeSlider.value;
        SettingsSaveSystem.Instance.LoadData(data);
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        var data = SettingsSaveSystem.Instance.GetData();
        resolutionDropdown.SetValueWithoutNotify(data.resolution);
        volumeSlider.SetValueWithoutNotify(data.masterVolume);

    }
    public void SetLang()
    {
        LocalizationManager.Instance.SetLanguage(langDropdown.value == 0 ? "RU" : "EN");
    }
}

