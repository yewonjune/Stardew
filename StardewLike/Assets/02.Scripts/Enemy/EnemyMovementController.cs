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
    [FoldoutGroup("Movement Settings", Expanded = true)]
    [LabelText("정지 시간(미사용)")]
    [SerializeField] float idleTime = 0f;

    [FoldoutGroup("Movement Settings")]
    [ReadOnly]
    [ShowInInspector]
    [LabelText("현재 이동 방향")]
    private Vector2 moveDir;

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
    }

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
        float speed = enemy.stats != null ? enemy.stats.moveSpeed : 2f;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > radius)
        {
            moveDir = Vector2.zero;
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
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (dir.x < -0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
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
}