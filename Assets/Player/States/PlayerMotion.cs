using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotion : PlayerState
{
    public float mouseSensitivity = 0.5f;
    public float acceleration = 15f;
    public float decceleration = 20f;
    public float moveSpeed = 9f;
    public float gravity = 9.8f;
    public float floorGravity = 2;
    public float maxGravity = 20f;

    [HideInInspector] public Vector2 moveDirection = Vector2.zero;
    float xRotation = 0;
    float yRotation = 0;
    [HideInInspector] public Vector3 motion = Vector3.zero;
    [HideInInspector] public Vector3 finalMove = Vector3.zero;

    public override void _Update() 
    {
        moveDirection = gameControls.Player.Move.ReadValue<Vector2>();

        if (moveDirection != Vector2.zero)
        {
            motion += new Vector3(moveDirection.x, 0, moveDirection.y) * acceleration * Time.deltaTime;
        }
        else
        {
            if (controller.isGrounded)
                motion = Vector3.MoveTowards(motion, Vector3Ext.ClampPlaneAxis(motion, 0), decceleration * Time.deltaTime);
        }
        
        motion = Vector3Ext.ClampPlaneAxis(motion, moveSpeed);
        
        finalMove = motion.x * player.transform.right + motion.y * Vector3.up + motion.z * player.transform.forward;

        controller.Move(finalMove * Time.deltaTime);

        if (!controller.isGrounded)
        {
            motion.y -= gravity * Time.deltaTime;
        }
        else
        {
            motion.y = -floorGravity;
        }

        motion.y = Mathf.Max(motion.y, -maxGravity);

        player.transform.eulerAngles = new Vector3(player.transform.eulerAngles.x, xRotation, player.transform.eulerAngles.z);
        mainCamera.transform.eulerAngles = new Vector3(yRotation, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);
    }

    public override void _Enter()
    {
        gameControls.Player.CameraMouse.performed += SetCameraDirection;
        gameControls.Player.Dash.performed += SetDash;
    }

    public override void _Exit()
    {
        gameControls.Player.CameraMouse.performed -= SetCameraDirection;
        gameControls.Player.Dash.performed -= SetDash;
    }

    void SetCameraDirection(InputAction.CallbackContext ctx)
    {
        xRotation += ctx.ReadValue<Vector2>().x * mouseSensitivity;
        yRotation -= ctx.ReadValue<Vector2>().y * mouseSensitivity;

        yRotation = Mathf.Clamp(yRotation, -90, 90);
    }

    void SetDash(InputAction.CallbackContext ctx)
    {
        moveDirection = Vector2.zero;
        motion = Vector3.zero;
        
        fsm.TransitionTo<PlayerDash>();
    }
}
