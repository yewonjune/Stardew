using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCPathAgent2D : MonoBehaviour
{
    public AStarGrid2D grid;
    public float speed = 2.5f;
    public float waypointReachDist = 0.06f;

    Animator animator;
    Rigidbody2D rb;

    readonly List<Vector3> _path = new();
    int _idx = -1;

    public bool IsMoving { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void SetGrid(AStarGrid2D g) => grid = g;

    public bool SetDestination(Vector3 worldTarget)
    {
        if (!grid)
        {
            Debug.LogWarning("[NPCPathAgent2D] Grid not set");
            Stop();
            return false;
        }

        if (grid.FindPath(transform.position, worldTarget, _path))
        {
            _idx = 0;
            IsMoving = true;
            return true;
        }
        else
        {
            _path.Clear();
            Stop();
            return false;
        }
    }

    public void Stop()
    {
        IsMoving = false;
        _idx = -1;
        _path.Clear();
        SetAnimIdle();
    }

    /// <summary> 문 통과 등 스냅 </summary>
    public void Warp(Vector3 worldPos)
    {
        rb.position = worldPos;
        transform.position = worldPos;
        // 경로 초기화
        _idx = -1;
        _path.Clear();
        IsMoving = false;
        SetAnimIdle();
    }

    void FixedUpdate()
    {
        if (!IsMoving || _idx < 0 || _idx >= _path.Count) return;

        Vector3 target = _path[_idx];
        Vector3 pos = rb.position;
        Vector3 dir = (target - pos);
        float dist = dir.magnitude;

        if (dist <= waypointReachDist)
        {
            _idx++;
            if (_idx >= _path.Count)
            {
                Stop();
                return;
            }
            return;
        }

        Vector3 step = dir.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        // 애니메이션 파라미터(경로 이동 중일 때만 세팅)
        if (animator)
        {
            animator.SetFloat("MoveX", step.x);
            animator.SetFloat("MoveY", step.y);
            animator.SetBool("isMoving", step.sqrMagnitude > 0.0001f);
        }
    }

    void SetAnimIdle()
    {
        if (!animator) return;
        animator.SetBool("isMoving", false);
    }
}
