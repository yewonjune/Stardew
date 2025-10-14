using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SoilTilemapController : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap soilTilemap;
    public Tilemap wateredTilemap;

    public TileBase tilledSoilTile;
    public TileBase wateredTile;

    HashSet<Vector3Int> tilled = new HashSet<Vector3Int>();
    HashSet<Vector3Int> watered = new HashSet<Vector3Int>();
    Dictionary<Vector3Int, Crop> plantedCrop = new Dictionary<Vector3Int, Crop>();

    public bool TryTillAtWorldPos(Vector3 worldPos)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);

        if(!groundTilemap.HasTile(cell))
            return false;

        if (soilTilemap.HasTile(cell))
            return false;

        soilTilemap.SetTile(cell, tilledSoilTile);

        tilled.Add(cell);

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

    public bool TryWaterAtWorldPos(Vector3 worldPos)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        return TryWaterAtCell(cell);
    }

    public bool TryWaterAtCell(Vector3Int cell)
    {
        if (!tilled.Contains(cell)) return false;
        if (watered.Contains(cell)) return false;

        watered.Add(cell);
        wateredTilemap.SetTile(cell, wateredTile);
        RefreshAround(wateredTilemap, cell);

        if (plantedCrop.TryGetValue(cell, out var crop))
            crop.SetWateredToday();

        return true;
    }

    public bool TryPlantAtWorldPos(Vector3 worldPos, Seeds seedData)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        return TryPlantAtCell(cell, seedData);
    }

    public bool TryPlantAtCell(Vector3Int cell, Seeds seedData, List<Sprite> growthSpritesOverride = null)
    {
        if (!tilled.Contains(cell)) return false;          // 갈린 땅만 심기 가능
        if (plantedCrop.ContainsKey(cell)) return false;   // 이미 작물 있음

        Vector3 world = groundTilemap.GetCellCenterWorld(cell);
        var go = Instantiate(seedData.cropPrefab, world, Quaternion.identity);
        var crop = go.GetComponent<Crop>();
        if (!crop) { Destroy(go); return false; }

        if (growthSpritesOverride != null && growthSpritesOverride.Count > 0)
            crop.growthSprites = growthSpritesOverride;

        crop.Init(this, cell, seedData);

        plantedCrop[cell] = crop;

        if (watered.Contains(cell))
            crop.SetWateredToday();

        return true;
    }

    public bool TryHarvestAtWorldPos(Vector3 worldPos, out Item harvestedItem)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        return TryHarvestAtCell(cell, out harvestedItem);
    }

    public bool TryHarvestAtCell(Vector3Int cell, out Item harvestedItem)
    {
        harvestedItem = null;

        if (!plantedCrop.TryGetValue(cell, out var crop)) return false;

        if (crop.TryHarvest(out harvestedItem))
        {
            if (harvestedItem != null && !crop.seedData.regrowAfterHarvest)
                plantedCrop.Remove(cell);
            return true;
        }

        return false;
    }

    public void NewDay()
    {
        Debug.Log("[Soil] NewDay 호출됨: 물표시 초기화 + 작물 하루 경과 처리");

        watered.Clear();
        if (wateredTilemap) wateredTilemap.ClearAllTiles();
        foreach (var kv in plantedCrop) kv.Value.OnNewDay();
    }

    public Vector3Int WorldToCell(Vector3 world) => groundTilemap.WorldToCell(world);
    public Vector3 CellToWorldCenter(Vector3Int cell) => groundTilemap.GetCellCenterWorld(cell);

    public bool IsTilledCell(Vector3Int cell) => tilled.Contains(cell);
    public bool IsWateredCell(Vector3Int cell) => watered.Contains(cell);
    public bool HasCropAtCell(Vector3Int cell) => plantedCrop.ContainsKey(cell);

    public Crop TryGetCrop(Vector3Int cell)
    {
        plantedCrop.TryGetValue(cell, out var crop);
        return crop;
    }

    public void ClearCropCell(Vector3Int cell)
    {
        plantedCrop.Remove(cell);
    }
}
