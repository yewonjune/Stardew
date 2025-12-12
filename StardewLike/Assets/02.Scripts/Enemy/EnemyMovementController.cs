using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    // ================== References ==================
    [FoldoutGroup("References", Expanded = true)]
    [LabelText("Player Transform")]
    [ReadOnly] Transform player;

    [FoldoutGroup("References")]
    [LabelText("Rigidbody2D")]
    [ReadOnly] public Rigidbody2D rb;

    [FoldoutGroup("References")]
    [LabelText("Animator")]
    [ReadOnly] public Animator animator;

    [FoldoutGroup("References")]
    [LabelText("Enemy Base")]
    [ReadOnly] public EnemyBase enemy;

    // ================== Movement Settings ==================
    [FoldoutGroup("Movement Settings")]
    [ReadOnly]
    [ShowInInspector]
    [LabelText("현재 이동 방향")]
    private Vector2 moveDir;

    [FoldoutGroup("Movement Settings")]
    [LabelText("배회 속도 배수")]
    [MinValue(0f)]
    public float wanderSpeedMultiplier = 0.5f;   // 추격 속도의 절반 정도로 어슬렁

    [FoldoutGroup("Movement Settings")]
    [LabelText("배회 방향 변경 간격 (최소/최대)")]
    public Vector2 wanderInterval = new Vector2(1f, 3f); // 1~3초마다 방향 바꿈

    [FoldoutGroup("Movement Settings")]
    [LabelText("집 기준 배회 반경")]
    public float homeRadius = 3f;                // 스폰 지점 주변에서만 빙빙

    Vector2 homePos;                 // 스폰 위치
    float nextWanderChangeTime = 0f; // 다음에 방향 바꿀 시간

    // ================== Attack Settings ==================
    [FoldoutGroup("Attack Settings", Expanded = true)]
    [LabelText("공격 사거리")]
    [MinValue(0f)]
    public float attackRange = 1.0f;

    [FoldoutGroup("Attack Settings")]
    [LabelText("공격 쿨타임")]
    [MinValue(0f)]
    public float attackCooldown = 1.5f;

    [FoldoutGroup("Attack Settings")]
    [ShowInInspector, ReadOnly]
    [LabelText("마지막 공격 시간")]
    private float lastAttackTime = -999f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyBase>();

        homePos = transform.position;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        ScheduleNextWanderChange();
    }

    void FixedUpdate()
    {
        if (GamePause.isPaused)
        {
            SetIdle();
            return;
        }

        if (player == null || enemy == null || enemy.enabled == false || enemy.IsKnockback)
        {
            SetIdle();
            return;
        }

        float radius = enemy.stats != null ? enemy.stats.detectRadius : 5f;
        float speed = enemy.stats != null ? enemy.stats.moveSpeed : 2f;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > radius)
        {
            UpdateWander();
            speed *= wanderSpeedMultiplier;
        }
        else
        {
            if (dist <= attackRange)
            {
                moveDir = Vector2.zero;
                TryAttack();
            }
            else
            {
                moveDir = (player.position - transform.position).normalized;
            }
        }

        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);

        Vector2 animDir = moveDir;
        if (animDir.sqrMagnitude <= 0.001f)
        {
            Vector2 toPlayer = (player.position - transform.position);
            if (toPlayer.sqrMagnitude > 0.001f)
                animDir = toPlayer.normalized;
        }

        UpdateAnimator(moveDir);
    }

    void SetIdle()
    {
        moveDir = Vector2.zero;
        UpdateAnimator(moveDir);
    }

    void UpdateAnimator(Vector2 dir)
    {
        bool isMoving = dir.sqrMagnitude > 0.001f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        if (dir.x > 0.1f)
        {
            transform.localScale = enemy.defaultFacingRight
                ? new Vector3(1, 1, 1)      // 오른쪽 기본일 때
                : new Vector3(-1, 1, 1);    // 왼쪽 기본일 때
        }
        else if (dir.x < -0.1f)
        {
            transform.localScale = enemy.defaultFacingRight
                ? new Vector3(-1, 1, 1)     // 오른쪽 기본 → 왼쪽 볼 때 뒤집기
                : new Vector3(1, 1, 1);     // 왼쪽 기본 → 오른쪽 볼 때 뒤집기
        }
    }
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

    }

    void UpdateWander()
    {
        // 일정 시간마다 새로운 랜덤 방향으로 변경
        if (Time.time >= nextWanderChangeTime || moveDir.sqrMagnitude < 0.001f)
        {
            // 기본은 랜덤 방향
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            // 집에서 너무 멀어지지 않게, 집 쪽으로 약간 당기는 느낌
            Vector2 toHome = (homePos - (Vector2)transform.position);
            if (toHome.sqrMagnitude > 0.01f)
            {
                // 집 방향 0.4 + 랜덤 0.6 정도로 섞어서 자연스럽게
                Vector2 homeBias = toHome.normalized * 0.4f;
                randomDir = (randomDir * 0.6f + homeBias).normalized;
            }

            moveDir = randomDir;
            ScheduleNextWanderChange();
        }

        float distFromHome = Vector2.Distance(transform.position, homePos);
        if (distFromHome > homeRadius)
        {
            moveDir = (homePos - (Vector2)transform.position).normalized;
        }
    }

    void ScheduleNextWanderChange()
    {
        float t = Random.Range(wanderInterval.x, wanderInterval.y);
        nextWanderChangeTime = Time.time + t;
    }
}