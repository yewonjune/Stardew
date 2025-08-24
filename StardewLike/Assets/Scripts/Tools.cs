using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/Tool")]
public class Tools : Item
{
    public ToolType toolType;
    public int power;                           // ±¸¸®, Ã¶, ±Ý, ÀÌ¸®µã ...
}

public enum ToolType
{
    Hoe,                // È£¹Ì
    Pickaxe,            // °î±ªÀÌ
    Axe,                // µµ³¢
    WateringCan,        // ¹°»Ñ¸®°³
    Scythe,             // ³´
    Sword               // °Ë
}