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

        var wanted = NextSpawnPointId.Trim();
        NextSpawnPointId = null;

        var active = SceneManager.GetActiveScene();

        var allPoints = GameObject.FindObjectsOfType<SpawnPoint>(true)
                                  .Where(p => p && p.gameObject.scene == active)
                                  .ToList();

        if (allPoints.Count == 0)
        {
            Debug.LogWarning($"[PlayerSpawnManager] No SpawnPoint in active scene '{active.name}'.");
            return;
        }

        Debug.Log($"[PlayerSpawnManager] SpawnPoints in '{active.name}': " +
                  string.Join(", ", allPoints.Select(p => p.spawnId)));

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
