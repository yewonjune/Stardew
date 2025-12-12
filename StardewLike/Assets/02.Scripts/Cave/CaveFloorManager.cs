using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveFloorManager : MonoBehaviour
{
    [System.Serializable]
    public class StagePack
    {
        public GameObject stageRoot;
        public GameObject[] caveRoots;
        public Tilemap[] caveGroundTilemaps;
        public EnemySpawner.EnemySpawnEntry[] enemyGroups;
        public Transform[] caveSpawnPoints;

        public GameObject treasureRoot;
        public Tilemap treasureGroundTilemap;
        public Transform treasureSpawnPoint;
    }

    [System.Serializable]
    public class TreasureRoom
    {
        public int floor;
        public GameObject roomRoot;
        public Transform spawnPoint;
    }

    public TreasureRoom[] treasureRooms;

    public StagePack[] stagePack;

    public ResourceSpawner_Cave resourceSpawner;
    public EnemySpawner enemySpawner;

    public int caveCountPerStage = 4;


    public void EnterFloor(int floor)
    {
        CaveStateManager.CurrentFloor = floor;

        if (TryEnterTreasureRoom(floor))
            return;

        int stageIndex = GetStageIndex(floor);
        int caveIndex = Random.Range(0, caveCountPerStage);
        CaveStateManager.CurrentCaveIndex = caveIndex;

        ApplyStage(stageIndex, caveIndex);

        if (resourceSpawner != null)
        {
            resourceSpawner.spawnResources = true;
            resourceSpawner.spawnEnemies = true;
            resourceSpawner.SpawnForCurrentCave();
        }

        if (treasureRooms != null)
        {
            for (int i = 0; i < treasureRooms.Length; i++)
                if (treasureRooms[i].roomRoot) treasureRooms[i].roomRoot.SetActive(false);
        }
    }

    bool TryEnterTreasureRoom(int floor)
    {
        if (treasureRooms == null) return false;

        if (stagePack != null)
        {
            for (int i = 0; i < stagePack.Length; i++)
            {
                if (stagePack[i].stageRoot) stagePack[i].stageRoot.SetActive(false);
            }
        }

        for (int i = 0; i < treasureRooms.Length; i++)
        {
            if (treasureRooms[i].roomRoot) treasureRooms[i].roomRoot.SetActive(false);
        }

        TreasureRoom tr = null;
        for (int i = 0; i < treasureRooms.Length; i++)
        {
            if(treasureRooms[i].floor == floor)
            {
                tr = treasureRooms[i];
                break;
            }
        }
        if(tr == null || tr.roomRoot == null) return false;

        tr.roomRoot.SetActive(true);

        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null && tr.spawnPoint != null)
            player.position = tr.spawnPoint.position;

        if (resourceSpawner != null)
        {
            resourceSpawner.spawnResources = false;
            resourceSpawner.spawnEnemies = false;
        }
        if (enemySpawner != null)
        {
            enemySpawner.enemyGroups = null;
            enemySpawner.ClearSpawnedEnemies();
        }

        return true;
    }

    int GetStageIndex(int floor)
    {
        if(floor <= 2) return 0;
        if (floor <= 4) return 1;
        return 2;
    }

    void ApplyStage(int stageIndex, int caveIndex)
    {
        if (stagePack == null || stagePack.Length == 0) return;
        stageIndex = Mathf.Clamp(stageIndex, 0, stagePack.Length - 1);
        caveIndex = Mathf.Clamp(caveIndex, 0, caveCountPerStage - 1);

        if (CaveSpawnManager.Instance != null)
            CaveSpawnManager.Instance.SetSpawnPoints(stagePack[stageIndex].caveSpawnPoints);

        for (int i = 0; i < stagePack.Length; i++)
        {
            if (stagePack[i].stageRoot != null)
                stagePack[i].stageRoot.SetActive(i == stageIndex);
        }

        var stage = stagePack[stageIndex];
        if (stage.caveRoots != null && stage.caveRoots.Length > 0)
        {
            for (int i = 0; i < stage.caveRoots.Length; i++)
            {
                if (stage.caveRoots[i] != null)
                    stage.caveRoots[i].SetActive(i == caveIndex);
            }
        }

        if (resourceSpawner != null)
        {
            resourceSpawner.caveGroundTilemaps = stage.caveGroundTilemaps;
            resourceSpawner.caveRoots = stage.caveRoots;
            resourceSpawner.enemySpawner = enemySpawner;
        }

        if (enemySpawner != null)
        {
            enemySpawner.enemyGroups = stage.enemyGroups;
        }
    }

    void SpawnNormal(int stageIndex, int caveIndex)
    {
        if (resourceSpawner != null)
            resourceSpawner.SpawnForCurrentCave();
    }

}
