using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider UISlider;


    private const string MUSIC_VOLUME = "BackgroundMusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private const string UI_Volume = "UISounds";

    void Start()
    {
        InitializeSliderValues();

        // Add listeners for slider changes
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        UISlider.onValueChanged.AddListener(SetUIVolume);
    }

    private void SetMusicVolume(float value)
    {
        float dbValue = value > 0 ? Mathf.Log10(Mathf.Clamp01(value)) * 20f : -80f;
        audioMixer.SetFloat(MUSIC_VOLUME, dbValue);
        PlayerPrefs.SetFloat(MUSIC_VOLUME, value);
    }

    private void SetSFXVolume(float value)
    {
        float dbValue = value > 0 ? Mathf.Log10(Mathf.Clamp01(value)) * 20f : -80f;
        audioMixer.SetFloat(SFX_VOLUME, dbValue);
        PlayerPrefs.SetFloat(SFX_VOLUME, value);
    }

    private void SetUIVolume(float value)
    {
        float dbValue = value > 0 ? Mathf.Log10(Mathf.Clamp01(value)) * 20f : -80f;
        audioMixer.SetFloat(UI_Volume, dbValue);
        PlayerPrefs.SetFloat(UI_Volume, value);
    }

    public void InitializeSliderValues()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_VOLUME, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_VOLUME, 1f);
        float ui = PlayerPrefs.GetFloat(UI_Volume, 1f);

        musicSlider.SetValueWithoutNotify(music);
        sfxSlider.SetValueWithoutNotify(sfx);
        UISlider.SetValueWithoutNotify(ui);

        SetMusicVolume(music);
        SetSFXVolume(sfx);
        SetUIVolume(ui);
    }
}
