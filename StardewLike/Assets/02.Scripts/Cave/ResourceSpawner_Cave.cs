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

    // ================== 디버그 / 상태 확인 ==================
    [TitleGroup("Runtime Info")]
    [ShowInInspector, ReadOnly, LabelText("Current Cave Index")]
    private int currentCaveIndex = -1;

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
        // 이번 방문에서 이전에 스폰된 자원(돌/광석/사다리 등) 싹 지우기
        ClearSpawnedResources();

        // CaveIndex 업데이트
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

        // 타일맵 활성/비활성 (현재 CaveIndex 것만 켜기)
        for (int i = 0; i < caveGroundTilemaps.Length; i++)
        {
            if (caveGroundTilemaps[i] != null)
                caveGroundTilemaps[i].gameObject.SetActive(i == currentCaveIndex);
        }

        Tilemap selectedMap = caveGroundTilemaps[currentCaveIndex];

        // 이 Cave에서 자원 랜덤 스폰
        GenerateRandom(selectedMap);
    }

    void GenerateRandom(Tilemap map)
    {
        if (map == null) return;

        BoundsInt bounds = map.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!map.HasTile(pos))
                continue;

            // 이 칸에 자원을 둘지 말지
            if (Random.value >= spawnProbability)
                continue;

            // 돌 vs 광석 결정
            GameObject prefab = (Random.value < oreRatio) ? orePrefab : rockPrefab;
            if (!prefab) continue;

            Vector3 worldPos = map.GetCellCenterWorld(pos);
            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }
    }
}
