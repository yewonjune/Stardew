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
}
