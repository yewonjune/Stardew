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

    [Min(0f)]
    public float stunDuration = 0.1f;

    // 왼쪽: false / 오른쪽 : true
    public bool defaultFacingRight = false;

    [SerializeField] DamagePopup damagePopupPrefab;
    [SerializeField] Vector3 damagePopupOffset = new Vector3(0f, 0.8f, 0f);

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

        if (damagePopupPrefab != null)
        {
            var popup = Instantiate(damagePopupPrefab, transform.position + damagePopupOffset, Quaternion.identity);
            popup.Play(damage, isCrit: false);
        }

        if (currentHp <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("Hit");

        if (hitDir.sqrMagnitude > 0.0001f && knockbackDuration > 0f && knockbackDistance > 0f)
        {
            // 진행 중인 Tween 정리
            transform.DOKill();
            if (rb != null) rb.velocity = Vector2.zero;

            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + (Vector3)hitDir.normalized * knockbackDistance;

            IsKnockback = true;   // Movement 쪽에서 이 동안 멈추게 됨

            transform.DOMove(targetPos, knockbackDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (rb != null) rb.velocity = Vector2.zero;

                    if (stunDuration > 0f)
                    {
                        // 넉백 끝난 뒤 잠깐 더 멈추기
                        StartCoroutine(StunWait());
                    }
                    else
                    {
                        IsKnockback = false;
                    }
                });
        }
        else
        {
            // 넉백은 없지만, 잠깐 스턴만 쓰고 싶을 수도 있음
            if (stunDuration > 0f)
            {
                StartCoroutine(StunWait());
            }
        }
    }

    IEnumerator StunWait()
    {
        IsKnockback = true;
        yield return new WaitForSeconds(stunDuration);
        IsKnockback = false;
    }

    protected virtual void Die()
    {
        isDead = true;

        if (stats != null)
        {
            QuestManager.I?.OnEnemyKilled(stats.enemyId);
        }

        if (animator)
            animator.SetTrigger("Die");

        Destroy(gameObject, stats != null ? stats.deathDestroyDelay : 0.6f);
    }
}
