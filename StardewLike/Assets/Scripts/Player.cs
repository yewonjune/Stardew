using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float moveSpeed = 3.0f;
    Animator animator;
    Rigidbody2D player;

    Vector2 lastDirection = Vector2.down;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //this.animator.SetTrigger("AM Player Idle");
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 input = new Vector2( Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));

        bool isMoving = input != Vector2.zero;

        // 마지막 방향 계산 (정제된 4방향)
        if (isMoving)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                lastDirection = new Vector2(input.x > 0 ? 1 : -1, 0); // 좌우
            else
                lastDirection = new Vector2(0, input.y > 0 ? 1 : -1); // 상하
        }

        Vector2 animDir = lastDirection;

        //애니메이터에 파라미터 전달
        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("MoveX", animDir.x);
        animator.SetFloat("MoveY", animDir.y);

        // 실제 이동 처리
        player.velocity = isMoving ? animDir * moveSpeed : Vector2.zero;

        // 좌우 반전 처리 (애니메이션 재활용용)
        if (animDir.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (animDir.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

    }
}
