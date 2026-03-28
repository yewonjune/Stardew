using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCMovement : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    public string npcId;
    public string InteractLabel => "대화하기";

    [Header("Dialogue")]
    public DialogueData dialogueData;

    [Header("Move")]
    public float speed = 2.5f;
    public float arriveDist = 0.05f;

    [Header("Path")]
    public Transform[] wayPoints;
    public bool autoLoopWayPoints = true;
    public bool startWithDefaultPath = false;   // 기본 경로로 자동 시작을 원할 때만 true

    [Header("Persistence Options")]
    public bool loadProgressOnStart = true;
    public bool autoSaveOnArrive = true;
    public bool autoSavePosition = true;
    public float positionSaveInterval = 2f;

    Animator animator;
    Rigidbody2D rb;

    int wayPointIndex = 0;
    Vector3 target;
    bool hasTarget;

    float lastX = 0f;
    float lastY = -1f;

    DoorWaypoint _lastDoorA;
    DoorWaypoint _lastDoorB;

    Coroutine posSaver;

    // 외부(스케줄/타 시스템)에서 경로를 강제로 지정했음을 나타내는 플래그
    bool _pathForcedExternally = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 1) 진행도/위치 복원
        if (loadProgressOnStart && NPCStateManager.Instance != null && !string.IsNullOrEmpty(npcId))
        {
            wayPointIndex = Mathf.Clamp(
                NPCStateManager.Instance.LoadWaypointIndex(npcId, wayPointIndex),
                0, Mathf.Max(0, (wayPoints?.Length ?? 1) - 1)
            );

            var savedScene = NPCStateManager.Instance.LoadScene(npcId, null);
            string npcScene = gameObject.scene.name;

            if (!string.IsNullOrEmpty(savedScene) && savedScene == npcScene)
            {
                if (NPCStateManager.Instance.TryLoadPosition(npcId, out var savedPos))
                {
                    Warp(savedPos);
                }
            }
        }

        // 2) 시작 타겟 지정 (조건부)
        //    - 외부에서 이미 SetPath로 경로 강제: 건너뜀
        //    - 진행도 저장 있음: 저장된 인덱스 기준으로 시작
        //    - 진행도 없음 & startWithDefaultPath가 true일 때만 기본 경로 시작
        if (!_pathForcedExternally && wayPoints != null && wayPoints.Length > 0)
        {
            bool hasProgress = NPCStateManager.Instance != null
                               && !string.IsNullOrEmpty(npcId)
                               && NPCStateManager.Instance.HasWaypointProgress(npcId);

            if (hasProgress)
            {
                wayPointIndex = Mathf.Clamp(wayPointIndex, 0, wayPoints.Length - 1);
                SetTarget(wayPoints[wayPointIndex].position);
            }
            else if (startWithDefaultPath)
            {
                wayPointIndex = 0;
                SetTarget(wayPoints[0].position);
            }
        }

        // 3) 위치 자동 저장 루프
        if (autoSavePosition && positionSaveInterval > 0f)
            posSaver = StartCoroutine(CoAutoSavePosition());

        var holder = GetComponent<NPCScheduleHolder>();
        if (holder != null) holder.isReturningToScene = true;
    }

    void OnDisable()
    {
        SafePersistState();
        if (posSaver != null) StopCoroutine(posSaver);
    }

    IEnumerator CoAutoSavePosition()
    {
        var wait = new WaitForSeconds(positionSaveInterval);
        while (true)
        {
            SafePersistPositionOnly();
            yield return wait;
        }
    }

    void FixedUpdate()
    {
        if (!hasTarget) return;

        Vector3 pos = rb.position;
        Vector3 dir = target - pos;
        float dist = dir.magnitude;

        if (dist <= arriveDist)
        {
            hasTarget = false;
            SetAnimIdle();

            var currentWp = wayPoints != null && wayPointIndex >= 0 && wayPointIndex < wayPoints.Length
                            ? wayPoints[wayPointIndex]
                            : null;

            var door = currentWp ? currentWp.GetComponent<DoorWaypoint>() : null;

            if (door != null && door.warpTarget != null)
            {
                if (door != _lastDoorA && door != _lastDoorB)
                {
                    Warp(door.warpTarget.position);

                    var targetDoor = door.warpTarget.GetComponent<DoorWaypoint>();
                    _lastDoorA = door;
                    _lastDoorB = targetDoor;

                    int idx = FindWayPointIndex(door.warpTarget);
                    if (idx >= 0)
                    {
                        wayPointIndex = idx;
                        SaveWaypointIndex();
                    }

                    TryGoNextWayPoint();
                    return;
                }
                else
                {
                    _lastDoorA = null;
                    _lastDoorB = null;
                }
            }
            else
            {
                _lastDoorA = null;
                _lastDoorB = null;
            }

            if (autoSaveOnArrive) SaveWaypointIndex();
            TryGoNextWayPoint();
            return;
        }

        Vector3 moveDir = dir.normalized;
        Vector3 step = moveDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        if (animator)
        {
            float animX = 0f;
            float animY = 0f;

            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                animX = moveDir.x > 0 ? 1f : -1f;
                animY = 0f;

                Vector3 localScale = transform.localScale;
                localScale.x = moveDir.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
            }
            else
            {
                animY = moveDir.y > 0 ? 1f : -1f;
                animX = 0f;
            }

            animator.SetFloat("MoveX", animX);
            animator.SetFloat("MoveY", animY);
            animator.SetBool("isMoving", true);

            lastX = animX;
            lastY = animY;
        }
    }

    public void Interact()
    {
        var aiDialogue = GetComponent<NPCAIDialogue>();
        if (aiDialogue != null)
        {
            var ui = FindObjectOfType<NPCAIDialogueUI>(true);
            if (ui != null)
            {
                ui.OpenDialogue(aiDialogue, aiDialogue.npcData);
                return;
            }
        }

        if (dialogueData)
            DialogueManager.Instance.StartDialogue(dialogueData);
    }

    public void SetTarget(Vector3 pos)
    {
        target = pos;
        hasTarget = true;
    }

    public void Stop()
    {
        hasTarget = false;
        SetAnimIdle();
    }

    public void Warp(Vector3 worldPos)
    {
        hasTarget = false;
        rb.position = worldPos;
        transform.position = worldPos;
        SetAnimIdle();
        SafePersistPositionOnly();
    }

    void SetAnimIdle()
    {
        if (!animator) return;
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveY", -1f);
        animator.SetBool("isMoving", false);
    }

    void TryGoNextWayPoint()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        wayPointIndex++;

        if (wayPointIndex >= wayPoints.Length)
        {
            if (autoLoopWayPoints) wayPointIndex = 0;
            else { SaveWaypointIndex(); return; }
        }

        SaveWaypointIndex();
        SetTarget(wayPoints[wayPointIndex].position);
    }

    public void SetPath(Transform[] newPoints, bool autoLoop = false, bool warpToEnd = false)
    {
        wayPoints = newPoints;
        autoLoopWayPoints = autoLoop;
        _pathForcedExternally = true;
        _lastDoorA = null; _lastDoorB = null;

        if (wayPoints == null || wayPoints.Length == 0)
        {
            Stop();
            return;
        }

        if (warpToEnd)
        {
            // 경로 마지막 지점으로 즉시 워프 (이동 없이)
            wayPointIndex = wayPoints.Length - 1;
            SaveWaypointIndex();
            Warp(wayPoints[wayPointIndex].position);
        }
        else
        {
            wayPointIndex = 0;
            SaveWaypointIndex();
            SetTarget(wayPoints[0].position);
        }
    }

    int FindWayPointIndex(Transform t)
    {
        if (wayPoints == null) return -1;
        for (int i = 0; i < wayPoints.Length; i++)
            if (wayPoints[i] == t) return i;
        return -1;
    }

    void OnDrawGizmos()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < wayPoints.Length - 1; i++)
        {
            if (wayPoints[i] != null && wayPoints[i + 1] != null)
                Gizmos.DrawLine(wayPoints[i].position, wayPoints[i + 1].position);
        }
    }

    // ===== 저장 유틸 =====
    void SaveWaypointIndex()
    {
        if (NPCStateManager.Instance == null || string.IsNullOrEmpty(npcId)) return;
        NPCStateManager.Instance.SaveWaypointIndex(
            npcId,
            Mathf.Clamp(wayPointIndex, 0, Mathf.Max(0, (wayPoints?.Length ?? 1) - 1))
        );
    }

    void SafePersistPositionOnly()
    {
        if (NPCStateManager.Instance == null || string.IsNullOrEmpty(npcId)) return;
        NPCStateManager.Instance.SavePosition(npcId, transform.position);
        NPCStateManager.Instance.SaveScene(npcId, gameObject.scene.name);
    }

    void SafePersistState()
    {
        SaveWaypointIndex();
        SafePersistPositionOnly();
    }
}
