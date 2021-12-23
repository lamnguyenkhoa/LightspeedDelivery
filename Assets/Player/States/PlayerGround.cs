using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGround : PlayerState
{
    private PlayerMotion playerMotion;
    public float standHeight = 2f;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _Update()
    {
        playerMotion._Update();

        if (!controller.isGrounded)
        {
            fsm.TransitionTo<PlayerAir>();
        }

        if (controller.height != standHeight)
        {
            controller.height = Mathf.Lerp(controller.height, standHeight, 0.1f);
        }
    }

    public override void _Enter()
    {
        playerMotion._Enter();
        playerMotion.moveSpeed = playerMotion.normalSpeed;
        gameControls.Player.Jump.performed += JumpPerformed;
        gameControls.Player.Crouch.performed += CrouchPerformed;

        anim.ResetTrigger("jump");
        anim.ResetTrigger("slide");
        anim.SetBool("isFalling", false);
        anim.SetBool("isStanding", true);
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