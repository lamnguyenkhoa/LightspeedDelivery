using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// PlayerMotion is continuous state, not transistioned into?
public class PlayerMotion : PlayerState
{
    public float cameraBobSmoothness = 20.0f;
    public PlayerDash playerDash;
    public float acceleration = 15f;
    public float brake = 15f;
    public float decceleration = 30f;
    public float normalSpeed = 14f;
    public float moveSpeed = 14f;
    public float runLimit = 10f;
    public bool isSprinting = false;
    public float gravity = 9.8f;
    public float floorGravity = 2;
    public float maxGravity = 20f;
    public LineRenderer aimRenderer;
    public LayerMask glassLayer;

    [HideInInspector] public Vector2 moveDirection = Vector2.zero;
    [HideInInspector] public float xRotation = 0;
    [HideInInspector] public float yRotation = 0;
    public Vector3 motion = Vector3.zero;
    [HideInInspector] public Vector3 finalMove = Vector3.zero;

    private bool isAiming = false;

    private void Awake()
    {
        playerDash = GetComponent<PlayerDash>();
    }

    public override void _Update()
    {
        moveDirection = gameControls.Player.Move.ReadValue<Vector2>();

        if (moveDirection != Vector2.zero)
        {
            motion += new Vector3(moveDirection.x, 0, moveDirection.y) * acceleration * Time.deltaTime;

            // Extra brake force if go opposite direction
            if (motion.x * moveDirection.x < 0)
                motion += new Vector3(moveDirection.x, 0, 0) * brake * Time.deltaTime;
            if (motion.z * moveDirection.y < 0)
                motion += new Vector3(0, 0, moveDirection.y) * brake * Time.deltaTime;
        }
        else
        {
            if (controller.isGrounded)
                motion = Vector3.MoveTowards(motion, Vector3Ext.ClampPlaneAxis(motion, 0), decceleration * Time.deltaTime);
        }

        motion = Vector3Ext.ClampPlaneAxis(motion, moveSpeed);

        if (motion.magnitude > runLimit)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

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

        player.transform.eulerAngles = new Vector3(0, xRotation, 0);
        mainCamera.transform.localEulerAngles = new Vector3(yRotation, 0, mainCamera.transform.localEulerAngles.z);

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, headPos.transform.position, cameraBobSmoothness * Time.deltaTime);

        anim.SetFloat("HeadLook", (yRotation + 90f) / 180f, 1f, Time.deltaTime * 10f);

        RenderAiming();
    }

    public override void _Enter()
    {
        gameControls.Player.CameraMouse.performed += SetCameraDirection;
        gameControls.Player.Dash.performed += ActivateDashAim;
        gameControls.Player.Dash.canceled += ReleaseDashAim;
        gameControls.Player.Cancel.performed += CancelAim;
    }

    public override void _Exit()
    {
        gameControls.Player.CameraMouse.performed -= SetCameraDirection;
        gameControls.Player.Dash.performed -= ActivateDashAim;
        gameControls.Player.Dash.canceled -= ReleaseDashAim;
        gameControls.Player.Cancel.performed -= CancelAim;
    }

    private void SetCameraDirection(InputAction.CallbackContext ctx)
    {
        xRotation += ctx.ReadValue<Vector2>().x * playerStats.mouseSensitivity;
        yRotation -= ctx.ReadValue<Vector2>().y * playerStats.mouseSensitivity;

        yRotation = Mathf.Clamp(yRotation, -90, 90);
    }

    private void CancelAim(InputAction.CallbackContext ctx)
    {
        isAiming = false;
        aimRenderer.positionCount = 0;
    }

    private void ActivateDashAim(InputAction.CallbackContext ctx)
    {
        isAiming = true;
    }

    private void ReleaseDashAim(InputAction.CallbackContext ctx)
    {
        if (isAiming && playerStats.power >= playerDash.dashCost)
        {
            moveDirection = Vector2.zero;
            motion = Vector3.zero;
            fsm.TransitionTo<PlayerDash>();
        }
        isAiming = false;
        aimRenderer.positionCount = 0;
    }

    private void RenderAiming()
    {
        if (isAiming)
        {
            List<Vector3> drawPoints = new List<Vector3>();
            drawPoints.Add(aimRenderer.transform.position);
            MirrorRaycast(mainCamera.transform.position, mainCamera.transform.forward, new List<GameObject>(), drawPoints);
            aimRenderer.positionCount = drawPoints.Count;
            aimRenderer.SetPositions(drawPoints.ToArray());
        }
    }

    private void MirrorRaycast(Vector3 position, Vector3 direction, List<GameObject> bouncedMirror, List<Vector3> drawPoints)
    {
        Ray ray = new Ray(position, direction);
        RaycastHit hit;
        bool continueBounce = false;

        if (Physics.Raycast(ray, out hit, playerDash.sunraySpeed * playerDash.sunrayTime, ~glassLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Mirror")
            {
                if (!bouncedMirror.Contains(hit.collider.gameObject))
                {
                    direction = Vector3.Reflect(direction, hit.normal);
                    position = hit.point;
                    bouncedMirror.Add(hit.collider.gameObject);
                    continueBounce = true;
                }
            }
            else
            {
                position = hit.point;
            }
        }
        else
        {
            position += direction * playerDash.sunraySpeed * playerDash.sunrayTime;
        }
        drawPoints.Add(position);

        if (continueBounce)
        {
            MirrorRaycast(position, direction, bouncedMirror, drawPoints);
        }
    }
}