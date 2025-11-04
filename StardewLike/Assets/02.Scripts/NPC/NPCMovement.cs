using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public DialogueData dialogueData;

    public float speed = 2.5f;
    public float arriveDist = 0.05f;

    Animator animator;
    Rigidbody2D rb;

    public Transform[] wayPoints;
    public bool autoLoopWayPoints = true;
    int wayPointIndex = 0;

    Vector3 target;
    bool hasTarget;

    float lastX = 0f;
    float lastY = -1f;

    DoorWaypoint _lastDoorA;
    DoorWaypoint _lastDoorB;

    public bool startWithDefaultPath = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }
    void Start()
    {
        if (startWithDefaultPath && wayPoints != null && wayPoints.Length > 0)
        {
            SetTarget(wayPoints[0].position);
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

            var currentWp = wayPoints[wayPointIndex];
            var door = currentWp != null ? currentWp.GetComponent<DoorWaypoint>() : null;

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
            if (autoLoopWayPoints)
            {
                wayPointIndex = 0;
            }
            else
            {
                return;
            }
        }

        SetTarget(wayPoints[wayPointIndex].position);
    }

    public void SetPath(Transform[] newPoints, bool autoLoop = false)
    {
        wayPoints = newPoints;
        autoLoopWayPoints = autoLoop;
        wayPointIndex = 0;

        if (wayPoints != null && wayPoints.Length > 0)
        {
            SetTarget(wayPoints[0].position);
        }
        else
        {
            Stop();
        }
    }

    int FindWayPointIndex(Transform t)
    {
        if (wayPoints == null) return -1;
        for (int i = 0; i < wayPoints.Length; i++)
        {
            if (wayPoints[i] == t)
                return i;
        }
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
}
