using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SoilTilemapController : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap soilTilemap;
    public Tilemap wateredTilemap;

    [Header("Tiles")]
    public TileBase tilledSoilTile;
    public TileBase wateredTile;

    // 런타임 상태
    private readonly HashSet<Vector3Int> tilled = new();
    private readonly HashSet<Vector3Int> watered = new();
    private readonly Dictionary<Vector3Int, Crop> plantedCrop = new();

    private string SceneName => gameObject.scene.name;

    private void Start()
    {
        RestoreFromState();
    }

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

        WorldStateManager worldStateManager = WorldStateManager.Instance;
        if (worldStateManager != null) 
            worldStateManager.SetTilled(SceneName, cell, true);

        return true;
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

        WorldStateManager worldStateManager = WorldStateManager.Instance;
        if (worldStateManager != null) 
            worldStateManager.SetWatered(SceneName, cell, true);

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

        WorldStateManager worldStateManager = WorldStateManager.Instance;
        if (worldStateManager != null)
        {
            string cropId = seedData != null ? seedData.name : "UnknownCrop";
            worldStateManager.UpsertCrop(SceneName, new CropSave
            {
                prefabId = cropId,  // 프로젝트에 맞는 식별자 사용
                cell = cell,
                growthStage = crop.CurrentStage,
                isWateredToday = watered.Contains(cell),
                //harvestedOnce = crop.HasBeenHarvestedOnce()
            });
        }

        return true;
    }

    public bool TryHarvestAtCell(Vector3Int cell, out Item harvestedItem)
    {
        harvestedItem = null;
        //Crop crop;

        if (!plantedCrop.TryGetValue(cell, out var crop)) return false;

        if (crop.TryHarvest(out harvestedItem))
        {
            WorldStateManager worldStateManager = WorldStateManager.Instance;

            if (harvestedItem != null && !crop.seedData.regrowAfterHarvest)
            {
                plantedCrop.Remove(cell);

                if (worldStateManager != null) 
                    worldStateManager.RemoveCrop(SceneName, cell);
            }
            else
            {
                if (worldStateManager != null)
                {
                    string cropId = crop.seedData != null ? crop.seedData.name : "UnknownCrop";
                    worldStateManager.UpsertCrop(SceneName, new CropSave
                    {
                        prefabId = cropId,
                        cell = cell,
                        growthStage = crop.CurrentStage,
                        isWateredToday = watered.Contains(cell),
                        //harvestedOnce = crop.HasBeenHarvestedOnce()
                    });
                }
            }
            return true;
        }

        return false;
    }
    public bool TryHarvestAtWorldPos(Vector3 worldPos, out Item harvestedItem)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        return TryHarvestAtCell(cell, out harvestedItem);
    }
    public void NewDay()
    {
        watered.Clear();
        if (wateredTilemap != null) wateredTilemap.ClearAllTiles();

        foreach (KeyValuePair<Vector3Int, Crop> kv in plantedCrop)
        {
            kv.Value.OnNewDay();

            WorldStateManager worldStateManager = WorldStateManager.Instance;
            if (worldStateManager != null)
            {
                Crop c = kv.Value;
                string cropId = c.seedData != null ? c.seedData.name : "UnknownCrop";
                worldStateManager.UpsertCrop(SceneName, new CropSave
                {
                    prefabId = cropId,
                    cell = kv.Key,
                    growthStage = c.CurrentStage,
                    isWateredToday = false,
                    //harvestedOnce = c.HasBeenHarvestedOnce()
                });
            }
        }

        WorldStateManager StateManager = WorldStateManager.Instance;
        if (StateManager != null)
        {
            // 간단히: 씬 상태의 watered 집합을 비움
            SceneState st = StateManager.GetOrCreate(SceneName);
            st.watered.Clear();
        }
    }

    public Vector3Int WorldToCell(Vector3 world) => groundTilemap.WorldToCell(world);
    public Vector3 CellToWorldCenter(Vector3Int cell) => groundTilemap.GetCellCenterWorld(cell);
    public bool IsTilledCell(Vector3Int cell) => tilled.Contains(cell);
    public bool IsWateredCell(Vector3Int cell) => watered.Contains(cell);
    public bool HasCropAtCell(Vector3Int cell) => plantedCrop.ContainsKey(cell);
    public Crop TryGetCrop(Vector3Int cell) { plantedCrop.TryGetValue(cell, out var c); return c; }
    public void ClearCropCell(Vector3Int cell) { plantedCrop.Remove(cell); }
    void RefreshAround(Tilemap tilemap, Vector3Int center)
    {
        if (!tilemap) return;

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector3Int pos = center + new Vector3Int(x, y, 0);
                tilemap.RefreshTile(pos);
            }
        }
    }

    private void RestoreFromState()
    {
        WorldStateManager worldStateManager = WorldStateManager.Instance;
        if (worldStateManager == null)
        {
            Debug.LogWarning("[Soil] WorldStateManager.Instance 가 없습니다. 복원을 생략합니다.");
            return;
        }

        SceneState st = worldStateManager.GetOrCreate(SceneName);

        // tilled 복원
        foreach (Vector3Int c in st.tilled)
        {
            soilTilemap.SetTile(c, tilledSoilTile);
            tilled.Add(c);
        }

        // watered 복원
        foreach (Vector3Int c in st.watered)
        {
            wateredTilemap.SetTile(c, wateredTile);
            watered.Add(c);
        }

        // crops 복원
        foreach (KeyValuePair<Vector3Int, CropSave> kv in st.crops)
        {
            Vector3Int cell = kv.Key;
            CropSave save = kv.Value;

            GameObject prefab = FindCropPrefabById(save.prefabId);
            if (prefab == null)
            {
                Debug.LogWarning($"[Soil] Crop prefab '{save.prefabId}' 를 찾을 수 없습니다.");
                continue;
            }

            Vector3 world = groundTilemap.GetCellCenterWorld(cell);
            GameObject go = Object.Instantiate(prefab, world, Quaternion.identity);
            Crop crop = go.GetComponent<Crop>();
            
            if (crop == null)
            {
                Object.Destroy(go);
                continue;
            }

            crop.Init(this, cell, crop.seedData); // seedData를 prefab에 붙여뒀다는 가정

            //if (save.harvestedOnce) crop.SetHarvestedOnce(true);
            crop.ForceSetStage(save.growthStage);
            if (save.isWateredToday)
            {
                crop.SetWateredToday();
            }

            plantedCrop[cell] = crop;
        }
    }

    private GameObject FindCropPrefabById(string prefabId)
    {
        if (string.IsNullOrEmpty(prefabId)) return null;
        // 예: Assets/Resources/Crops/Tomato.prefab
        return Resources.Load<GameObject>($"Crops/{prefabId}");
    }
}

