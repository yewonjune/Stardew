using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BombGoblinMovementController : MonoBehaviour
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

    [FoldoutGroup("References")]
    [LabelText("Bomb Attack")]
    [ReadOnly] public BombGoblinAttack bombAttack;

    // ================== Movement ==================
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
    public Vector2 wanderInterval = new Vector2(3f, 6f); // 1~3초마다 방향 바꿈

    [FoldoutGroup("Movement Settings")]
    [LabelText("집 기준 배회 반경")]
    public float homeRadius = 3f;                // 스폰 지점 주변에서만 빙빙

    // 내부용
    Vector2 homePos;                 // 스폰 위치
    float nextWanderChangeTime = 0f; // 다음에 방향 바꿀 시간

    // ================== Attack Settings ==================
    [FoldoutGroup("Attack Settings", Expanded = true)]
    [LabelText("공격 사거리 (폭탄 던질 거리)")]
    [MinValue(0f)]
    public float attackRange = 3.0f;

    [FoldoutGroup("Attack Settings")]
    [LabelText("공격 쿨타임")]
    [MinValue(0f)]
    public float attackCooldown = 3f;

    [FoldoutGroup("Attack Settings")]
    [LabelText("Attack 애니메이션 전체 길이(초)")]
    [MinValue(0f)]
    public float attackAnimDuration = 0.5f;

    // ================== Flee Settings ==================
    [FoldoutGroup("Flee Settings", Expanded = true)]
    [LabelText("도망 속도 배수")]
    public float fleeSpeedMultiplier = 1.5f;

    [FoldoutGroup("Flee Settings")]
    [LabelText("도망 종료 거리 (플레이어와 이만큼 떨어지면 멈춤)")]
    public float fleeDistance = 3f;

    // ================== Runtime ==================
    [ShowInInspector, ReadOnly]
    [FoldoutGroup("Runtime")]
    private float lastAttackTime = -999f;

    [ShowInInspector, ReadOnly]
    [FoldoutGroup("Runtime")]
    private bool isAttacking = false;

    enum State { Idle, Chase, Attacking, Flee }
    [ShowInInspector, ReadOnly]
    [FoldoutGroup("Runtime")]
    State state = State.Idle;

    Vector2 lastTargetPos; // 폭탄 던질 플레이어 위치
    Vector2 fleeDir;       // 도망 방향

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyBase>();
        bombAttack = GetComponent<BombGoblinAttack>();

        homePos = transform.position;
    }

    // Start is called before the first frame update
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
        float baseSpeed = enemy.stats != null ? enemy.stats.moveSpeed : 2f;
        float dist = Vector2.Distance(transform.position, player.position);

        if (isAttacking) return;

        switch (state)
        {
            case State.Idle:
                if (dist <= radius)
                {
                    // 플레이어가 탐지 반경 안으로 오면 쫓아가기 시작
                    state = State.Chase;
                    moveDir = Vector2.zero;
                }
                else
                {
                    // 플레이어 없을 때는 집 주변에서 어슬렁
                    UpdateWander();
                    baseSpeed *= wanderSpeedMultiplier;
                }
                break;

            case State.Chase:
                if (dist > radius * 1.2f)
                {
                    // 너무 멀어지면 다시 Idle
                    state = State.Idle;
                    moveDir = Vector2.zero;
                }
                else if (dist > attackRange)
                {
                    // 공격 사거리 밖 → 추격
                    moveDir = (player.position - transform.position).normalized;
                }
                else
                {
                    // 공격 사거리 안 → 준비
                    moveDir = Vector2.zero;

                    if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                    {
                        StartCoroutine(AttackRoutine());
                    }
                }
                break;

            case State.Attacking:
                // 공격 중엔 제자리
                moveDir = Vector2.zero;
                break;

            case State.Flee:
                float fleeSpeed = baseSpeed * fleeSpeedMultiplier;

                if (dist < fleeDistance)
                {
                    moveDir = fleeDir;
                }
                else
                {
                    // 충분히 멀어지면 다시 Idle로
                    state = State.Idle;
                    moveDir = Vector2.zero;
                }
                break;

                rb.MovePosition(rb.position + moveDir * fleeSpeed * Time.fixedDeltaTime);
                UpdateAnimator(moveDir);
        }

        // Chase / Idle / Attacking 상태의 이동
        rb.MovePosition(rb.position + moveDir * baseSpeed * Time.fixedDeltaTime);

        // 애니메이션 방향 결정
        Vector2 animDir = moveDir;
        if (animDir.sqrMagnitude <= 0.001f)
        {
            // 안 움직일 때는 플레이어 쪽을 바라보게
            Vector2 toPlayer = (player.position - transform.position);
            if (toPlayer.sqrMagnitude > 0.001f)
                animDir = toPlayer.normalized;
        }

        UpdateAnimator(animDir);
    }

    void SetIdle()
    {
        state = State.Idle;
        moveDir = Vector2.zero;
        UpdateAnimator(Vector2.zero);
    }

    void UpdateAnimator(Vector2 dir)
    {
        bool isMoving = dir.sqrMagnitude > 0.001f;

        if(!isAttacking)
            animator.SetBool("isMoving", isMoving);
        else
            animator.SetBool("isMoving", false);

        if (isMoving)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        // 좌우 뒤집기 (EnemyBase 설정 재사용)
        if (dir.x > 0.1f)
        {
            transform.localScale = enemy.defaultFacingRight
                ? new Vector3(1, 1, 1)
                : new Vector3(-1, 1, 1);
        }
        else if (dir.x < -0.1f)
        {
            transform.localScale = enemy.defaultFacingRight
                ? new Vector3(-1, 1, 1)
                : new Vector3(1, 1, 1);
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        state = State.Attacking;
        lastAttackTime = Time.time;

        // 1. 현재 플레이어 위치 저장
        lastTargetPos = player.position;

        // 2. 플레이어 방향으로 바라보게
        Vector2 lookDir = (player.position - transform.position).normalized;
        if (lookDir.sqrMagnitude > 0.001f)
            UpdateAnimator(lookDir);

        // 3. Attack 애니메이션 먼저 재생
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        // 5. Attack 애니메이션이 끝날 때까지 기다리기
        //    (attackAnimDuration을 Attack 클립 길이에 맞춰 넣어줘)
        yield return new WaitForSeconds(attackAnimDuration);

        // 6. 이제부터 Flee 시작
        fleeDir = (transform.position - player.position).normalized;
        if (fleeDir.sqrMagnitude < 0.001f)
            fleeDir = Vector2.left; // 혹시 0이면 임시 방향

        state = State.Flee;
        isAttacking = false;
    }

    public void OnAnimationThrowBomb()
    {
        bombAttack?.ThrowBomb(lastTargetPos);
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

        // homeRadius보다 많이 나가면 집 쪽으로 강하게 돌리기
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
