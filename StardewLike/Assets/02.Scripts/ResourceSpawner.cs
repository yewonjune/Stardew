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
        GenerateResouce();
    }

    void GenerateResouce()
    {
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if(!groundTilemap.HasTile(pos)) continue;

            if(Random.value < spawnProbability) 
            {
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(pos);

                Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f, resourceLayer);
                if (hit != null) continue;

                GameObject resoucePrefab = (Random.value < 0.5f) ? rockPrefab : stumpPrefab;

                Instantiate(resoucePrefab, worldPos, Quaternion.identity);

            }
        }
    }
}
