using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrouch : PlayerState
{
    PlayerMotion playerMotion;
    public float crouchSpeed = 5f;
    public float normalHeight = 2f;
    public float crouchHeight = 1f;

    private void Awake() 
    {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _Update()
    {
        playerMotion._Update();
    }

    public override void _Enter()
    {
        playerMotion._Enter();
        playerMotion.moveSpeed = crouchSpeed;
        gameControls.Player.Jump.performed += StandUp;
        gameControls.Player.Crouch.performed += StandUp;

        controller.height = crouchHeight;
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= StandUp;
        gameControls.Player.Crouch.performed -= StandUp;

        controller.height = normalHeight;
    }

    void StandUp(InputAction.CallbackContext ctx)
    {
        fsm.TransitionTo<PlayerGround>();
    }
}
