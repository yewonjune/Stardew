using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveSpawnManager : MonoBehaviour
{
    [Header("층별 플레이어 스폰 위치 (0 = Cave0, 1 = Cave1, ...)")]
    public Transform[] caveSpawnPoints;

    public static CaveSpawnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Vector3 GetSpawnPosition(int caveIndex)
    {
        if (caveSpawnPoints == null || caveSpawnPoints.Length == 0)
            return Vector3.zero;

        if (caveIndex < 0 || caveIndex >= caveSpawnPoints.Length)
            caveIndex = Mathf.Clamp(caveIndex, 0, caveSpawnPoints.Length - 1);

        var t = caveSpawnPoints[caveIndex];
        return t ? t.position : Vector3.zero;
    }
    public void SetSpawnPoints(Transform[] newPoints)
    {
        caveSpawnPoints = newPoints;
    }
}
