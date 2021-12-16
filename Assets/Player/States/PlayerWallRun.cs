using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWallRun : PlayerState
{
    private PlayerMotion playerMotion;
    public float staminaCost = 75f;
    public float recoverRate = 125f;
    public float recoverDelay = 1.5f;
    public float wallRunSpeed = 5;
    public float gravity = 8f;
    public float maxGravity = 2f;
    public float tiltSmoothness = 5f;
    public float maxCameraTilt = 30;
    private float targetCameraTilt = 0;
    private float cameraTilt = 0;
    private bool runningRight = true;

    private Vector3 motion = Vector3.zero;

    private void Awake()
    {
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

        player.transform.eulerAngles = new Vector3(0, playerMotion.xRotation, 0);

        playerStats.stamina -= staminaCost * Time.deltaTime;
        if (playerStats.stamina <= 0)
            fsm.TransitionTo<PlayerAir>();

        if (controller.isGrounded)
            fsm.TransitionTo<PlayerGround>();
    }

    public override void _Enter<Boolean>(Boolean wallRunRight)
    {
        playerMotion._Enter();
        gameControls.Player.Jump.performed += JumpPerformed;

        motion = player.transform.forward * wallRunSpeed * gameControls.Player.Move.ReadValue<Vector2>().y;
        motion.y = playerMotion.finalMove.y;

        if (wallRunRight.Equals(true))
        {
            motion += player.transform.right;
            runningRight = true;
            targetCameraTilt = maxCameraTilt;
        }
        else
        {
            motion -= player.transform.right;
            runningRight = false;
            targetCameraTilt = -maxCameraTilt;
        }

        CancelInvoke("RecoverStamina");
    }

    public override void _Exit()
    {
        playerMotion._Exit();
        gameControls.Player.Jump.performed -= JumpPerformed;

        Invoke("RecoverStamina", recoverDelay);

        targetCameraTilt = 0;
    }

    public void JumpPerformed(InputAction.CallbackContext ctx)
    {
        if (runningRight)
        {
            playerMotion.motion.x = -wallRunSpeed;
        }
        else
        {
            playerMotion.motion.x = wallRunSpeed;
        }

        fsm.TransitionTo<PlayerAir, bool>(true);
    }

    private bool recoverStamina = false;

    private void Update()
    {
        cameraTilt = Mathf.MoveTowards(cameraTilt, targetCameraTilt, tiltSmoothness * Time.deltaTime);
        mainCamera.transform.localEulerAngles = new Vector3(playerMotion.yRotation, 0, cameraTilt);

        if (!recoverStamina) return;
        playerStats.stamina += recoverRate * Time.deltaTime;
        if (playerStats.stamina >= playerStats.maxStamina) recoverStamina = false;
    }

    private void RecoverStamina()
    {
        recoverStamina = true;
    }
}