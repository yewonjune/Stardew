using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    [Header("Data")]
    public Seeds seedData;                 // ScriptableObject: growthDays, regrowAfterHarvest, regrowDays, harvestItem, cropPrefab 등
    public List<Sprite> growthSprites;     // 단계별 스프라이트 (초기성장/재성장에 공통 사용)

    [Header("Runtime")]
    public Vector3Int cell;                // 그리드 좌표(Soil에서 세팅)
    private SoilTilemapController soil;    // 소유 타일맵 컨트롤러
    private SpriteRenderer sr;

    // 내부 상태
    int daysProgress;                      // 최초 성장 진행일(0..growthDays)
    int regrowProgress;                    // 재성장 진행일(0..regrowDays)
    bool wateredToday;                     // 오늘 물을 줬는지
    bool harvestedOnce;                    // 한 번이라도 수확했는지(재성장 구간 진입 플래그)

    // == 공개 프로퍼티/메서드 (SoilTilemapController가 사용) =====================

    /// <summary>Soil에서 생성 직후 호출: 데이터/좌표/참조 연결</summary>
    public void Init(SoilTilemapController soilTilemapController, Vector3Int cell, Seeds data)
    {
        soil = soilTilemapController;
        this.cell = cell;
        seedData = data;

        if (!sr) sr = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    /// <summary>오늘 물을 줬음을 표시 (Soil에서 호출)</summary>
    public void SetWateredToday() => wateredToday = true;

    /// <summary>하루가 지남: 물을 줬다면 성장 진행</summary>
    public void OnNewDay()
    {
        if (!seedData) return;

        if (IsRegrowing())
        {
            if (wateredToday && regrowProgress < seedData.regrowDays)
                regrowProgress++;
        }
        else
        {
            if (wateredToday && daysProgress < seedData.growthDays)
                daysProgress++;
        }

        wateredToday = false; // 다음 날로 이월 금지
        UpdateVisual();
    }

    /// <summary>현재 시각적 성장 단계를 저장용 정수로 제공.
    /// 0..(N-1)=초기성장 / N..(2N-1)=재성장. (N=growthSprites.Count, 최소 1로 간주)</summary>
    public int CurrentStage
    {
        get
        {
            int N = Mathf.Max(1, (growthSprites != null ? growthSprites.Count : 0));
            if (!seedData || N == 1)
            {
                // 스프라이트 1장만 있거나 데이터가 없으면 0/또는 재성장 시 N 반환
                return harvestedOnce && seedData && seedData.regrowAfterHarvest ? N : 0;
            }

            if (!harvestedOnce)
            {
                // 초기성장 비율 → 0..N-1
                float denom = Mathf.Max(1, seedData.growthDays);
                int stage = Mathf.RoundToInt((daysProgress / denom) * (N - 1));
                return Mathf.Clamp(stage, 0, N - 1);
            }
            else if (seedData.regrowAfterHarvest)
            {
                // 재성장 비율 → N..2N-1
                float denom = Mathf.Max(1, seedData.regrowDays);
                int rStage = Mathf.RoundToInt((regrowProgress / denom) * (N - 1));
                rStage = Mathf.Clamp(rStage, 0, N - 1);
                return N + rStage;
            }

            // 단발성 작물에서 harvestedOnce=true 상태가 저장될 일은 거의 없지만, 방어적으로 마지막 단계 반환
            return N - 1;
        }
    }

    /// <summary>저장된 CurrentStage 값을 다시 내부 상태로 역설정</summary>
    public void ForceSetStage(int savedStage)
    {
        if (!seedData) return;

        int N = Mathf.Max(1, (growthSprites != null ? growthSprites.Count : 0));
        N = Mathf.Max(N, 1);
        int maxVisualIndex = N - 1;

        if (savedStage < N)
        {
            // 초기성장 구간
            harvestedOnce = false;
            float t = (maxVisualIndex == 0) ? 1f : (savedStage / (float)maxVisualIndex);
            daysProgress = Mathf.RoundToInt(t * Mathf.Max(0, seedData.growthDays));
            regrowProgress = 0;
        }
        else
        {
            // 재성장 구간 (N..2N-1)
            harvestedOnce = true;
            int r = Mathf.Clamp(savedStage - N, 0, maxVisualIndex);
            float t = (maxVisualIndex == 0) ? 1f : (r / (float)maxVisualIndex);
            regrowProgress = Mathf.RoundToInt(t * Mathf.Max(0, seedData.regrowDays));
            // 재성장에 들어왔다는 건 최초성장은 끝났음을 의미
            daysProgress = Mathf.Max(daysProgress, seedData.growthDays);
        }

        UpdateVisual();
    }

    /// <summary>성숙 여부</summary>
    public bool IsMature()
    {
        if (!seedData) return false;

        if (!harvestedOnce)
            return daysProgress >= seedData.growthDays;

        if (seedData.regrowAfterHarvest)
            return regrowProgress >= seedData.regrowDays;

        return false;
    }

    /// <summary>현재 상태에서 수확 시도</summary>
    public bool TryHarvest(out Item itemOut)
    {
        itemOut = null;
        if (!IsMature() || !seedData) return false;

        // 드랍/인벤토리용 결과
        itemOut = seedData.harvestItem;

        if (seedData.regrowAfterHarvest)
        {
            // 재성장 모드 진입/유지
            harvestedOnce = true;
            regrowProgress = 0;       // 다음 사이클 시작
            UpdateVisual();
        }
        else
        {
            // 단발성: Soil에 칸 비우기 통보 후 제거
            if (soil) soil.ClearCropCell(cell);
            Destroy(gameObject);
        }
        return true;
    }

    // == 내부 편의 메서드 ================================================

    bool IsRegrowing()
    {
        return seedData && seedData.regrowAfterHarvest && harvestedOnce && !IsMature();
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void UpdateVisual()
    {
        if (!sr || growthSprites == null || growthSprites.Count == 0)
            return;

        int N = growthSprites.Count;
        int index = 0;

        if (!harvestedOnce)
        {
            float denom = Mathf.Max(1, seedData ? seedData.growthDays : 1);
            float t = Mathf.Clamp01(daysProgress / denom);
            index = Mathf.RoundToInt(t * (N - 1));
        }
        else if (seedData && seedData.regrowAfterHarvest)
        {
            float denom = Mathf.Max(1, seedData.regrowDays);
            float t = Mathf.Clamp01(regrowProgress / denom);
            index = Mathf.RoundToInt(t * (N - 1));
        }

        index = Mathf.Clamp(index, 0, N - 1);
        sr.sprite = growthSprites[index];
    }

    // === 선택: 디버그/보조 접근자 =======================================
    public int GetDaysProgress() => daysProgress;
    public int GetRegrowProgress() => regrowProgress;
    public bool HasBeenHarvestedOnce() => harvestedOnce;
    public void SetHarvestedOnce(bool v)
    {
        harvestedOnce = v;
        UpdateVisual();
    }
}
