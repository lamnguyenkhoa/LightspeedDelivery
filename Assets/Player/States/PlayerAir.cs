using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAir : PlayerState
{
    private PlayerMotion playerMotion;

    public float jumpHeight = 12f;
    public float coyoteMaxTime = 0.5f;
    private float coyoteTimeCount = 0;

    public float wallCheckTime = 0.25f;
    private bool checkWall = true;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _FixedUpdate()
    {
        if (!checkWall) return;

        bool isWallRight = Physics.Raycast(transform.position, transform.right, 1f);
        bool isWallLeft = Physics.Raycast(transform.position, -transform.right, 1f);

        if (gameControls.Player.Move.ReadValue<Vector2>().x < 0 && isWallLeft)
            fsm.TransitionTo<PlayerWallRun, bool>(false);
        else if (gameControls.Player.Move.ReadValue<Vector2>().x > 0 && isWallRight)
            fsm.TransitionTo<PlayerWallRun, bool>(true);
    }

    public override void _Update()
    {
        playerMotion._Update();

        if (coyoteTimeCount <= coyoteMaxTime)
            coyoteTimeCount += Time.deltaTime;

        if (controller.isGrounded)
        {
            fsm.TransitionTo<PlayerGround>();
        }
    }

    public override void _Enter()
    {
        playerMotion._Enter();
        gameControls.Player.Jump.performed += JumpPerformed;
        coyoteTimeCount = 0;
        checkWall = true;
        anim.SetBool("isFalling", true);
    }

    public override void _Enter<Boolean>(Boolean fromWall)
    {
        playerMotion._Enter();
        gameControls.Player.Jump.performed += JumpPerformed;

        coyoteTimeCount = coyoteMaxTime;
        playerMotion.motion.y = 0;
        playerMotion.motion.y = jumpHeight;

        if (fromWall.Equals(true))
        {
            checkWall = false;
            Invoke("SetCheckWall", wallCheckTime);
        }
        else
            checkWall = true;

        anim.SetTrigger("jump");
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= JumpPerformed;
        CancelInvoke("SetCheckWall");
    }

    public void JumpPerformed(InputAction.CallbackContext ctx)
    {
        if (coyoteTimeCount < coyoteMaxTime)
            fsm.TransitionTo<PlayerAir, bool>(false);
    }

    private void SetCheckWall()
    {
        checkWall = true;
    }
}