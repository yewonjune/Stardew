using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlaceTarget
{
    Fence,
    Path,
    Decor
}


[CreateAssetMenu(menuName ="Game/Placeable Tile")]
public class PlaceableTileData : Item
{
    public PlaceTarget target;
    public TileBase tile;
    public bool allowReplace = false;
}
