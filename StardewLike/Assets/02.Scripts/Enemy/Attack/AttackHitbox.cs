using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public float hitCooldown = 0.2f;

    float lastHitTime;
    public EnemyBase enemyBase;

    private void Awake()
    {
        if(!enemyBase) enemyBase = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time - lastHitTime < hitCooldown) return;

        var hp = collision.GetComponentInParent<PlayerHealthController>();
        if (hp == null) return;

        lastHitTime = Time.time;

        int finalDamage = enemyBase != null && enemyBase.stats != null ? enemyBase.stats.attackPower : damage;
        hp.TakeDamage(finalDamage);
    }
}

