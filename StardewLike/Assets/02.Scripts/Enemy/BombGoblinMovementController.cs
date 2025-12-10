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
    [LabelText("폭탄 준비 시간 (들고 있다가 던지기까지)")]
    [MinValue(0f)]
    public float prepareTime = 0.5f;

    // ================== Flee Settings ==================
    [FoldoutGroup("Flee Settings", Expanded = true)]
    [LabelText("도망 속도 배수")]
    public float fleeSpeedMultiplier = 1.5f;

    [FoldoutGroup("Flee Settings")]
    [LabelText("도망 종료 거리 (플레이어와 이만큼 떨어지면 멈춤)")]
    public float fleeDistance = 6f;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
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

        switch (state)
        {
            case State.Idle:
                if (dist <= radius)
                    state = State.Chase;

                moveDir = Vector2.zero;
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

                rb.MovePosition(rb.position + moveDir * fleeSpeed * Time.fixedDeltaTime);
                UpdateAnimator(moveDir);
                return;
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
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isFlee", state == State.Flee);

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

        // 1. 폭탄 들기 시작할 때 플레이어 위치 저장
        lastTargetPos = player.position;

        // 공격 방향으로 바라보게
        Vector2 lookDir = (player.position - transform.position).normalized;
        if (lookDir.sqrMagnitude > 0.001f)
            UpdateAnimator(lookDir);

        // 2. 폭탄 드는 애니메이션
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack"); // BombGoblinAttack 애니메이션 (들기+던지기 포함)

        // "폭탄 들고 있는 연출" 시간
        yield return new WaitForSeconds(prepareTime);

        // 3. 실제 폭탄 던지기
        bombAttack?.ThrowBomb(lastTargetPos);

        // 4. 도망 방향 = 플레이어 반대 방향
        fleeDir = (transform.position - player.position).normalized;
        if (fleeDir.sqrMagnitude < 0.001f)
            fleeDir = Vector2.left; // 혹시나 0이면 임시 방향

        state = State.Flee;

        // 도망은 FixedUpdate에서 처리, 쿨타임 끝나면 다시 공격 가능
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
}
