using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCMovement : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogueData;

    [Header("Legacy/Fallback Move (No Grid)")]
    public float speed = 2.5f;
    public float arriveDist = 0.05f;

    Animator animator;
    Rigidbody2D rb;

    // 직선 이동용 상태(그리드 없을 때만 사용)
    Vector3 target;
    bool moving;

    // 경로 이동 에이전트
    NPCPathAgent2D agent;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NPCPathAgent2D>();
    }

    void OnValidate()
    {
        // 에디터에서 speed 동기화(편의)
        if (!agent) agent = GetComponent<NPCPathAgent2D>();
        if (agent) agent.speed = speed;
    }

    void FixedUpdate()
    {
        // ★ A* 에이전트가 활성로 이동 중이면, 여기서는 아무 것도 하지 않는다.
        if (agent && agent.IsMoving) return;

        // ★ 그리드가 없거나 A* 미사용일 때만 직선 이동 fallback
        if (!moving) return;

        Vector3 pos = rb.position;
        Vector3 dir = (target - pos);
        float dist = dir.magnitude;

        if (dist <= arriveDist)
        {
            moving = false;
            SetAnimIdle();
            return;
        }

        Vector3 step = dir.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        // 애니메이션 파라미터(직선 이동일 때만 세팅)
        if (animator)
        {
            animator.SetFloat("MoveX", step.x);
            animator.SetFloat("MoveY", step.y);
            animator.SetBool("isMoving", step.sqrMagnitude > 0.0001f);
        }
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogueData);
    }

    /// <summary>
    /// 외부(스케줄/AI)가 호출하는 진입점.
    /// 그리드가 있으면 PathAgent로, 없으면 직선 이동 fallback.
    /// </summary>
    public void SetTarget(Vector3 pos)
    {
        // 1) A* 경로 사용 시: PathAgent에 위임
        if (agent && agent.grid)
        {
            moving = false; // 직선이동 비활성
            agent.SetDestination(pos);
            return;
        }

        // 2) 그리드가 없으면 직선 fallback
        target = pos;
        moving = true;
    }

    /// <summary> 즉시 정지 </summary>
    public void Stop()
    {
        moving = false;
        if (agent) agent.Stop();
        SetAnimIdle();
    }

    /// <summary> 순간이동(문 통과 스냅 등에 사용) </summary>
    public void Warp(Vector3 worldPos)
    {
        moving = false;
        if (agent) agent.Stop();
        rb.position = worldPos;
        transform.position = worldPos;
        SetAnimIdle();
    }

    void SetAnimIdle()
    {
        if (!animator) return;
        animator.SetBool("isMoving", false);
    }
}
