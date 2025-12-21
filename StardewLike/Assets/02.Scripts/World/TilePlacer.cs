using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    public Camera cam;
    public GridLayout grid;

    public Tilemap fenceTilemap;
    public Tilemap pathTilemap;
    public Tilemap decorTilemap;
    [SerializeField] TileBase fenceTile;
    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (grid == null) grid = FindObjectOfType<GridLayout>();
    }
    void Start()
    {
        RestoreFencesFromState();
    }

    public bool TryPlace(PlaceableTileData item, Vector3 mouseScreenPos)
    {
        if (item == null || item.tile == null) return false;

        Tilemap tm = GetTargetTilemap(item.target);
        if (tm == null || cam == null || grid == null) return false;

        Vector3 world = cam.ScreenToWorldPoint(mouseScreenPos);
        world.z = 0f;

        Vector3Int cell = grid.WorldToCell(world);

        // 이미 타일이 있는지 체크
        var existing = tm.GetTile(cell);
        if (existing != null && !item.allowReplace)
            return false;

        tm.SetTile(cell, item.tile);
        tm.RefreshTile(cell);
        return true;
    }

    public bool TryRemove(PlaceableTileData item, Vector3 mouseScreenPos)
    {
        if (item == null) return false;

        Tilemap tm = GetTargetTilemap(item.target);
        if (tm == null || cam == null || grid == null) return false;

        Vector3 world = cam.ScreenToWorldPoint(mouseScreenPos);
        world.z = 0f;

        Vector3Int cell = grid.WorldToCell(world);

        if (tm.GetTile(cell) == null) return false;

        tm.SetTile(cell, null);
        tm.RefreshTile(cell);
        return true;
    }

    Tilemap GetTargetTilemap(PlaceTarget target)
    {
        return target switch
        {
            PlaceTarget.Fence => fenceTilemap,
            PlaceTarget.Path => pathTilemap,
            PlaceTarget.Decor => decorTilemap,
            _ => null
        };
    }
    public bool TryPlaceAtCell(PlaceableTileData item, Vector3Int cell)
    {
        if (item == null || item.tile == null) return false;

        Tilemap tm = GetTargetTilemap(item.target);
        if (tm == null) return false;

        var existing = tm.GetTile(cell);
        if (existing != null && !item.allowReplace)
            return false;

        tm.SetTile(cell, item.tile);
        tm.RefreshTile(cell);

        if (item.target == PlaceTarget.Fence && WorldStateManager.Instance != null)
        {
            string sceneName = tm.gameObject.scene.name;
            WorldStateManager.Instance.AddFenceCell(sceneName, cell);
        }

        return true;

    }
    public bool TryRemoveAtCell(PlaceableTileData item, Vector3Int cell)
    {
        if (item == null) return false;

        Tilemap tm = GetTargetTilemap(item.target);
        if (tm == null) return false;

        if (tm.GetTile(cell) == null) return false;

        tm.SetTile(cell, null);
        tm.RefreshTile(cell);

        if (item.target == PlaceTarget.Fence && WorldStateManager.Instance != null)
        {
            string sceneName = tm.gameObject.scene.name;
            WorldStateManager.Instance.RemoveFenceCell(sceneName, cell);
        }

        return true;
    }

    public void RestoreFencesFromState()
    {
        if (WorldStateManager.Instance == null) return;
        if (fenceTilemap == null || fenceTile == null) return;

        string scene = fenceTilemap.gameObject.scene.name;
        var st = WorldStateManager.Instance.GetOrCreate(scene);

        fenceTilemap.ClearAllTiles();
        foreach (var cell in st.fences)
            fenceTilemap.SetTile(cell, fenceTile);

        fenceTilemap.RefreshAllTiles();
    }
}
