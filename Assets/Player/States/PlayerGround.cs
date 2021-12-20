using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGround : PlayerState
{
    private PlayerMotion playerMotion;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _Update()
    {
        playerMotion._Update();

        anim.SetFloat("WalkForward", playerMotion.moveDirection.y, 1f, Time.deltaTime * 3f);
        anim.SetFloat("WalkRight", playerMotion.moveDirection.x, 1f, Time.deltaTime * 3f);

        if (!controller.isGrounded)
        {
            fsm.TransitionTo<PlayerAir>();
        }
    }

    public override void _Enter()
    {
        playerMotion._Enter();

        gameControls.Player.Jump.performed += JumpPerformed;
        gameControls.Player.Crouch.performed += CrouchPerformed;

        anim.ResetTrigger("jump");
        anim.SetBool("isFalling", false);
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= JumpPerformed;
        gameControls.Player.Crouch.performed -= CrouchPerformed;
    }

    public void JumpPerformed(InputAction.CallbackContext ctx)
    {
        fsm.TransitionTo<PlayerAir, bool>(false);
    }

    private void CrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (playerMotion.isSprinting)
            fsm.TransitionTo<PlayerSlide>();
        else
            fsm.TransitionTo<PlayerCrouch>();
    }
}