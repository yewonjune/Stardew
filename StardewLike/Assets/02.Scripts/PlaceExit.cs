using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceExit : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Positions")]
    public Vector3 playerOutdoorPosition;

    private CameraManager cameraManager;

    // Start is called before the first frame update
    void Start()
    {
        cameraManager = CameraManager.Instance;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == player)
        {
            ExitHouse();
        }
    }

    void ExitHouse()
    {
        if (cameraManager == null || player == null)
        {
            Debug.LogError("cameraManager or player is null!");
            return;
        }

        FadeManager.Instance.FadeOutIn(() =>
        {
            cameraManager.SwitchToFarm();
            player.position = playerOutdoorPosition;
        });
    }
}
