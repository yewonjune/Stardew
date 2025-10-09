using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceEntrance : MonoBehaviour
{
    public Transform player;

    public Vector3 playerIndoorPosition;

    public float interactDistance = 1.5f;

    private CameraManager cameraManager;
    private int entranceLayerMask;
    private Collider2D selfCol;


    // Start is called before the first frame update
    void Start()
    {
        cameraManager = CameraManager.Instance;
        entranceLayerMask = LayerMask.GetMask("Entrance");
        selfCol = GetComponent<Collider2D>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, entranceLayerMask);
            
            if (hit.collider != null && hit.collider == selfCol && InRange())
            {
                EnterHouse();
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && InRange())
        {
            EnterHouse();
        }
    }

    void EnterHouse()
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
            cameraManager.SwitchToHouse();
            player.position = playerIndoorPosition;

            player.GetComponent<MonoBehaviour>().StartCoroutine(ReenableColliderNextFrame(col));
        });
    }

    System.Collections.IEnumerator ReenableColliderNextFrame(Collider2D col)
    {
        yield return null;
        if (col) col.enabled = true;
    }

    bool InRange()
    {
        float sqr = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        return sqr <= interactDistance * interactDistance;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
