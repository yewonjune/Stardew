using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceExit : MonoBehaviour
{
    public Transform player;

    public Vector3 playerOutdoorPosition;

    private CameraManager cameraManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }

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

        var col = player.GetComponent<Collider2D>();
        if (col) col.enabled = false;

        FadeManager.Instance.FadeOutIn(() =>
        {
            cameraManager.SwitchToFarm();
            player.position = playerOutdoorPosition;

            player.GetComponent<MonoBehaviour>().StartCoroutine(ReenableColliderNextFrame(col));
        });
    }
    System.Collections.IEnumerator ReenableColliderNextFrame(Collider2D col)
    {
        yield return null;
        if (col) col.enabled = true;
    }
}
