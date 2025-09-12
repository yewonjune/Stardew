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

        RefreshAround(soilTilemap, cell);
        return true;
    }

    void RefreshAround(Tilemap tilemap, Vector3Int center)
    {
        for(int y=-1; y<=1; y++)
        {
            for(int x=-1; x<=1; x++)
            {
                Vector3Int pos = center + new Vector3Int(x, y, 0);
                tilemap.RefreshTile(pos);
            }
        }
    }
}
