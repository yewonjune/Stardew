using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Season
{
    Spring,
    Summer,
    Fall,
    Winter
}

public class SeasonManager : MonoBehaviour
{
    public static SeasonManager Instance { get; private set; }

    public Season currentSeason = Season.Spring;

    public event Action<Season> OnSeasonChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSeason(Season season)
    {
        if (currentSeason == season) return;

        currentSeason = season;
        Debug.Log($"[Season] 계절 변경: {currentSeason}");
        OnSeasonChanged?.Invoke(currentSeason);
    }
    public void OnNewDay(int day)
    {
        int dayOfYear = (day - 1) % 120;   // 0~119
        int seasonIndex = dayOfYear / 30;

        Season newSeason = (Season)seasonIndex;
        SetSeason(newSeason);
    }

}
