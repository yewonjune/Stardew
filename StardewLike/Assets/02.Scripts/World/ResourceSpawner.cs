using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap noSpawnTilemap;

    public GameObject rockPrefab;
    public GameObject stumpPrefab;
    public GameObject[] treePrefabs;

    public float spawnProbability = 0.1f;
    public LayerMask resourceLayer;

    // Start is called before the first frame update
    void Start()
    {
        string sceneName = gameObject.scene.name;
        SceneState state = WorldStateManager.Instance.GetOrCreate(sceneName);

        if (!state.initialSpawnDone)
        {
            GenerateAndRegister(state);
            WorldStateManager.Instance.MarkInitialSpawnDone(sceneName);
        }

        RestoreFromState(state);
    }

    void GenerateAndRegister(SceneState state)
    {
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(pos))
                continue;

            if (noSpawnTilemap != null && noSpawnTilemap.HasTile(pos))
                continue;

            if (Random.value >= spawnProbability)
                continue;

            float r = Random.value;
            GameObject resourcePrefab = null;

            if (r < 0.5f)      // 50%
            {
                if (treePrefabs != null && treePrefabs.Length > 0)
                {
                    int idx = Random.Range(0, treePrefabs.Length);
                    resourcePrefab = treePrefabs[idx];
                }

            }
            else if (r < 0.8f)
            {
                resourcePrefab = rockPrefab;      // 30%
            }
            else
            {
                resourcePrefab = stumpPrefab;     // 20%
            }

            if (resourcePrefab == null) continue;

            Vector3 worldPos = groundTilemap.GetCellCenterWorld(pos);
            GameObject go = Instantiate(resourcePrefab, worldPos, Quaternion.identity, transform);
            string id = resourcePrefab.name;

            WorldStateManager.Instance.AddResource(
                gameObject.scene.name,
                new ResourceSave
                {
                    prefabId = id,
                    position = worldPos,
                    harvestedOrRemoved = false
                });
        }
    }

    void RestoreFromState(SceneState state)
    {
        foreach (ResourceSave r in state.resources)
        {
            var cell = groundTilemap.WorldToCell(r.position);
            if (noSpawnTilemap != null && noSpawnTilemap.HasTile(cell))
                continue;

            if (r.harvestedOrRemoved)
                continue;

            Collider2D hit = Physics2D.OverlapPoint(r.position, resourceLayer);
            if (hit != null)
                continue;

            GameObject prefab = ChoosePrefabById(r.prefabId);
            if (prefab != null)
                Instantiate(prefab, r.position, Quaternion.identity, transform);
        }
    }

    GameObject ChoosePrefabById(string id)
    {
        if (rockPrefab != null && rockPrefab.name == id)
            return rockPrefab;

        if (stumpPrefab != null && stumpPrefab.name == id)
            return stumpPrefab;

        foreach (var t in treePrefabs) if (t.name == id) return t;

        return null;
    }
}
