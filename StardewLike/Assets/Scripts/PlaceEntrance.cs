using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceEntrance : MonoBehaviour
{
    public Transform player;
    public Vector3 playerIndoorPosition;

    private bool isInside = false;

    private CameraManager cameraManager;

    // Start is called before the first frame update
    void Start()
    {
        cameraManager = CameraManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int entranceLayerMask = LayerMask.GetMask("Entrance");
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, entranceLayerMask);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    EnterHouse();
                }
            }
        }
    }

    void EnterHouse()
    {
        if (cameraManager == null || player == null)
        {
            Debug.LogError("cameraManager or player is null!");
            return;
        }

        FadeManager.Instance.FadeOutIn(() =>
        {
            cameraManager.SwitchToHouse();
            player.position = playerIndoorPosition;
        });
    }
}
