using System.Collections;
using UnityEngine;

public class PlaceEntrance : MonoBehaviour, IInteractable
{
    public Transform player;

    [Header("이동할 위치")]
    public Transform targetTransform;
    //public Vector3 playerIndoorPosition;

    [SerializeField] string targetCamKey = "House";

    public float interactDistance = 1.5f;
    public string entranceLayerName = "Entrance";

    int entranceLayerMask;
    Collider2D selfCol;

    [Header("Optional: Random Spawn (ex. Cave 1~4)")]
    public bool useRandomSpawnPositions = false;

    public string InteractLabel => "들어가기";

    public void Interact() => EnterPlace();

    void Start()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }

        if (CameraManager.Instance == null)
        {
            CameraManager.Instance = FindObjectOfType<CameraManager>();
        }

        entranceLayerMask = LayerMask.GetMask(entranceLayerName);
        selfCol = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && selfCol)
        {
            var cam = Camera.main;
            if (!cam) return;

            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.OverlapPoint(mouseWorld, entranceLayerMask);
            if (hit == selfCol && InRange())
                EnterPlace();
        }
    }

    void EnterPlace()
    {
        if (!player) return;

        if (CameraManager.Instance == null)
        {
            CameraManager.Instance = FindObjectOfType<CameraManager>();
        }

        var fade = FadeManager.Instance ?? FindObjectOfType<FadeManager>();
        var col = player.GetComponent<Collider2D>();
        var mover = player.GetComponent<PlayerMovement>();
        var rb = player.GetComponent<Rigidbody2D>();

        if (mover) mover.enabled = false;
        if (rb) { rb.velocity = Vector2.zero; rb.isKinematic = true; }
        if (col) col.enabled = false;

        System.Action teleport = () =>
        {
            // 1) 랜덤 동굴 입장용
            if (useRandomSpawnPositions)
            {
                int startFloor = 1;

                var floorManager = FindObjectOfType<CaveFloorManager>();
                if (floorManager != null)
                {
                    floorManager.EnterFloor(startFloor);

                    int caveIndex = CaveStateManager.CurrentCaveIndex;
                    Vector3 spawnPos = (CaveSpawnManager.Instance != null)
                        ? CaveSpawnManager.Instance.GetSpawnPosition(caveIndex)
                        : Vector3.zero;

                    player.position = spawnPos;

                    StartCoroutine(RestoreNextFrame(rb, col, mover));
                    return;
                }
            }

            // 2) 랜덤 스폰 안 쓸 때는 targetTransform으로 이동
            if (targetTransform != null)
            {
                player.position = targetTransform.position;
            }

            if (CameraManager.Instance != null && !string.IsNullOrEmpty(targetCamKey))
            {
                CameraManager.Instance.SwitchTo(targetCamKey);
            }
            StartCoroutine(RestoreNextFrame(rb, col, mover));
        };

            if (fade) fade.FadeOutIn(teleport);
            else teleport();
        
    }

    IEnumerator RestoreNextFrame(Rigidbody2D rb, Collider2D col, PlayerMovement mover)
    {
        yield return null;
        if (rb) rb.isKinematic = false;
        if (col) col.enabled = true;
        if (mover) mover.enabled = true;
    }

    bool InRange()
    {
        if (!player) return false;
        float sqr = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        return sqr <= interactDistance * interactDistance;
    }
}
