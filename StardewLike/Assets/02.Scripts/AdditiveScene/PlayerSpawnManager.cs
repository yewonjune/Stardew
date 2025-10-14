using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance;
    public static string NextSpawnPointId;

    void Awake() { Instance = this; }

    public void PlacePlayerAtSpawn()
    {
        if (string.IsNullOrWhiteSpace(NextSpawnPointId))
        {
            Debug.LogWarning("[PlayerSpawnManager] NextSpawnPointId is empty.");
            return;
        }

        // 공백/대소문자 실수 방지
        var wanted = NextSpawnPointId.Trim();
        NextSpawnPointId = null;

        var active = SceneManager.GetActiveScene();

        // 활성 씬에 '속한' SpawnPoint만 모두 수집(비활성 포함)
        var allPoints = GameObject.FindObjectsOfType<SpawnPoint>(true)
                                  .Where(p => p && p.gameObject.scene == active)
                                  .ToList();

        if (allPoints.Count == 0)
        {
            Debug.LogWarning($"[PlayerSpawnManager] No SpawnPoint in active scene '{active.name}'.");
            return;
        }

        // 씬 내 존재 목록을 로그로 출력(즉시 원인 파악)
        Debug.Log($"[PlayerSpawnManager] SpawnPoints in '{active.name}': " +
                  string.Join(", ", allPoints.Select(p => p.spawnId)));

        // 정확 일치(앞뒤 공백 제거, 대소문자 구분 없음 원하면 OrdinalIgnoreCase로)
        var target = allPoints.FirstOrDefault(p =>
            string.Equals(p.spawnId?.Trim(), wanted, System.StringComparison.Ordinal));

        if (!target)
        {
            Debug.LogWarning($"[PlayerSpawnManager] SpawnPoint '{wanted}' not found in active scene '{active.name}'.");
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!player)
        {
            Debug.LogError("[PlayerSpawnManager] Player (Tag=Player) not found.");
            return;
        }

        player.position = target.transform.position;
    }
}
