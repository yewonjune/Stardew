using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public EnemyStats stats;

    protected Animator animator;
    protected Rigidbody2D rb;
    protected int currentHp;
    protected bool isDead = false;

    public bool IsKnockback { get; private set; }

    public float knockbackDistance = 0.3f;
    public float knockbackDuration = 0.1f;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHp = stats != null ? stats.maxHp : 10;
    }
    public virtual void TakeDamage(int damage, Vector2 hitDir)
    {
        if (isDead) return;

        currentHp -= damage;
        
        if (currentHp <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("Hit");

        // DOTween ³Ë¹é
        if (hitDir.sqrMagnitude > 0.0001f)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + (Vector3)hitDir.normalized * knockbackDistance;

            transform.DOKill();
            if (rb != null) rb.velocity = Vector2.zero;

            IsKnockback = true;

            transform.DOMove(targetPos, knockbackDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (rb != null) rb.velocity = Vector2.zero;
                    IsKnockback = false;
                });
        }

        if (currentHp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        if (animator)
            animator.SetTrigger("Die");

        Destroy(gameObject, stats != null ? stats.deathDestroyDelay : 0.6f);
    }
}
