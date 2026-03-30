using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    void OnEnable()
    {
        // 패널 열릴 때 현재 설정값으로 UI 초기화
        var s = SettingsManager.Instance;
        bgmSlider.SetValueWithoutNotify(s.BGMVolume);
        sfxSlider.SetValueWithoutNotify(s.SFXVolume);
        fullscreenToggle.SetIsOnWithoutNotify(s.IsFullscreen);
        InitResolutionDropdown(s.ResolutionIndex);
    }
    private void InitResolutionDropdown(int currentIndex)
    {
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var res in Screen.resolutions)
            options.Add($"{res.width} x {res.height}");
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
    }

    // Inspector에서 각 UI의 OnValueChanged에 연결
    public void OnBGMChanged(float value) => SettingsManager.Instance.SetBGMVolume(value);
    public void OnSFXChanged(float value) => SettingsManager.Instance.SetSFXVolume(value);
    public void OnFullscreenChanged(bool value) => SettingsManager.Instance.SetFullscreen(value);
    public void OnResolutionChanged(int index) => SettingsManager.Instance.SetResolution(index);

    public void OnCloseButton()
    {
        settingsPanel.SetActive(false);
        PlayerActionLock.Unlock("Settings");
        Time.timeScale = 1f;
    }
}
