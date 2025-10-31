using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;
    public int maxStack = 99;

    public bool canSell = true;
    public int sellPrice = 10;

    public GameObject worldPrefab;
}


