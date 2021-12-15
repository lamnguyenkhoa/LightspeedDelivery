using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region Variables

    // Variables shared with Finite State Machine
    public MainInstances mainInstances;
    public EventsManager eventsManager;
    public PlayerStats playerStats;
    [HideInInspector] public GameControls gameControls;
    [HideInInspector] public FiniteStateMachine fsm;

    [HideInInspector] public Vector3 motion;
    public FoodGun foodGun;
    public Animator anim;
    // End of veriables shared with Finite State Machine

    [Space, Header("Camera")]
    public float mouseSensitivity = 100f;
    public Camera mainCamera;
    public float xRotation;
    public Transform headPos;

    private float maxAchievedFallSpeed = 0f;

    [Space, Header("Movement")]
    public float moveSpeed = 7f;
    public float sprintSpeed = 12f;
    public float crouchSpeed = 4;
    public float currentSpeed;
    public float smoothSpeed;
    public float jumpHeight = 4f;
    public float gravity = -9.8f;
    private float maxGravity = -100f;
    private Vector3 move; // player controlled movement
    private Vector3 smoothMove;
    private Vector3 finalMove;
    public LayerMask glassMask;
    private bool isRunning;
    public Vector3 velocity; // environmental stuff affect player movement
    private Vector3 dashDirection;
    [HideInInspector] public CharacterController controller;
    private bool reliableIsGrouned; // because controller.isGrounded is damn unreliable
    private float coyoteTime = 0.2f;
    public float airTimer = 0f;
    [SerializeField] private bool isCrouching;
    [SerializeField] private bool isSliding;
    [SerializeField] private bool isSlopeSliding;

    public float slideMaxTime = 1.5f;
    private float slideTimer = 0f;

    [Space, Header("Slope")]
    [SerializeField] private float slopeAngle;
    [SerializeField] private bool slopeCanLongSlide;
    [SerializeField] private bool slopeVerySteep;
    public float minAngleToLongSlide = 15f;
    public float forceSlideLimit = 45f; // must larger than walk slope limit
    public float steepLimit = 60f; // must larger than forceSlideLimit

    private Vector3 slopeNormal;

    [Space, Header("Sunray dash")]
    public float rayDetectionDistance = 1.5f;
    public float sunraySpeed = 20f;
    private bool inSunrayForm = false;
    public float maxPower = 100f;
    private float currentPower;
    public Slider powerSlider;
    public float dashCost = 25f;
    public float powerRecovery = 50f;
    public float timeInSunrayForm = 0.5f;
    private float timer;
    private bool isAiming = false;
    public bool resetCameraAfterDash = false;
    public float maxFOV = 90f;
    private float startFOV;
    public GameObject characterModel;
    public GameObject sunrayModel;
    public LineRenderer aimRenderer;

    // Use int instead of bool to prevent edge case bug
    [HideInInspector] public int nPlantInRange = 0;

    [Space, Header("Food gun")]
    // public GameObject foodGun;
    public FoodBag foodBagPrefab;
    private float currentShootForce = 0f;
    public float maxShootForce = 60f;
    public float shootForceIncreaseRate = 20f;
    public float minShootForce = 5f;
    public int currentFoodBag; // max number must be equal or higher than requiredDeliveryAmount
    public Slider shootForceSlider;
    private bool isGunCharging = false;

    [Space, Header("GUI")]
    // public PlayerStats playerStats;
    public TextMeshProUGUI deliveredText;
    public TextMeshProUGUI foodBagLeftText;
    public int requiredDeliveryAmount = 3; // set at start of misison
    // public int deliveredAmount;

    [Space, Header("Wallrun")]
    public bool enableWallRun = true;
    public float maxStamina = 100f;
    private float currentStamina;
    public Slider staminaSlider;
    public float staminaRecovery = 10f;
    public float staminaRecoveryDelay = 1.5f;
    private float timeSinceLastUseStamina;
    public float wallRunCost = 10f;
    private bool isWallLeft, isWallRight;
    public bool isWallRunning;
    public float wallRunCameraTilt;
    public float maxWallRunCameraTilt;
    public float wallJumpLockControlTime = 1f; // disable player control in this amount of time
    private float wallJumpTimer;
    private float wallJumpDirection; // which side the player will jump toward, either -1 or 1

    [Space, Header("Animation")]
    public bool justJump;
    public bool isFalling;

    [Space, Header("Pause Menu")]
    public bool isPaused;
    public GameObject pauseMenu;

    #endregion Variables

    #region UnityCallbacks

    private void OnEnable() 
    {
        gameControls.Enable();
    }

    private void OnDisable() 
    {
        gameControls.Disable();
    }

    private void Awake()
    {
        playerStats.ResetStats();
        mainInstances.player = this;
        mainInstances.mainCamera = mainCamera;
        fsm = GetComponent<FiniteStateMachine>();

        controller = GetComponent<CharacterController>();
        gameControls = new GameControls();
    }

    private void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
        startFOV = mainCamera.fieldOfView;
        currentPower = 0f;
        nPlantInRange = 0;
        currentFoodBag = requiredDeliveryAmount;
        currentStamina = maxStamina;
        isCrouching = false;
    }

    private void Update() 
    {
        if (playerStats.restorePower) playerStats.power += playerStats.powerRecoverRate * Time.deltaTime;
    }

    // private void Update()
    // {
    //     HandlePause();

    //     if (inSunrayForm)
    //     {
    //         mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, maxFOV, 0.05f);

    //         mainCamera.transform.forward = Vector3.Lerp(mainCamera.transform.forward, dashDirection, 0.05f);

    //         Vector3 dashEulerRotation = Quaternion.LookRotation(dashDirection).eulerAngles;
    //         // Update xRotation so when player go back to Human Form it doesn't reset the camera
    //         // rotation
    //         xRotation = dashEulerRotation.x;
    //         if (xRotation > 90)
    //         {
    //             xRotation = xRotation - 360f;
    //         }

    //         // Smooth rotate these 2 value so player dont feel nauseous
    //         Quaternion bodyRotation = Quaternion.Euler(new Vector3(0, dashEulerRotation.y, 0));
    //         Quaternion cameraRotation = Quaternion.Euler(new Vector3(xRotation, 0, 0));

    //         transform.rotation = Quaternion.Lerp(transform.rotation, bodyRotation, 0.1f);
    //         mainCamera.transform.localRotation = Quaternion.Lerp(mainCamera.transform.localRotation, cameraRotation, 0.1f);

    //         SunrayDash();

    //         if (Time.time > timer + timeInSunrayForm)
    //         {
    //             HumanForm();
    //         }
    //     }
    //     else
    //     {
    //         HandleCamera();

    //         HandleSlope();

    //         HandleAnimationSyncedHeadBob();

    //         HandleMovement();

    //         HandleAnimation();

    //         HandleCrouch();

    //         if (enableWallRun)
    //             HandleWallRun();

    //         HandleGravity();

    //         //HandleLanding();

    //         //HandleHeadBob();

    //         HandleSpecialAbilities();

    //         HandleShootGun();

    //         HandleAnimation();

    //         UpdateGUI();

    //         HandleRecovery();
    //     }
    // }

    #endregion UnityCallbacks

    #region MainFunctions

    private void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                isPaused = true;
                pauseMenu.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                isPaused = false;
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void HandleAnimationSyncedHeadBob()
    {
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, headPos.transform.position, 0.05f);
        // mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, headPos.transform.rotation, 0.1f);
    }

    private void HandleSlope()
    {
        if (reliableIsGrouned)
        {
            // Check raycast downward
            if (Physics.Raycast(transform.position - new Vector3(0, controller.height / 2, 0), Vector3.down, out RaycastHit hitDown, 1f))
            {
                //Debug.Log(hitDown.transform.name);
                slopeNormal = hitDown.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitDown.normal);
            }
            // Check raycast forward
            else if (Physics.Raycast(transform.position + transform.forward * controller.radius, transform.forward, out RaycastHit hitForward, 1f))
            {
                //Debug.Log(hitForward.transform.name);
                slopeNormal = hitForward.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitForward.normal);
            }
            // Check raycast backward
            else if (Physics.Raycast(transform.position - transform.forward * controller.radius, -transform.forward, out RaycastHit hitBackward, 1f))
            {
                //Debug.Log(hitBackward.transform.name);
                slopeNormal = hitBackward.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitBackward.normal);
            }
            // Check raycast rightward
            else if (Physics.Raycast(transform.position + transform.right * controller.radius, transform.right, out RaycastHit hitRightward, 1f))
            {
                //Debug.Log(hitRightward.transform.name);
                slopeNormal = hitRightward.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitRightward.normal);
            }
            // Check raycast leftward
            else if (Physics.Raycast(transform.position - transform.right * controller.radius, -transform.right, out RaycastHit hitLeftward, 1f))
            {
                //Debug.Log(hitLeftward.transform.name);
                slopeNormal = hitLeftward.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitLeftward.normal);
            }
            else
            {
                // It should never reach here
                slopeAngle = -2f;
                slopeCanLongSlide = false;
                slopeVerySteep = true;
                slopeNormal = Vector3.zero;
            }

            // From from `minAngleToLongSlide` to `forceSlideLimit`, player can continuous / long side and stop at will.
            // From `forceSlideLimit` to `steepLimit` force slide on contact, no longer cancellable, but still can jump
            if (minAngleToLongSlide <= slopeAngle && slopeAngle <= steepLimit)
                slopeCanLongSlide = true;
            else
                slopeCanLongSlide = false;

            if (slopeAngle > steepLimit)
                slopeVerySteep = true;
            else
                slopeVerySteep = false;
        }
        else
        {
            slopeAngle = -1f;
            slopeCanLongSlide = false;
            slopeVerySteep = false;
            isSlopeSliding = false;
        }

        if (slopeVerySteep)
        {
            // Use jump/ falling animation

            // jump large value make it jittery
            // -y to make sure it stick to its surface
            controller.Move(new Vector3(slopeNormal.x, -2f, slopeNormal.z) * 5f * Time.deltaTime);
            airTimer = coyoteTime + 1f; // prevent coyote jump
        }

        if (slopeCanLongSlide)
        {
            if ((slopeAngle > forceSlideLimit) || (isSliding && Vector3.Angle(slopeNormal, transform.forward) < 95))
            {
                // Use slide animation
                isSlopeSliding = true;
                slideTimer = slideMaxTime;
                controller.Move(new Vector3(slopeNormal.x, -2f, slopeNormal.z) * sprintSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleAnimation()
    {
        anim.SetFloat("WalkForward", move.z, 1f, Time.deltaTime * 3f);
        anim.SetFloat("WalkRight", move.x, 1f, Time.deltaTime * 3f);
        anim.SetFloat("HeadLook", (xRotation + 90f) / 180f, 1f, Time.deltaTime * 10f);

        if (justJump)
        {
            // Jump up, trigger
            anim.SetTrigger("jump");
            justJump = false;
        }

        if (isFalling && airTimer > coyoteTime)
        {
            // Idle falling
            anim.SetBool("isFalling", true);
        }

        if (reliableIsGrouned && isFalling)
        {
            // Landing
            isFalling = false;
            airTimer = 0f;
            anim.SetBool("isFalling", false);
            maxAchievedFallSpeed = 0f;
            anim.ResetTrigger("jump");
            justJump = false;
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isCrouching)
            {
                isCrouching = false;
                isSliding = false;
                isSlopeSliding = false;
                slideTimer = 0f;
            }
            else
            {
                isCrouching = true;
                if (isRunning)
                {
                    slideTimer = slideMaxTime;
                }
            }
        }

        if (isCrouching)
            controller.height = Mathf.Lerp(controller.height, 1f, 0.2f);
        else
            controller.height = Mathf.Lerp(controller.height, 2f, 0.2f);
    }

    private void HandleWallRun()
    {
        // Tilt camera back
        if (wallRunCameraTilt > 0 && !isWallRunning)
            wallRunCameraTilt -= maxWallRunCameraTilt * 2 * Time.deltaTime;
        if (wallRunCameraTilt < 0 && !isWallRunning)
            wallRunCameraTilt += maxWallRunCameraTilt * 2 * Time.deltaTime;

        if (currentStamina <= 0 || reliableIsGrouned || isCrouching)
        {
            isWallRunning = false;
            return;
        }

        // Check for wall
        isWallRight = Physics.Raycast(transform.position, transform.right, 1f);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, 1f);
        if (!isWallLeft && !isWallRight)
            isWallRunning = false;

        // Start wallrun
        if (((Input.GetKey(KeyCode.D) && isWallRight) ||
            (Input.GetKey(KeyCode.A) && isWallLeft)) && Input.GetKeyDown(KeyCode.Space))
        {
            isWallRunning = true;
        }

        if (isWallRunning)
        {
            // Keep the player from fall down (they still fall down a tiny bit though)
            if (velocity.y < 0) velocity.y = 0;

            // Stick to wall
            if (isWallRight)
                controller.Move(transform.right * Time.deltaTime);
            else
                controller.Move(-transform.right * Time.deltaTime);

            // Consume power
            currentStamina -= wallRunCost * Time.deltaTime;
            timeSinceLastUseStamina = 0f;
        }

        // Camera tilt in 0.5s
        if (Mathf.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
            wallRunCameraTilt += maxWallRunCameraTilt * 2 * Time.deltaTime;
        if (Mathf.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
            wallRunCameraTilt -= maxWallRunCameraTilt * 2 * Time.deltaTime;
    }

    private void HandleShootGun()
    {
        // Shoot food bag
        // "Hold" LMB to charge
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isGunCharging = true;
        }

        if (isGunCharging)
        {
            currentShootForce += shootForceIncreaseRate * Time.deltaTime;
            currentShootForce = Mathf.Clamp(currentShootForce, 0, maxShootForce);
        }

        // Cancel aiming
        if (Input.GetKeyDown(KeyCode.F))
        {
            isGunCharging = false;
            currentShootForce = 0f;
        }

        // Release charged shoot
        if (Input.GetKeyUp(KeyCode.Mouse0) && isGunCharging)
        {
            if (currentShootForce >= minShootForce && currentFoodBag >= 1)
            {
                AudioManager.instance.PlayFoodSplat();
                FoodBag newFoodBag = Instantiate(foodBagPrefab, foodGun.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                newFoodBag.shootDirection = mainCamera.transform.forward;
                newFoodBag.shootForce = currentShootForce;
                // not all momentum yet, only player "active" momentum
                newFoodBag.bonusFromPlayerMomentum = finalMove;
                currentFoodBag -= 1;
            }
            currentShootForce = 0f;
            isGunCharging = false;
        }
    }

    private void HandleSpecialAbilities()
    {
        // Aim Sunray
        if (Input.GetKeyDown(KeyCode.Mouse1))
            isAiming = true;

        // Cancel aiming
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Cancel aiming
            isAiming = false;
            aimRenderer.positionCount = 0;
        }

        // Render aiming sunray
        RenderAiming();

        // Use Sunray
        if (Input.GetKeyUp(KeyCode.Mouse1) && isAiming)
        {
            if (currentPower >= dashCost)
            {
                AudioManager.instance.PlayDash();
                currentPower -= dashCost;
                SunrayForm();
                aimRenderer.positionCount = 0;
                isAiming = false;
                timer = Time.time;
            }
            else
            {
                isAiming = false;
                aimRenderer.positionCount = 0;
                Debug.Log("Not enough power");
            }
        }
    }

    public void HandleCamera()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, startFOV, 0.05f);

        // Camera
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, wallRunCameraTilt);
        transform.Rotate(Vector3.up * mouseX * Time.deltaTime);
    }

    public void HandleGravity()
    {
        // Gravity
        // For some reason, the built-in controller.isGrounded only work if
        // move the character controller downward first
        //isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);
        velocity.y += gravity * Time.deltaTime;
        velocity.x = Mathf.Lerp(velocity.x, 0, 0.005f);
        velocity.z = Mathf.Lerp(velocity.z, 0, 0.005f);

        velocity.y = Mathf.Clamp(velocity.y, maxGravity, float.MaxValue);
        controller.Move(velocity * Time.deltaTime);
        reliableIsGrouned = controller.isGrounded;
        if (reliableIsGrouned)
        {
            velocity.x = 0f;
            velocity.z = 0f;

            if (velocity.y < 0)
            {
                // The velocty.y need to be negative so controller.isGrounded
                // work properly
                velocity.y = -2f;
                // Reset step offset
                controller.stepOffset = 0.3f;
            }
        }
        else
        {
            // Prevent character jittering when jump and move forward
            // near an wall's edge
            controller.stepOffset = 0f;
            isFalling = true;
            airTimer += Time.deltaTime;
            maxAchievedFallSpeed = Mathf.Max(maxAchievedFallSpeed, -velocity.y);
        }
    }

    private void HandleRecovery()
    {
        if (nPlantInRange >= 1)
            currentPower += powerRecovery * Time.deltaTime;

        timeSinceLastUseStamina += Time.deltaTime;
        if (timeSinceLastUseStamina >= staminaRecoveryDelay)
            currentStamina += staminaRecovery * Time.deltaTime;
    }

    public void HandleMovement()
    {
        float bonusJumpForce;

        // Movement
        move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (slideTimer > 0f)
        {
            move.z = 1f;
            isSliding = true;
            slideTimer -= Time.deltaTime;
        }
        else
        {
            if (isSliding)
            {
                isSlopeSliding = false;
                isCrouching = false;
                isSliding = false;
            }
        }

        if (wallJumpTimer > 0f)
        {
            // Force horizontal move direction
            move.x = wallJumpDirection;
            wallJumpTimer -= Time.deltaTime;
        }

        if (isSlopeSliding)
        {
            move.z = 0f;
            smoothMove = move;
        }

        if (!isWallRunning)
            move = Vector3.ClampMagnitude(move, 1);
        smoothMove = Vector3.Lerp(smoothMove, move, Time.deltaTime * 5f);

        if ((Input.GetKey(KeyCode.LeftShift) || isWallRunning) && !isCrouching)
            isRunning = true;
        else
            isRunning = false;

        if (isRunning)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        // Directional modifier
        if (move.x != 0 && move.z == 0)
            currentSpeed *= 0.75f;
        if (move.z == -1)
            currentSpeed *= 0.5f;

        if (slideTimer > 0f)
            smoothSpeed = Mathf.Lerp(smoothSpeed, currentSpeed, 0.005f);
        else
            smoothSpeed = Mathf.Lerp(smoothSpeed, currentSpeed, 0.1f);

        finalMove = (smoothMove.x * transform.right + smoothMove.z * transform.forward) * smoothSpeed;
        controller.Move(finalMove * Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && !slopeVerySteep)
        {
            if (reliableIsGrouned || airTimer <= coyoteTime || isWallRunning)
            {
                justJump = true;

                // Bonus jump height while wallrunning
                if (isWallRunning)
                    bonusJumpForce = jumpHeight / 5;
                else
                    bonusJumpForce = 0f;

                // Bonus jump length from slide momentum
                if (isCrouching && slopeCanLongSlide)
                {
                    velocity.x = slopeNormal.x * sprintSpeed * jumpHeight / 2;
                    velocity.z = slopeNormal.z * sprintSpeed * jumpHeight / 2;
                }
                velocity.y = Mathf.Sqrt((jumpHeight + bonusJumpForce) * -2f * gravity);
                isCrouching = false;

                // Extra left/right velocity if jump while wall running
                if (isWallRunning)
                {
                    wallJumpTimer = wallJumpLockControlTime;
                    if (isWallLeft)
                        wallJumpDirection = 1f; // jump to the right
                    else
                        wallJumpDirection = -1f;
                }
            }
        }
    }

    private void UpdateGUI()
    {
        // Also prevent currentPower from going beyond maxPower
        currentPower = Mathf.Clamp(currentPower, 0, maxPower);
        powerSlider.value = currentPower / maxPower;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina / maxStamina;

        shootForceSlider.value = currentShootForce / maxShootForce;

        deliveredText.text = "Delivered: " + playerStats.deliveredAmount + "/" + requiredDeliveryAmount;
        foodBagLeftText.text = "Food bags left: " + currentFoodBag;
    }

    #endregion MainFunctions

    #region SubFuctions

    private void SunrayDash()
    {
        controller.Move(dashDirection * sunraySpeed * Time.deltaTime);

        // Check to see if in front of player is mirror
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dashDirection,
                out hit, rayDetectionDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Mirror")
            {
                AudioManager.instance.PlayDash();
                Vector3 reflectDirection = Vector3.Reflect(dashDirection, hit.normal);
                transform.position = hit.point;
                dashDirection = reflectDirection;
                timer = Time.time;
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Glass"))
            {
                // Do nothing
            }
            else if (hit.collider.gameObject.tag == "SolarPanel")
            {
                currentPower = 0f;
                Debug.Log("You lost all power!");
            }
            else
            {
                HumanForm();
            }
        }
    }

    private void SunrayForm()
    {
        wallRunCameraTilt = 0f;
        dashDirection = mainCamera.transform.forward;
        inSunrayForm = true;
        sunrayModel.SetActive(true);
        characterModel.SetActive(false);
        controller.height = 0f;
        controller.radius = 0.2f;
        Physics.IgnoreLayerCollision(glassMask, this.gameObject.layer, true);
    }

    private void HumanForm()
    {
        if (resetCameraAfterDash)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothResetCameraAfterDash());
        }
        velocity = Vector3.zero;
        inSunrayForm = false;
        sunrayModel.SetActive(false);
        characterModel.SetActive(true);
        controller.height = 2f;
        controller.radius = 0.5f;
        Physics.IgnoreLayerCollision(glassMask, this.gameObject.layer, false);
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
        Vector3 startingPosition = position;
        Ray ray = new Ray(position, direction);
        RaycastHit hit;
        bool continueBounce = false;

        if (Physics.Raycast(ray, out hit, sunraySpeed * timeInSunrayForm, ~glassMask, QueryTriggerInteraction.Ignore))
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
            position += direction * sunraySpeed * timeInSunrayForm;
        }
        drawPoints.Add(position);

        if (continueBounce)
        {
            MirrorRaycast(position, direction, bouncedMirror, drawPoints);
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireSphere(groundCheck.position, 0.4f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.right);
    }

    public void GoBackToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    #endregion SubFuctions

    #region Coroutine

    private IEnumerator SmoothResetCameraAfterDash()
    {
        float resetDuration = 0.2f;
        float timer = 0f;
        while (timer < resetDuration)
        {
            timer += Time.deltaTime;
            xRotation = Mathf.Lerp(xRotation, 0f, 0.08f);
            yield return null;
        }
        yield return null;
    }

    #endregion Coroutine
}