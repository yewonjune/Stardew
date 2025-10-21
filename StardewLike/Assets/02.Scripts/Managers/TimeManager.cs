using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public Image ClockHand;

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

        UpdateClockHand();
    }

    public async void EndDay()
    {
        day++;
        hour = 6;
        minute = 0;

        Debug.Log($"Day {day} 시작!");

        if (soilTilemapController) soilTilemapController.NewDay();

        // === 여기서 저장 ===
        try
        {
            var svc = FindObjectOfType<CloudSaveService>();
            var player = FindObjectOfType<PlayerMovement>()?.transform ?? this.transform;

            var data = SaveBuilder.Build(this, player);
            await svc.SaveAsync("slot1", data); // 슬롯명은 상황에 맞게
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CloudSave] Save failed: " + ex);
        }
    }

    void UpdateUI()
    {
        dayText.text = $"Day {day}";

        int displayMinute = (minute / 10) * 10;
        timeText.text = $"{hour:00}:{displayMinute:00}";
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged; // 추가
        RebindSoilController();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged; // 추가
    }

    void OnActiveSceneChanged(Scene prev, Scene next)
    {
        RebindSoilController();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == SceneManager.GetActiveScene())
            RebindSoilController();
    }

    void RebindSoilController()
    {
        soilTilemapController = null;

        var all = FindObjectsOfType<SoilTilemapController>(includeInactive: false);
        if (all == null || all.Length == 0)
        {
            Debug.Log("[PlayerUseTool] 씬 어디에도 SoilTilemapController가 없음");
            return;
        }

        float best = float.PositiveInfinity;
        SoilTilemapController bestCtrl = null;
        Vector3 p = transform.position;

        foreach (var s in all)
        {
            if (!s.isActiveAndEnabled) continue;
            float d = (s.transform.position - p).sqrMagnitude;
            if (d < best) { best = d; bestCtrl = s; }
        }

        soilTilemapController = bestCtrl ?? all[0];
    }

    void UpdateClockHand()
    {
        if (!ClockHand) return;

        int totalMinutes = hour * 60 + minute;

        float t = Mathf.InverseLerp(360f, 1440f, totalMinutes);

        float angle = Mathf.Lerp(-90f, 90f, t);

        ClockHand.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
