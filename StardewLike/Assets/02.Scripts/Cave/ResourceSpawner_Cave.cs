using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class ResourceSpawner_Cave : MonoBehaviour
{
    // ================== Cave 설정 ==================
    [Title("Cave Resource Spawner")]
    [InfoBox("CaveGround 타일맵들을 0~3 순서(Cave1~4)로 넣어주세요!\n" +
             "Player가 입장한 CaveIndex만 골라서 그 안에만 자원을 스폰스폰")]
    [LabelText("Cave Ground Tilemaps")]
    [Required]
    public Tilemap[] caveGroundTilemaps;

    // ================== 프리팹 ==================
    [TitleGroup("Prefabs")]
    [HorizontalGroup("Prefabs/Split", Width = 0.5f)]
    [BoxGroup("Prefabs/Split/Rock"), LabelText("Rock Prefab"), Required]
    public GameObject rockPrefab;

    [HorizontalGroup("Prefabs/Split", Width = 0.5f)]
    [BoxGroup("Prefabs/Split/Ore"), LabelText("Ore Prefab"), Required]
    public GameObject orePrefab;

    // ================== 스폰 설정 ==================
    [TitleGroup("Spawn Settings")]
    [BoxGroup("Spawn Settings/General")]
    [LabelText("Spawn Probability"), Range(0f, 1f)]
    public float spawnProbability = 0.05f;

    [BoxGroup("Spawn Settings/General")]
    [LabelText("Resource Layer")]
    public LayerMask resourceLayer;

    // ================== 디버그 / 상태 확인 ==================
    [TitleGroup("Runtime Info")]
    [ShowInInspector, ReadOnly, LabelText("Current Cave Index")]
    private int currentCaveIndex = -1;

    [ShowInInspector, ReadOnly, LabelText("WorldState Key")]
    private string caveKey = "(not initialized)";
    private bool initialized = false;

    // --------------------------------------------------------

    void Start()
    {
        if (CaveStateManager.CurrentCaveIndex >= 0)
        {
            SpawnForCurrentCave();
        }
    }

    [Button("Spawn For Current Cave")]
    public void SpawnForCurrentCave()
    {
        if (initialized) return; // 두 번 스폰 방지
        initialized = true;

        currentCaveIndex = CaveStateManager.CurrentCaveIndex;

        if (caveGroundTilemaps == null || caveGroundTilemaps.Length == 0)
        {
            Debug.LogWarning("[CaveSpawner] caveGroundTilemaps가 비었습니다.", this);
            return;
        }

        if (currentCaveIndex < 0 || currentCaveIndex >= caveGroundTilemaps.Length)
        {
            Debug.LogWarning($"[CaveSpawner] 잘못된 CaveIndex: {currentCaveIndex}", this);
            return;
        }

        Tilemap selectedMap = caveGroundTilemaps[currentCaveIndex];

        caveKey = $"{gameObject.scene.name}_Cave{currentCaveIndex}";

        var state = WorldStateManager.Instance.GetOrCreate(caveKey);

        if (!state.initialSpawnDone)
        {
            GenerateAndRegister(state, selectedMap, caveKey);
            WorldStateManager.Instance.MarkInitialSpawnDone(caveKey);
        }

        RestoreFromState(state);
    }

    void GenerateAndRegister(SceneState state, Tilemap map, string key)
    {
        BoundsInt bounds = map.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!map.HasTile(pos))
                continue;

            if (Random.value >= spawnProbability)
                continue;

            GameObject prefab = (Random.value < 0.5f) ? rockPrefab : orePrefab;
            if (!prefab) continue;

            Vector3 worldPos = map.GetCellCenterWorld(pos);
            Instantiate(prefab, worldPos, Quaternion.identity, transform);

            WorldStateManager.Instance.AddResource(
                key,
                new ResourceSave
                {
                    prefabId = prefab.name,
                    position = worldPos,
                    harvestedOrRemoved = false
                });
        }
    }

    void RestoreFromState(SceneState state)
    {
        foreach (ResourceSave r in state.resources)
        {
            if (r.harvestedOrRemoved)
                continue;

            // 이미 뭔가가 그 자리에 있으면 스킵
            Collider2D hit = Physics2D.OverlapPoint(r.position, resourceLayer);
            if (hit != null)
                continue;

            GameObject prefab = ChoosePrefab(r.prefabId);
            if (prefab != null)
                Instantiate(prefab, r.position, Quaternion.identity, transform);
        }
    }

    GameObject ChoosePrefab(string id)
    {
        if (rockPrefab && rockPrefab.name == id)
            return rockPrefab;

        if (orePrefab && orePrefab.name == id)
            return orePrefab;

        return null;
    }
}
