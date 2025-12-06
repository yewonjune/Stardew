using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonIconUI : MonoBehaviour
{
    public Image iconImage;

    public Sprite springIcon;
    public Sprite summerIcon;
    public Sprite fallIcon;
    public Sprite winterIcon;

    // Start is called before the first frame update
    void Start()
    {
        UpdateIcon();

        if(SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged += OnSeasonChanged;
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (SeasonManager.Instance != null)
        {
            SeasonManager.Instance.OnSeasonChanged -= OnSeasonChanged;
        }
    }

    private void OnSeasonChanged(Season newSeason)
    {
        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (iconImage == null)
        {
            Debug.LogWarning("[SeasonIconUI] iconImage가 없음!");
            return;
        }

        // 계절에 따른 아이콘 변경
        var sm = SeasonManager.Instance;
        if (sm == null)
        {
            Debug.LogWarning("[SeasonIconUI] SeasonManager.Instance 없음");
            return;
        }

        switch (sm.currentSeason)
        {
            case Season.Spring:
                iconImage.sprite = springIcon;
                break;
            case Season.Summer:
                iconImage.sprite = summerIcon;
                break;
            case Season.Fall:
                iconImage.sprite = fallIcon;
                break;
            case Season.Winter:
                iconImage.sprite = winterIcon;
                break;
        }
    }
}
