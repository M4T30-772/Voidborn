using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{   
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] public Slider musicSlider;

    public void SetVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    public void SetQuality(int _qualityIndex)
    {
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    public void SetFullScreen(bool _isFullScreen)
    {
        Screen.fullScreen = _isFullScreen;
    }

    public void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        musicSlider.value = 1f;
        SetVolume();
    }

    void Update()
    {
        
    }
}
