using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAir : PlayerState
{
    PlayerMotion playerMotion;

    public float jumpHeight = 12f;
    public float coyoteMaxTime = 0.5f;
    float coyoteTimeCount = 0;

    private void Awake() 
    {
        playerMotion = GetComponent<PlayerMotion>();
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
    }

    public override void _Enter<T>(T msg)
    {
        playerMotion._Enter();
        gameControls.Player.Jump.performed += JumpPerformed;

        coyoteTimeCount = coyoteMaxTime;
        playerMotion.motion.y = 0;
        playerMotion.motion.y = jumpHeight;
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= JumpPerformed;
    }

    public void JumpPerformed(InputAction.CallbackContext ctx)
    {
        if (coyoteTimeCount < coyoteMaxTime)
            fsm.TransitionTo<PlayerAir, bool>(true);
    }
}