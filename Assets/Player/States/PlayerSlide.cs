using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : PlayerState
{
    PlayerMotion playerMotion;
    public float slideSpeed = 16f;
    public float slideAcceleration = 45f;
    public float slideTime = 1.5f;
    public float normalHeight = 2f;
    public float slideHeight = 0.7f;
    public float floorGravity = 2;
    Vector3 slideDirection = Vector3.zero;
    Vector3 motion = Vector3.zero;

    private void Awake() 
    {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _Update()
    {
        motion += slideDirection * slideAcceleration * Time.deltaTime;
        motion = Vector3Ext.ClampPlaneAxis(motion, slideSpeed);
        controller.Move(motion * Time.deltaTime);
        
        motion.y = -floorGravity;

        // if (!controller.isGrounded)
        // {
        //     fsm.TransitionTo<PlayerAir>();
        // }
    }

    public override void _Enter()
    {
        gameControls.Player.Jump.performed += JumpPerformed;
        gameControls.Player.Crouch.canceled += CancelSlide;

        controller.height = slideHeight;
        motion = playerMotion.finalMove;
        slideDirection = player.transform.forward;

        Invoke("SlideEnded", slideTime);
    }

    public override void _Exit()
    {
        gameControls.Player.Jump.performed -= JumpPerformed;
        gameControls.Player.Crouch.canceled -= CancelSlide;

        controller.height = normalHeight;

        CancelInvoke("SlideEnded");
    }

    void JumpPerformed(InputAction.CallbackContext ctx)
    {
        fsm.TransitionTo<PlayerAir, bool>(true);
    }

    void CancelSlide(InputAction.CallbackContext ctx)
    {
        SlideEnded();
    }

    void SlideEnded()
    {
        fsm.TransitionTo<PlayerGround>();
    }
}
