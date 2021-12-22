using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : PlayerState
{
    private PlayerMotion playerMotion;
    public float slideSpeed = 16f;
    public float slideAcceleration = 45f;
    public float slideTime = 1.5f;
    public float normalHeight = 2f;
    public float slideHeight = 0.7f;
    public float floorGravity = 2;
    private Vector3 slideDirection = Vector3.zero;
    private Vector3 motion = Vector3.zero;

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
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position,
            headPos.transform.position, playerMotion.cameraBobSmoothness * Time.deltaTime);

        if (controller.height != slideHeight)
        {
            controller.height = Mathf.Lerp(controller.height, slideHeight, 0.1f);
        }
    }

    public override void _Enter()
    {
        gameControls.Player.Jump.performed += JumpPerformed;
        gameControls.Player.Crouch.canceled += CancelSlide;

        motion = playerMotion.finalMove;
        slideDirection = player.transform.forward;
        anim.SetTrigger("slide");
        anim.SetBool("isStanding", false);

        Invoke("SlideEnded", slideTime);
        player.model.localPosition = new Vector3(0, -0.7f, 0);
    }

    public override void _Exit()
    {
        gameControls.Player.Jump.performed -= JumpPerformed;
        gameControls.Player.Crouch.canceled -= CancelSlide;

        CancelInvoke("SlideEnded");
        player.model.localPosition = new Vector3(0, -1, 0);
    }

    private void JumpPerformed(InputAction.CallbackContext ctx)
    {
        fsm.TransitionTo<PlayerAir, bool>(true);
    }

    private void CancelSlide(InputAction.CallbackContext ctx)
    {
        SlideEnded();
    }

    private void SlideEnded()
    {
        fsm.TransitionTo<PlayerGround>();
    }
}