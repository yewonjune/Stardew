using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TimeManager : MonoBehaviour
{
    public int day = 1;
    public int hour = 6;
    public int minute = 0;

    public float timeScale = 60f; // 1초 = 게임 속 1분
    private float timer;

    public Text dayText;
    public Text timeText;

    public SoilTilemapController soilTilemapController;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * timeScale;

        if(timer >= 60f)
        {
            minute++;
            timer = 0;

            if(minute >= 60)
            {
                hour++;
                minute = 0;
            }

            if(hour >= 24)
            {
                EndDay();
            }
        }

        UpdateUI();
    }

    void EndDay()
    {
        day++;
        hour = 6;
        minute = 0;

        Debug.Log($"Day {day} 시작!");

        if (soilTilemapController) soilTilemapController.NewDay();
        else Debug.LogWarning("[DayManager] SoilTilemapController 참조 안 됨");
    }

    void UpdateUI()
    {
        dayText.text = $"Day {day}";

        int displayMinute = (minute / 10) * 10;
        timeText.text = $"{hour:00}:{displayMinute:00}";
    }
}
