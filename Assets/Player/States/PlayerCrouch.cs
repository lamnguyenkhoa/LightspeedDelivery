using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrouch : PlayerState
{
    private PlayerMotion playerMotion;
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
        if (controller.height != crouchHeight)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, 0.1f);
            Debug.Log(crouchHeight);
        }
    }

    public override void _Enter()
    {
        playerMotion._Enter();
        playerMotion.moveSpeed = crouchSpeed;
        gameControls.Player.Crouch.canceled += StandUp;
        anim.SetBool("isStanding", false);
        player.model.localPosition = new Vector3(0, -0.5f, 0);
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Crouch.canceled -= StandUp;
        anim.SetBool("isStanding", true);
        player.model.localPosition = new Vector3(0, -1, 0);
    }

    private void StandUp(InputAction.CallbackContext ctx)
    {
        fsm.TransitionTo<PlayerGround>();
    }
}