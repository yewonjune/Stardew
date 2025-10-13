using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool debugLog = false;

    float moveSpeed = 6.0f;
    float speedLerp = 20f;

    float setSpeed;
    float currentSpeed;

    Animator animator;
    Rigidbody2D player;

    public Vector2 lastDirection = Vector2.down;

    bool canControl = true;

    public bool IsControllable => canControl;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Rigidbody2D>();

        setSpeed = moveSpeed;
        currentSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (GamePause.isPaused || DialogueManager.IsBusy) return;

        currentSpeed = Mathf.Lerp(currentSpeed, setSpeed, Time.deltaTime * speedLerp);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (!canControl)
        {
            if (player) player.velocity = Vector2.zero;
            if (animator)
            {
                animator.SetFloat("MoveX", lastDirection.x);
                animator.SetFloat("MoveY", lastDirection.y);
                animator.SetBool("isMoving", false);
            }
            return;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (debugLog)
            Debug.Log($"[PM] input=({input.x},{input.y}) canControl={canControl}");

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

        if (isMoving)
            player.MovePosition(player.position + animDir * moveSpeed * Time.fixedDeltaTime);
        else
            player.velocity = Vector2.zero;

        if (animDir.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        else if (animDir.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
    }


    public void SetSpeed(float value)
    {
        moveSpeed = value;
    }

    public void SetControl(bool enable)
    {
        canControl = enable;
        if (!enable && player) player.velocity = Vector2.zero;
    }
}
