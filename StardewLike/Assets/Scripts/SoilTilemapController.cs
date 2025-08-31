using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SoilTilemapController : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap soilTilemap;
    public TileBase tilledSoilTile;

    public bool TryTillAtWorldPos(Vector3 worldPos)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);

        if(!groundTilemap.HasTile(cell))
            return false;

        if (soilTilemap.HasTile(cell))
            return false;

        soilTilemap.SetTile(cell, tilledSoilTile);
        return true;
    }
}
