using System.Collections;
using UnityEngine;

public class PlaceEntrance : MonoBehaviour
{
    public Transform player;

    public Vector3 playerIndoorPosition;

    public float interactDistance = 1.5f;
    public string entranceLayerName = "Entrance";

    CameraManager cameraManager;
    int entranceLayerMask;
    Collider2D selfCol;

    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }

        cameraManager = CameraManager.Instance ?? FindObjectOfType<CameraManager>();
        entranceLayerMask = LayerMask.GetMask(entranceLayerName);
        selfCol = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && InRange())
            EnterHouse();

        if (Input.GetMouseButtonDown(0) && selfCol)
        {
            var cam = Camera.main;
            if (!cam) return;

            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.OverlapPoint(mouseWorld, entranceLayerMask);
            if (hit == selfCol && InRange())
                EnterHouse();
        }
    }

    void EnterHouse()
    {
        if (!cameraManager)
        {
            cameraManager = CameraManager.Instance ?? FindObjectOfType<CameraManager>();
        }

        var fade = FadeManager.Instance ?? FindObjectOfType<FadeManager>();

        var col = player.GetComponent<Collider2D>();

        if (!fade)
        {
            if (col) col.enabled = false;
            cameraManager.SwitchToHouse();
            player.position = playerIndoorPosition;
            StartCoroutine(ReenableColliderNextFrame(col));
            return;
        }

        if (col) col.enabled = false;

        fade.FadeOutIn(() =>
        {
            cameraManager.SwitchToHouse();
            player.position = playerIndoorPosition;
            StartCoroutine(ReenableColliderNextFrame(col));
        });
    }

    IEnumerator ReenableColliderNextFrame(Collider2D col)
    {
        yield return null;
        if (col) col.enabled = true;
    }

    bool InRange()
    {
        if (!player) return false;
        float sqr = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        return sqr <= interactDistance * interactDistance;
    }
}
