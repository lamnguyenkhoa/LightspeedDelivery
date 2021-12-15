using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGround : PlayerState
{
    PlayerMotion playerMotion;

    public float runSpeed = 9f;
    public float sprintSpeed = 14f;

    bool sprinting = false;

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
        
        playerMotion.moveSpeed = runSpeed;
        gameControls.Player.Jump.performed += JumpPerformed;
        gameControls.Player.Crouch.performed += CrouchPerformed;

        gameControls.Player.Sprint.performed += SetSprint;
        gameControls.Player.Sprint.canceled += CancelSprint;

        if (gameControls.Player.Sprint.ReadValue<float>() != 0)
            SetSprint();
        else
            CancelSprint();

        anim.ResetTrigger("jump");
        anim.SetBool("isFalling", false);
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= JumpPerformed;
        gameControls.Player.Crouch.performed -= CrouchPerformed;
        
        gameControls.Player.Sprint.performed -= SetSprint;
        gameControls.Player.Sprint.canceled -= CancelSprint;
    }

    public void JumpPerformed(InputAction.CallbackContext ctx)
    {
        fsm.TransitionTo<PlayerAir, bool>(false);
    }

    void CrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (sprinting)
            fsm.TransitionTo<PlayerSlide>();
        else
            fsm.TransitionTo<PlayerCrouch>();
    }

    void SetSprint()
    {
        sprinting = true;
        playerMotion.moveSpeed = sprintSpeed;
    }

    void SetSprint(InputAction.CallbackContext ctx)
    {
        SetSprint();
    }

    void CancelSprint()
    {
        sprinting = false;
        playerMotion.moveSpeed = runSpeed;
    }

    void CancelSprint(InputAction.CallbackContext ctx)
    {
        CancelSprint();
    }
}
