using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public DialogueData dialogueData;

    Animator animator;
    Rigidbody2D rigidbody2D;

    public float speed;
    public Vector3 target;
    private bool moving;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!moving) return;

        Vector3 dir = (target - transform.position).normalized;
        Vector3 newPos = Vector3.MoveTowards(rigidbody2D.position, target, speed * Time.fixedDeltaTime);
        rigidbody2D.MovePosition(newPos);

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetBool("isMoving", dir.magnitude > 0.01f);

        // 목표에 거의 도착하면 멈추기
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            moving = false;
            animator.SetBool("isMoving", false);
        }
    }

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogueData);
    }

    public void SetTarget(Vector3 pos)
    {
        target = pos;
        moving = true;
    }
}
