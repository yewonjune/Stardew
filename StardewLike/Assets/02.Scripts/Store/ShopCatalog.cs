using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopItemEntry
{
    public Item item;
    public int buyPrice = 50;
    public int stock = -1;

    public bool IsAvailableThisSeason(Season currentSeason)
    {
        if (item is Seeds seed)
        {
            return seed.CanGrowInSeason(currentSeason);
        }

        return true;
    }
}

[CreateAssetMenu(fileName = "ShopCatalog", menuName = "Shop/Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    public List<ShopItemEntry> shopItemEntries = new();
}
