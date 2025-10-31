using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopItemEntry
{
    public Item item;
    public int buyPrice = 50;
    //public int sellPrice = 20;
    public int stock = -1;
}

[CreateAssetMenu(fileName = "ShopCatalog", menuName = "Shop/Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    public List<ShopItemEntry> shopItemEntries = new();
}
