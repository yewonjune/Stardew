using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStaminaController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 6.5f;

    [SerializeField] float drainPerSecond = 20f;
    [SerializeField] float regenPerSecond = 15f;
    [SerializeField] KeyCode runKey = KeyCode.Space;

    PlayerMovement playerMovement;

    public float maxStamina;
    public float stamina;
    bool isRunning;
    bool exhausted;

    void Awake()
    {
        if (!playerMovement) playerMovement = GetComponent<PlayerMovement>();
        stamina = maxStamina;

        if (playerMovement)
            playerMovement.SetSpeed(walkSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        HandleRunLogic();
        ApplySpeed();
    }

    void HandleRunLogic()
    {
        bool moving = IsMoving();
        bool wantRun = Input.GetKey(runKey);
        bool canRun = !exhausted && stamina > 0.01f;

        isRunning = moving && wantRun && canRun;

        if (isRunning)
        {
            stamina -= drainPerSecond * Time.deltaTime;
            if (stamina <= 0f)
            {
                stamina = 0f;
                exhausted = true;
                Debug.Log("스태미나 고갈");
            }
        }
        else
        {
            if (stamina < maxStamina)
                stamina += regenPerSecond * Time.deltaTime;

            if (stamina >= maxStamina)
            {
                stamina = maxStamina;

                if (exhausted) exhausted = false;
            }
        }
    }

    void ApplySpeed()
    {
        if (!playerMovement) return;
        playerMovement.SetSpeed(isRunning ? runSpeed : walkSpeed);
    }

    bool IsMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(h) + Mathf.Abs(v) > 0.01f;
    }

    public void ResetStamina()
    {
        stamina = maxStamina;
        exhausted = false;
    }
}
