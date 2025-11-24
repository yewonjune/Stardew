using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLadder : MonoBehaviour
{
    public Transform player;
    public float interactDistance = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && InRange())
        {
            GoDown();
        }
    }

    bool InRange()
    {
        if (!player) return false;
        float sqr = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        return sqr <= interactDistance * interactDistance;
    }

    void GoDown()
    {
        if (!player) return;

        int currentIndex = CaveStateManager.CurrentCaveIndex;

        var spawner = FindObjectOfType<ResourceSpawner_Cave>();

        int newIndex = currentIndex;
        if (spawner != null && spawner.caveGroundTilemaps != null &&
            spawner.caveGroundTilemaps.Length > 0)
        {
            int count = spawner.caveGroundTilemaps.Length;

            if (count == 1)
            {
                newIndex = 0;
            }
            else
            {
                newIndex = Random.Range(0, count);

                if (newIndex == currentIndex)
                {
                    newIndex = (newIndex + 1) % count;
                }
            }
        }

        CaveStateManager.CurrentCaveIndex = newIndex;

        Vector3 spawnPos = CaveSpawnManager.Instance.GetSpawnPosition(newIndex);
        player.position = spawnPos;

        if (spawner != null)
        {
            spawner.SpawnForCurrentCave();
        }

        Destroy(gameObject);
    }

}
