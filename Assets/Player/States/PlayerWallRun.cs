using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWallRun : PlayerState
{
    PlayerMotion playerMotion;
    public float wallRunSpeed = 5;
    public float gravity = 8f;
    public float maxGravity = 2f;
    public float cameraTilt = 0;
    public float maxCameraTilt = 0;
    bool runningRight = true;

    Vector3 motion = Vector3.zero;

    private void Awake() {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _FixedUpdate()
    {
        bool isWallRight = Physics.Raycast(transform.position, transform.right, 1f);
        bool isWallLeft = Physics.Raycast(transform.position, -transform.right, 1f);

        if (!isWallLeft && !isWallRight)
            fsm.TransitionTo<PlayerAir>();
    }

    public override void _Update()
    {
        controller.Move(motion * Time.deltaTime);

        motion.y -= gravity * Time.deltaTime;
        motion.y = Mathf.Max(motion.y, -maxGravity);

        if (controller.isGrounded)
            fsm.TransitionTo<PlayerGround>();

        player.transform.eulerAngles = new Vector3(0, playerMotion.xRotation, 0);
        mainCamera.transform.localEulerAngles = new Vector3(playerMotion.yRotation, 0, 0);
    }

    public override void _Enter<Boolean>(Boolean wallRunRight)
    {
        playerMotion._Enter();
        gameControls.Player.Jump.performed += JumpPerformed;

        motion = player.transform.forward * wallRunSpeed;
        motion.y = playerMotion.finalMove.y;

        if (wallRunRight.Equals(true))
        {
            motion += player.transform.right;
            runningRight = true;
        }
        else
        {
            motion -= player.transform.right;
            runningRight = false;
        }
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= JumpPerformed;
    }

    public void JumpPerformed(InputAction.CallbackContext ctx)
    {
        if (runningRight)
            playerMotion.motion -= player.transform.right * wallRunSpeed;
        else
            playerMotion.motion += player.transform.right * wallRunSpeed;
        fsm.TransitionTo<PlayerAir, bool>(true);
    }
}
