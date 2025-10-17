using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    public Tilemap groundTilemap;

    public GameObject rockPrefab;
    public GameObject stumpPrefab;

    public float spawnProbability = 0.05f;
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

            if (Random.value >= spawnProbability)
                continue;

            GameObject resoucePrefab = (Random.value < 0.5f) ? rockPrefab : stumpPrefab;

            Vector3 worldPos = groundTilemap.GetCellCenterWorld(pos);
            GameObject go = Instantiate(resoucePrefab, worldPos, Quaternion.identity, transform);
            string id = resoucePrefab.name; // 간단히 프리팹 이름으로 식별

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
        // 이미 기록된 자원들을 다시 생성(아직 씬에 없고, 제거되지 않은 것만)
        foreach (ResourceSave r in state.resources)
        {
            if (r.harvestedOrRemoved)
                continue;

            // 씬에 동일 위치에 이미 있는지 체크
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

        return null;
    }
}
