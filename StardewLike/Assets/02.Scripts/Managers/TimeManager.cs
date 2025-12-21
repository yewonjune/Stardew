using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class TimeManager : MonoBehaviour
{
    [FoldoutGroup("Events")]
    [LabelText("분 변경 이벤트 (hour, minute)")]
    public event System.Action<int, int> OnMinuteChanged;

    [FoldoutGroup("Internal State"), ReadOnly]
    int _prevMinute = -1;

    [FoldoutGroup("Internal State"), ReadOnly]
    private float timer;

    // ================= TIME =================
    [Title("게임 시간 설정")]
    [FoldoutGroup("Time"), LabelText("Day")]
    public int day = 1;

    [FoldoutGroup("Time"), LabelText("Hour (0~23)"), Range(0, 23)]
    public int hour = 6;

    [FoldoutGroup("Time"), LabelText("Minute (0~59)"), Range(0, 59)]
    public int minute = 0;

    [FoldoutGroup("Time"), LabelText("Time Scale (1초 당 게임 분)")]
    [MinValue(1f)]
    public float timeScale = 60f;

    // ================= UI =================
    [Title("UI 레퍼런스")]
    [FoldoutGroup("UI"), LabelText("Day Text")]
    public Text dayText;

    [FoldoutGroup("UI"), LabelText("Time Text")]
    public Text timeText;

    [FoldoutGroup("UI"), LabelText("Clock Hand")]
    public Image ClockHand;

    // ================= REFERENCES =================
    [Title("기타 레퍼런스")]
    [FoldoutGroup("References"), LabelText("Soil Tilemap Controller")]
    public SoilTilemapController soilTilemapController;

    void Update()
    {
        timer += Time.deltaTime * timeScale;

        if(timer >= 60f)
        {
            AdvanceMinute();
        }

        if (dayText && timeText) UpdateUI();
        if (ClockHand) UpdateClockHand();
    }

    private void AdvanceMinute()
    {
        timer = 0;
        minute++;

        if (minute >= 60)
        {
            minute = 0;
            hour++;
        }

        if (hour >= 24)
        {
            EndDay();
            return;
        }

        if (minute != _prevMinute)
        {
            _prevMinute = minute;
            OnMinuteChanged?.Invoke(hour, minute);
        }
    }

    public async void EndDay()
    {
        day++;
        hour = 6;
        minute = 0;

        SeasonManager.Instance?.OnNewDay(day);

        PlayerFatigueController playerFatigueController = FindObjectOfType<PlayerFatigueController>(true);
        if (playerFatigueController != null)
        {
            playerFatigueController.RecoverOnSleep(recoverAmount: 60f, fullRecover: false);
        }

        SoundManager.instance?.PlaySFX("Chicken");

        if (soilTilemapController) soilTilemapController.NewDay();

        // === 여기서 저장 ===
        try
        {
            var svc = CloudSaveService.Instance ?? FindObjectOfType<CloudSaveService>(true);
            if (svc == null) { Debug.LogError("[EndDay] CloudSaveService missing"); return; }
            await svc.InitTask;

            var inv = Inventory.instance;
            if (inv == null) { Debug.LogError("[EndDay] Inventory.instance missing"); return; }

            if (PlayerWallet.Instance == null) { Debug.LogError("[EndDay] PlayerWallet.Instance missing"); return; }

            if (WorldStateManager.Instance == null) { Debug.LogError("[EndDay] WorldStateManager.Instance missing"); return; }

            var player = GameObject.FindGameObjectWithTag("Player")?.transform
                         ?? FindObjectOfType<PlayerMovement>(true)?.transform;
            if (player == null) { Debug.LogError("[EndDay] Player transform missing"); return; }

            var data = SaveBuilder.Build(this, player);
            await svc.SaveAsync(BootParam.Slot, data);

            Debug.Log("[EndDay] Save OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[CloudSave] Save failed: " + ex);
        }
    }

    void UpdateUI()
    {
        if (dayText == null || timeText == null) return;

        int dayOfYear = (day - 1) % 120;   // 0~119
        int seasonIndex = dayOfYear / 30;  // 0,1,2,3
        int dayInSeason = (dayOfYear % 30) + 1; // 1~30

        string seasonKorean = "";

        switch ((Season)seasonIndex)
        {
            case Season.Spring: seasonKorean = "봄"; break;
            case Season.Summer: seasonKorean = "여름"; break;
            case Season.Fall: seasonKorean = "가을"; break;
            case Season.Winter: seasonKorean = "겨울"; break;
        }

        dayText.text = $"{seasonKorean} {dayInSeason}일";

        int displayMinute = (minute / 10) * 10;
        timeText.text = $"{hour:00}:{displayMinute:00}";
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        RebindSoilController();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
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
