using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float moveSpeed = 6.0f;

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

    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        bool isMoving = input != Vector2.zero;

        if (isMoving)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                lastDirection = new Vector2(input.x > 0 ? 1 : -1, 0);
            else
                lastDirection = new Vector2(0, input.y > 0 ? 1 : -1);
        }

        Vector2 animDir = lastDirection;

        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("MoveX", animDir.x);
        animator.SetFloat("MoveY", animDir.y);

        player.velocity = isMoving ? animDir * moveSpeed : Vector2.zero;

        if (animDir.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (animDir.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

    }
}
