using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance;
    public static string NextSpawnPointId;

    void Awake() { Instance = this; }

    public void PlacePlayerAtSpawn()
    {
        var points = GameObject.FindObjectsOfType<SpawnPoint>();
        foreach (var p in points)
        {
            if (p.spawnId == NextSpawnPointId)
            {
                var player = GameObject.FindGameObjectWithTag("Player").transform;
                player.position = p.transform.position;
                return;
            }
        }
    }
}
