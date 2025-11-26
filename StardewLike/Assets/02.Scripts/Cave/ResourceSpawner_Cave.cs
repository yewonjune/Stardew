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
             "Player가 입장한 CaveIndex만 골라서 그 인덱스의 타일맵만 활성화 + 자원 랜덤 스폰")]
    [LabelText("Cave Ground Tilemaps")]
    [Required]
    public Tilemap[] caveGroundTilemaps;
    public GameObject[] caveRoots;

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
    [LabelText("Ore Ratio (0~1)"), Range(0f, 1f)]
    public float oreRatio = 0.1f;

    // ================== 상태 확인 ==================
    [TitleGroup("Runtime Info")]
    [ShowInInspector, ReadOnly, LabelText("Current Cave Index")]
    private int currentCaveIndex = -1;

    // ================== Enemy 스폰 ==================
    [TitleGroup("Enemy Settings")]
    [LabelText("Enemy Spawner (옵션)")]
    public EnemySpawner enemySpawner;

    // --------------------------------------------------------

    void Start()
    {
        if (CaveStateManager.CurrentCaveIndex >= 0)
        {
            SpawnForCurrentCave();
        }
    }

    void ClearSpawnedResources()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    [Button("Spawn For Current Cave")]
    public void SpawnForCurrentCave()
    {
        ClearSpawnedResources();
        enemySpawner.ClearSpawnedEnemies();

        currentCaveIndex = CaveStateManager.CurrentCaveIndex;

        if (caveGroundTilemaps == null || caveGroundTilemaps.Length == 0)
        {
            return;
        }

        if (currentCaveIndex < 0 || currentCaveIndex >= caveGroundTilemaps.Length)
        {
            return;
        }

        for (int i = 0; i < caveGroundTilemaps.Length; i++)
        {
            bool isActive = (i == currentCaveIndex);

            if (caveRoots != null && i < caveRoots.Length && caveRoots[i] != null)
            {
                caveRoots[i].SetActive(isActive);
            }
        }


        Tilemap selectedMap = caveGroundTilemaps[currentCaveIndex];

        GenerateRandom(selectedMap);

        if (enemySpawner != null && selectedMap != null)
        {
            enemySpawner.SpawnOnTilemap(selectedMap);
        }
    }

    void GenerateRandom(Tilemap map)
    {
        if (map == null) return;

        BoundsInt bounds = map.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!map.HasTile(pos))
                continue;

            if (Random.value >= spawnProbability)
                continue;

            GameObject prefab = (Random.value < oreRatio) ? orePrefab : rockPrefab;
            if (!prefab) continue;

            Vector3 worldPos = map.GetCellCenterWorld(pos);
            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }
    }
}
