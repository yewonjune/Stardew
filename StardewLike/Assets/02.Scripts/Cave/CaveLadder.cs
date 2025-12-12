using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLadder : MonoBehaviour
{
    public Transform player;
    public float interactDistance = 1.5f;

    CaveFloorManager floorManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }

        floorManager = FindObjectOfType<CaveFloorManager>();
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

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeOutIn(DoGoDown);
        else
            DoGoDown();
    }

    void DoGoDown()
    {
        CaveStateManager.CurrentFloor++;

        if (floorManager != null)
            floorManager.EnterFloor(CaveStateManager.CurrentFloor);

        int caveIndex = CaveStateManager.CurrentCaveIndex;
        Vector3 spawnPos = (CaveSpawnManager.Instance != null)
            ? CaveSpawnManager.Instance.GetSpawnPosition(caveIndex)
            : Vector3.zero;

        player.position = spawnPos;

        // UI °»½Å
        var floorUI = FindObjectOfType<CaveFloorUI>();
        if (floorUI != null) floorUI.UpdateFloorUI();

        Destroy(gameObject);
    }

}
