using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFishCatalog", menuName = "Fishing/Fish Catalog")]
public class FishCatalog : ScriptableObject
{
    public List<FishData> fishes = new();

    // 반응 시간(reactionTime)에 따라 확률이 달라질 수도 있음
    public FishData PickRandomFish(float reactionTimeSeconds)
    {
        if (fishes == null || fishes.Count == 0)
        {
            Debug.LogWarning("FishCatalog에 등록된 FishData가 없습니다!");
            return null;
        }

        float totalWeight = 0f;

        // 전체 가중치 합 계산
        foreach (var fish in fishes)
        {
            if (fish == null) continue;

            // 짧은 반응시간(예: 0.5초)일수록 reactionBonus 영향이 커짐
            float bonus = fish.reactionBonus > 0f
                ? Mathf.Lerp(0f, fish.reactionBonus, Mathf.Clamp01(1f / Mathf.Max(0.01f, reactionTimeSeconds)))
                : 0f;

            totalWeight += Mathf.Max(0f, fish.weight + bonus);
        }

        // 랜덤 선택
        float randomPoint = Random.value * totalWeight;

        foreach (var fish in fishes)
        {
            if (fish == null) continue;

            float bonus = fish.reactionBonus > 0f
                ? Mathf.Lerp(0f, fish.reactionBonus, Mathf.Clamp01(1f / Mathf.Max(0.01f, reactionTimeSeconds)))
                : 0f;

            float currentWeight = Mathf.Max(0f, fish.weight + bonus);

            if (randomPoint < currentWeight)
                return fish;

            randomPoint -= currentWeight;
        }

        // 혹시나 실패 시 랜덤 반환
        return fishes[Random.Range(0, fishes.Count)];
    }
}
