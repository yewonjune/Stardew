using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public EnemyStats stats;

    protected Animator animator;
    protected int currentHp;
    protected bool isDead = false;
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        currentHp = stats != null ? stats.maxHp : 10;
    }
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;
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
