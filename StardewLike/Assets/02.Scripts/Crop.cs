using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    public Seeds seedData;
    public Vector3Int cell;
    private SoilTilemapController soilTilemapController;

    public List<Sprite> growthSprites;
    SpriteRenderer spriteRenderer;

    int daysProgress;
    bool wateredToday;
    bool harvestedOnce;
    int regrowProgress;

    public void Init(SoilTilemapController soilTilemapController, Vector3Int cell, Seeds data)
    {
        this.soilTilemapController = soilTilemapController;
        this.cell = cell;
        this.seedData = data;

        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    public void SetWateredToday() => wateredToday = true;

    public void OnNewDay()
    {
        if (!IsRegrowing())
        {
            if (wateredToday && daysProgress < seedData.growthDays)
            {
                daysProgress++;
            }
        }
        else
        {
            if (wateredToday && regrowProgress < seedData.regrowDays)
            {
                regrowProgress++;
            }
        }

        wateredToday = false;

        UpdateVisual();
    }

    bool IsRegrowing() => seedData.regrowAfterHarvest && harvestedOnce && !IsMature();

    public bool IsMature()
    {
        // 최초 성장: growthDays 충족이면 성숙
        if (!harvestedOnce) return daysProgress >= seedData.growthDays;

        // 재성장 상태면 regrowProgress 기준
        if (seedData.regrowAfterHarvest) return regrowProgress >= seedData.regrowDays;

        return false;
    }

    public bool TryHarvest(out Item itemOut)
    {
        itemOut = null;
        if (!IsMature()) return false;

        itemOut = seedData.harvestItem;

        if (seedData.regrowAfterHarvest)
        {
            // 재성장 시작
            harvestedOnce = true;
            regrowProgress = 0;
            // 재성장이면 오브젝트 유지, 상태만 초기화
            UpdateVisual();
        }
        else
        {
            // 단발성 작물: Soil에 알려서 칸 비우고 파괴
            soilTilemapController.ClearCropCell(cell);
            Destroy(gameObject);
        }
        return true;
    }
    void UpdateVisual()
    {
        if (!spriteRenderer || growthSprites == null || growthSprites.Count == 0) return;

        int stage = 0;

        if (!harvestedOnce)
        {
            // 0 ~ growthDays → 0 ~ (growthSprites.Count-1) 매핑
            float t = Mathf.Clamp01(seedData.growthDays <= 0 ? 1f : (daysProgress / (float)seedData.growthDays));
            stage = Mathf.RoundToInt(t * (growthSprites.Count - 1));
        }
        else if (seedData.regrowAfterHarvest)
        {
            // 재성장 구간: 0 ~ regrowDays → 0 ~ (growthSprites.Count-1) 매핑
            float t = Mathf.Clamp01(seedData.regrowDays <= 0 ? 1f : (regrowProgress / (float)seedData.regrowDays));
            stage = Mathf.RoundToInt(t * (growthSprites.Count - 1));
        }

        stage = Mathf.Clamp(stage, 0, growthSprites.Count - 1);
        spriteRenderer.sprite = growthSprites[stage];
    }
}
