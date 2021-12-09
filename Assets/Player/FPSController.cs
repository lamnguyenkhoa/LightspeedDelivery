using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    [Space, Header("Camera")]
    public float mouseSensitivity = 100f;
    public Camera mainCamera;

    // Headbob
    private float amplitude;
    private float frequency;
    private Vector3 startCameraPos;
    private bool isLanding = false;
    private bool justFall = false;
    private float maxAchievedFallSpeed = 0f;

    [Space, Header("Movement")]
    public float moveSpeed = 7f;
    public float sprintSpeed = 12f;
    public float currentSpeed;
    public float smoothSpeed;
    public float jumpHeight = 4f;
    public float gravity = -9.8f;
    private float maxGravity = -100f;
    private Vector3 move; // player controlled movement
    private Vector3 smoothMove;
    private Vector3 finalMove;
    public LayerMask glassMask;
    private float xRotation;
    private bool isRunning;
    private Vector3 velocity; // environmental stuff affect player movement
    private Vector3 dashDirection;
    private CharacterController controller;
    private bool reliableIsGrouned; // because controller.isGrounded is damn unreliable
    private float coyoteTime = 0.2f;
    private float airTimer = 0f;

    [Space, Header("Sunray dash")]
    public float sunraySpeed = 20f;
    private bool inSunrayForm = false;
    public float maxPower = 100f;
    private float currentPower;
    public Slider powerSlider;
    public float dashCost = 25f;
    private float powerRecovery = 50f;
    public float timeInSunrayForm = 0.5f;
    private float timer;
    private bool isAiming = false;
    public bool resetCameraAfterDash = false;
    public float maxFOV = 90f;
    private float startFOV;
    public GameObject characterModel;
    public GameObject sunrayModel;
    public GameObject sunTrailPrefab;
    public LineRenderer aimRenderer;

    // Use int instead of bool to prevent edge case bug
    [HideInInspector] public int nPlantInRange = 0;

    [Space, Header("Food gun")]
    public GameObject foodGun;
    public FoodBagScript foodBagPrefab;
    private float currentShootForce = 0f;
    public float maxShootForce = 60f;
    public float shootForceIncreaseRate = 20f;
    public float minShootForce = 5f;
    public int currentFoodBag; // max number must be equal or higher than requiredDeliveryAmount
    public Slider shootForceSlider;
    private bool isGunCharging = false;

    [Space, Header("GUI")]
    public TextMeshProUGUI deliveredText;
    public TextMeshProUGUI foodBagLeftText;
    public int requiredDeliveryAmount = 3; // set at start of misison
    public int deliveredAmount;

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
    private float wallRunCameraTilt;
    public float maxWallRunCameraTilt;
    public float wallJumpLockControlTime = 1f; // disable player control in this amount of time
    private float wallJumpTimer;
    private float wallJumpDirection; // which side the player will jump toward, either -1 or 1

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
        startFOV = mainCamera.fieldOfView;
        currentPower = 0f;
        startCameraPos = mainCamera.transform.localPosition;
        nPlantInRange = 0;
        currentFoodBag = requiredDeliveryAmount;
    }

    private void Update()
    {
        if (inSunrayForm)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, maxFOV, 0.05f);

            //mainCamera.transform.forward = Vector3.Lerp(mainCamera.transform.forward, dashDirection, 0.05f);

            Vector3 dashEulerRotation = Quaternion.LookRotation(dashDirection).eulerAngles;
            // Update xRotation so when player go back to Human Form it doesn't reset the camera
            // rotation
            xRotation = dashEulerRotation.x;
            if (xRotation > 90)
            {
                xRotation = xRotation - 360f;
            }

            // Smooth rotate these 2 value so player dont feel nauseous
            Quaternion bodyRotation = Quaternion.Euler(new Vector3(0, dashEulerRotation.y, 0));
            Quaternion cameraRotation = Quaternion.Euler(new Vector3(xRotation, 0, 0));

            transform.rotation = Quaternion.Lerp(transform.rotation, bodyRotation, 0.1f);
            mainCamera.transform.localRotation = Quaternion.Lerp(mainCamera.transform.localRotation, cameraRotation, 0.1f);

            SunrayDash();

            if (Time.time > timer + timeInSunrayForm)
            {
                HumanForm();
            }
        }
        else
        {
            HandleCamera();

            HandleMovement();

            if (enableWallRun)
                HandleWallRun();

            HandleGravity();

            HandleLanding();

            HandleHeadBob();

            HandleSpecialAbilities();

            HandleShootGun();

            UpdateGUI();

            HandleRecovery();
        }
    }

    private void HandleWallRun()
    {
        // Tilt camera back
        if (wallRunCameraTilt > 0 && !isWallRunning)
            wallRunCameraTilt -= maxWallRunCameraTilt * 2 * Time.deltaTime;
        if (wallRunCameraTilt < 0 && !isWallRunning)
            wallRunCameraTilt += maxWallRunCameraTilt * 2 * Time.deltaTime;

        // No wallrun if you on ground bruh
        // Also no wallrun if no power
        if (currentStamina <= 0 || reliableIsGrouned)
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
        if ((Input.GetKey(KeyCode.D) && isWallRight) ||
            (Input.GetKey(KeyCode.A) && isWallLeft))
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
                FoodBagScript newFoodBag = Instantiate(foodBagPrefab, foodGun.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
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

    private void HandleLanding()
    {
        if (justFall && reliableIsGrouned)
        {
            justFall = false;
            airTimer = 0f;
            if (maxAchievedFallSpeed > 8f)
            {
                StartCoroutine(CameraJumpLanding(maxAchievedFallSpeed));
            }
            maxAchievedFallSpeed = 0f;
        }
    }

    private void HandleCamera()
    {
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, startFOV, 0.05f);

        // Camera
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, wallRunCameraTilt);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleGravity()
    {
        // Gravity
        // For some reason, the built-in controller.isGrounded only work if
        // move the character controller downward first
        //isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, maxGravity, float.MaxValue);
        controller.Move(velocity * Time.deltaTime);
        reliableIsGrouned = controller.isGrounded;
        if (reliableIsGrouned)
        {
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
            justFall = true;
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

    private void HandleMovement()
    {
        // Movement
        move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (wallJumpTimer > 0f)
        {
            // Force horizontal move direction
            move.x = wallJumpDirection;
            wallJumpTimer -= Time.deltaTime;
        }

        if (!isWallRunning)
            move = Vector3.ClampMagnitude(move, 1);
        smoothMove = Vector3.Lerp(smoothMove, move, Time.deltaTime * 5f);

        if (Input.GetKey(KeyCode.LeftShift) || isWallRunning)
            isRunning = true;
        else
            isRunning = false;

        if (isRunning)
        {
            currentSpeed = sprintSpeed;
            frequency = 20f;
            amplitude = 1.8f;
        }
        else
        {
            currentSpeed = moveSpeed;
            amplitude = 1.2f;
            frequency = 15f;
        }

        // Directional modifier
        if (move.x != 0 && move.z == 0)
            currentSpeed *= 0.75f;
        if (move.z == -1)
            currentSpeed *= 0.5f;

        smoothSpeed = Mathf.Lerp(smoothSpeed, currentSpeed, Time.deltaTime * 5f);
        finalMove = (smoothMove.x * transform.right + smoothMove.z * transform.forward) * smoothSpeed;
        controller.Move(finalMove * Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (reliableIsGrouned || airTimer <= coyoteTime || isWallRunning)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

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

    private void HandleHeadBob()
    {
        if (isLanding) return;

        if (finalMove.magnitude < 8f || (!reliableIsGrouned && !isWallRunning))
        {
            // Reset position
            if (mainCamera.transform.localPosition == startCameraPos) return;
            mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, startCameraPos, 5 * Time.deltaTime);
        }
        else
        {
            Vector3 headbobPos = Vector3.zero;
            headbobPos.y += Mathf.Sin(Time.time * frequency) * amplitude;
            headbobPos.x += Mathf.Sin(Time.time * frequency / 2) * amplitude;

            mainCamera.transform.localPosition += headbobPos * Time.deltaTime;
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

        deliveredText.text = "Delivered: " + deliveredAmount + "/" + requiredDeliveryAmount;
        foodBagLeftText.text = "Food bags left: " + currentFoodBag;
    }

    private void SunrayDash()
    {
        //GameObject sunTrail = Instantiate(sunTrailPrefab, this.transform, true);
        //sunTrail.transform.localPosition = Vector3.zero;
        controller.Move(dashDirection * sunraySpeed * Time.deltaTime);

        // Check to see if in front of player is mirror
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dashDirection,
                out hit, 0.5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Mirror")
            {
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

        //sunTrail.transform.parent = null;
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
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Glass"), this.gameObject.layer, true);
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
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Glass"), this.gameObject.layer, false);
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

        if (Physics.Raycast(ray, out hit, sunraySpeed * timeInSunrayForm))
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

    private IEnumerator CameraJumpLanding(float fallSpeed)
    {
        isLanding = true;
        float downYPos = 0.2f;
        if (fallSpeed >= 16f)
            downYPos = -0.5f;

        float upDuration = 0.3f;
        float downDuration = 0.15f;

        float delta = (downYPos - startCameraPos.y) / downDuration;
        float timer = 0f;
        while (timer < downDuration)
        {
            timer += Time.deltaTime;
            mainCamera.transform.localPosition += new Vector3(0, delta, 0) * Time.deltaTime;
            yield return null;
        }

        if (fallSpeed >= 16f)
            yield return new WaitForSeconds(0.1f);

        delta = (startCameraPos.y - downYPos) / upDuration;
        timer = 0f;
        while (timer < upDuration)
        {
            timer += Time.deltaTime;
            mainCamera.transform.localPosition += new Vector3(0, delta, 0) * Time.deltaTime;
            yield return null;
        }

        isLanding = false;
        yield return null;
    }
}