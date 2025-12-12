using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject enemyPrefab;
        public int minCount = 1;
        public int maxCount = 5;
    }

    [Header("이번 층에 등장할 몬스터 목록")]
    public EnemySpawnEntry[] enemyGroups;

    [Header("스폰 규칙")]
    public float minDistanceFromPlayer = 3f;
    public int maxRetryPerEnemy = 20;

    Transform player;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    public void SpawnOnTilemap(Tilemap groundTilemap)
    {
        if (groundTilemap == null)
        {
            Debug.LogWarning("[EnemySpawner] groundTilemap 이 null 입니다.");
            return;
        }

        if (enemyGroups == null || enemyGroups.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] enemyGroups 가 비어 있음.");
            return;
        }

        // 이 타일맵에서 스폰 가능한 위치들 수집
        List<Vector3> validPositions = CollectValidPositions(groundTilemap);

        if (validPositions.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] 스폰 가능한 위치가 없음.");
            return;
        }

        // 몬스터 종류별로 쫙 뿌리기
        foreach (var group in enemyGroups)
        {
            if (group.enemyPrefab == null) continue;

            int count = Random.Range(group.minCount, group.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition(validPositions);
                int retry = 0;

                while (player != null &&
                       Vector2.Distance(spawnPos, player.position) < minDistanceFromPlayer &&
                       retry < maxRetryPerEnemy)
                {
                    spawnPos = GetRandomSpawnPosition(validPositions);
                    retry++;
                }

                Instantiate(group.enemyPrefab, spawnPos, Quaternion.identity, transform);
            }
        }
    }

    List<Vector3> CollectValidPositions(Tilemap groundTilemap)
    {
        List<Vector3> result = new List<Vector3>();
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = groundTilemap.GetTile(cellPos);
            if (tile == null) continue;

            Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
            result.Add(worldPos);
        }

        return result;
    }

    Vector3 GetRandomSpawnPosition(List<Vector3> list)
    {
        int index = Random.Range(0, list.Count);
        return list[index];
    }

    public void ClearSpawnedEnemies()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
