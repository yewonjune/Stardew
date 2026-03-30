using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string KEY_BGM = "BGMVolume";
    private const string KEY_SFX = "SFXVolume";
    private const string KEY_FULLSCREEN = "Fullscreen";
    private const string KEY_RESOLUTION = "ResolutionIndex";

    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public bool IsFullscreen { get; private set; }
    public int ResolutionIndex { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LoadSettings();
        ApplyAll();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = value;
        if (SoundManager.instance?.bgmSource != null)
            SoundManager.instance.bgmSource.volume = value;
        PlayerPrefs.SetFloat(KEY_BGM, value);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        if (SoundManager.instance?.sfxSource != null)
            SoundManager.instance.sfxSource.volume = value;
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }

    public void SetFullscreen(bool value)
    {
        IsFullscreen = value;
        Screen.fullScreen = value;
        PlayerPrefs.SetInt(KEY_FULLSCREEN, value ? 1 : 0);
    }

    public void SetResolution(int index)
    {
        var resolutions = Screen.resolutions;
        if (index < 0 || index >= resolutions.Length) return;
        ResolutionIndex = index;
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, IsFullscreen);
        PlayerPrefs.SetInt(KEY_RESOLUTION, index);
    }

    void LoadSettings()
    {
        BGMVolume = PlayerPrefs.GetFloat(KEY_BGM, 1f);
        SFXVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        IsFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
        ResolutionIndex = PlayerPrefs.GetInt(KEY_RESOLUTION, Screen.resolutions.Length - 1);
    }

    void ApplyAll()
    {
        SetBGMVolume(BGMVolume);
        SetSFXVolume(SFXVolume);
        SetFullscreen(IsFullscreen);
        SetResolution(ResolutionIndex);
    }
}