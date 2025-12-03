using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Seed", menuName = "Inventory/Seed")]
public class Seeds : Item
{
    public GameObject cropPrefab;

    public int growthDays = 4;
    public Item harvestItem;
    public bool regrowAfterHarvest;
    public int regrowDays = 2;

    public Season[] growSeasons;

    public bool CanGrowInSeason(Season season)
    {
        if (growSeasons == null || growSeasons.Length == 0)
            return true; // 비워두면 모든 계절 가능

        foreach (var s in growSeasons)
        {
            if (s == season) return true;
        }
        return false;
    }
}
