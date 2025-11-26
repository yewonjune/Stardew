using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public float idleTime = 0f;
    Transform player;
    Rigidbody2D rb;
    Animator animator;
    EnemyBase enemy;
    Vector2 moveDir;

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
        if (player == null || enemy == null || enemy.enabled == false)
        {
            SetIdle();
            return;
        }

        float radius = enemy.stats != null ? enemy.stats.detectRadius : 5f;
        float speed = enemy.stats != null ? enemy.stats.moveSpeed : 2f;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= radius)
            moveDir = (player.position - transform.position).normalized;
        else
            moveDir = Vector2.zero;

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
}