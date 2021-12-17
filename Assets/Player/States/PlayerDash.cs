using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : PlayerState
{
    private PlayerMotion playerMotion;
    public float dashCost = 50f;
    public float sunraySpeed = 50f;
    public float sunrayTime = 1.0f;
    public float detectionDistance = 1.5f;
    public float smoothRotation = 1000f;
    public float timer;
    public LayerMask mirrorMask;
    private Vector3 dashDirection = Vector3.zero;

    private void Awake()
    {
        playerMotion = GetComponent<PlayerMotion>();
    }

    public override void _FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dashDirection, out hit, detectionDistance, mirrorMask, QueryTriggerInteraction.Ignore))
        {
            AudioManager.instance.PlayDash();
            Vector3 reflectDirection = Vector3.Reflect(dashDirection, hit.normal);
            transform.position = hit.point;
            dashDirection = reflectDirection;
            timer = sunrayTime;
        }
    }

    public override void _Update()
    {
        controller.Move(dashDirection * sunraySpeed * Time.deltaTime);

        Vector3 dashEulerRotation = Quaternion.LookRotation(dashDirection).eulerAngles;
        playerMotion.yRotation = dashEulerRotation.x;
        playerMotion.xRotation = dashEulerRotation.y;

        Quaternion bodyRotation = Quaternion.Euler(new Vector3(0, playerMotion.xRotation, 0));
        Quaternion cameraRotation = Quaternion.Euler(new Vector3(playerMotion.yRotation, 0, 0));

        if (playerMotion.yRotation > 90)
        {
            playerMotion.yRotation = playerMotion.yRotation - 360f;
        }

        player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, bodyRotation, smoothRotation * Time.deltaTime);
        mainCamera.transform.localRotation = Quaternion.RotateTowards(mainCamera.transform.localRotation, cameraRotation, smoothRotation * Time.deltaTime);
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            DashEnded();
        }
    }

    public override void _Enter()
    {
        eventsManager.PlayerDashStarted();
        dashDirection = mainCamera.transform.forward;
        playerStats.power -= dashCost;
        foodGun.DeactivateGun();
        timer = sunrayTime;
    }

    public override void _Exit()
    {
        eventsManager.PlayerDashEnded();
        foodGun.ActivateGun();
    }

    private void DashEnded()
    {
        fsm.TransitionTo<PlayerAir>();
    }
}