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


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }
    void Start()
    {
        if (wayPoints != null && wayPoints.Length > 0)
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

        // 도착했으면 멈춤
        if (dist <= arriveDist)
        {
            hasTarget = false;
            SetAnimIdle();

            var currentWp = wayPoints[wayPointIndex];
            var door = currentWp != null ? currentWp.GetComponent<DoorWaypoint>() : null;
            if (door != null && door.warpTarget != null)
            {
                Warp(door.warpTarget.position);
            }

            TryGoNextWayPoint();
            return;

        }

        Vector3 moveDir = dir.normalized;
        Vector3 step = moveDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        if (animator)
        {
            // 방향 정리: 어느 축이 더 큰지에 따라 상하/좌우 고정
            float animX = 0f;
            float animY = 0f;

            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                // 좌우 이동
                animX = moveDir.x > 0 ? 1f : -1f;
                animY = 0f;

                Vector3 localScale = transform.localScale;
                localScale.x = moveDir.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
            }
            else
            {
                // 상하 이동
                animY = moveDir.y > 0 ? 1f : -1f;
                animX = 0f;
            }

            animator.SetFloat("MoveX", animX);
            animator.SetFloat("MoveY", animY);
            animator.SetBool("isMoving", true);

            // 나중에 멈춰도 이 방향 유지
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

        animator.SetFloat("MoveX", lastX);
        animator.SetFloat("MoveY", lastY);
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
                wayPointIndex = 0; // 다시 처음부터
            }
            else
            {
                return; // 더 이상 안 감
            }
        }

        SetTarget(wayPoints[wayPointIndex].position);
    }

    void OnDrawGizmos()
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        Gizmos.color = Color.green;

        // 순서대로 선 연결
        Gizmos.color = Color.yellow;
        for (int i = 0; i < wayPoints.Length - 1; i++)
        {
            if (wayPoints[i] != null && wayPoints[i + 1] != null)
                Gizmos.DrawLine(wayPoints[i].position, wayPoints[i + 1].position);
        }

    }
}
