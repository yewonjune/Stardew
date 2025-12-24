using System.Collections;
using UnityEngine;

public class NPC_CutsceneActor : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator animator;

    [Header("Move")]
    [SerializeField] float moveSpeed = 2.2f;
    Coroutine moveCo;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    public void Stop()
    {
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = null;
        SetWalking(false, Vector2.zero);
    }

    public void MoveTo(Vector3 worldTarget, System.Action onArrive = null)
    {
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(CoMoveTo(worldTarget, onArrive));
    }

    IEnumerator CoMoveTo(Vector3 target, System.Action onArrive)
    {
        target.z = transform.position.z;

        while (true)
        {
            Vector3 delta = target - transform.position;
            float dist = delta.magnitude;

            if (dist <= 0.02f) break;

            Vector2 dir = ((Vector2)delta).normalized;
            SetWalking(true, dir);

            float step = moveSpeed * Time.deltaTime;
            transform.position += (Vector3)(dir * step);

            yield return null;
        }

        transform.position = target;
        SetWalking(false, Vector2.zero);

        moveCo = null;
        onArrive?.Invoke();
    }

    void SetWalking(bool walking, Vector2 dir)
    {
        if (!animator) return;

        animator.SetBool("IsMoving", walking);

        if (dir.sqrMagnitude > 0.0001f)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }
    }

    public void FaceTo(Vector3 worldPos)
    {
        Vector2 dir = ((Vector2)(worldPos - transform.position)).normalized;
        if (dir.sqrMagnitude < 0.0001f) return;

        SetWalking(false, dir);
    }
}
