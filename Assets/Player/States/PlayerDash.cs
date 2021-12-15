using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : PlayerState
{
    public float sunraySpeed = 50f;
    public float sunrayTime = 1.0f;
    Vector3 dashDirection = Vector3.zero;

    public override void _Update()
    {
        controller.Move(dashDirection * sunraySpeed * Time.deltaTime);
    }

    public override void _Enter()
    {
        eventsManager.PlayerDashStarted();
        dashDirection = mainCamera.transform.forward;

        Invoke("DashEnded", sunrayTime);
    }

    public override void _Exit()
    {
        eventsManager.PlayerDashEnded();
        
        CancelInvoke("DashEnded");
    }

    void DashEnded()
    {
        fsm.TransitionTo<PlayerAir>();
    }
}
