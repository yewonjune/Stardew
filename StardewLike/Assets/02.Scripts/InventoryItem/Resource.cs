using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Inventory/Resource")]
public class Resource : Item
{
    public ResourceType resourceType;

    public int sellPrice;
}

public enum ResourceType
{
    Rock,
    Tree,
    Log,
    Weed,
    Ore,
    Gem,
    Magic
}
