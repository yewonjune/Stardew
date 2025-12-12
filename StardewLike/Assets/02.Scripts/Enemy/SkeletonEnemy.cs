using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonEnemy : EnemyBase
{
    public float reviveDelay = 5f;
    public string dieStateName = "SkeletonDie";
    public AnimationClip dieClip;
    public EnemyMovementController movementController;
    public Collider2D bodyCollider;

    protected override void Die()
    {
        if(isDead) return;

        StartCoroutine(DieAndReviveRoutine());
    }

    private IEnumerator DieAndReviveRoutine()
    {
        isDead = true;

        if(movementController != null)
            movementController.enabled = false;

        if(rb != null)
            rb.velocity = Vector3.zero;

        if(bodyCollider != null)
            bodyCollider.enabled = false;

        if (animator != null && dieClip != null)
        {
            animator.speed = 1f;
            animator.Play(dieStateName, 0, 0f);

            yield return new WaitForSeconds(dieClip.length);

            animator.speed = 0f;
            animator.Play(dieStateName, 0, 1f);
        }

        yield return new WaitForSeconds(reviveDelay);

        yield return StartCoroutine(ReviveReversePlay());

        isDead = false;
        currentHp = stats != null ? stats.maxHp : 20;

        if (movementController != null)
            movementController.enabled = true;

        if (bodyCollider != null)
            bodyCollider.enabled = true;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("SkeletonIdle", 0, 0f);
        }

    }

    private IEnumerator ReviveReversePlay()
    {
        float clipLength = dieClip.length;
        float timer = 0f;

        if (animator != null)
        {
            animator.speed = 0f;
            animator.Play(dieStateName, 0, 1f);
        }

        while (timer < clipLength)
        {
            timer += Time.deltaTime;

            float normalizerd = Mathf.Lerp(1f, 0f, timer / clipLength);

            animator.Play(dieStateName, 0, normalizerd);

            yield return null;
        }
            animator.speed = 1f;

    }
}
