using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Camera mainCamera;

    public float moveSpeed = 7f;
    public float sprintSpeed = 12f;
    public float currentSpeed;
    public float smoothSpeed;
    public float jumpHeight = 4f;
    public float gravity = -9.8f;
    private float maxGravity = -100f;

    //public LayerMask groundMask;
    public LayerMask glassMask;
    private float xRotation;
    public Vector3 move; // player controlled movement
    public Vector3 smoothMove;
    public Vector3 finalMove;

    public Vector3 velocity; // environmental stuff affect player movement
    private Vector3 dashDirection;
    private CharacterController controller;

    //public bool isGrounded = false;
    //public Transform groundCheck;
    private bool inSunrayForm = false;
    public float sunraySpeed = 20f;
    public GameObject characterModel;
    public GameObject sunrayModel;
    public GameObject sunTrailPrefab;

    public LineRenderer aimRenderer;

    public bool resetCameraAfterDash = false;

    public float timeInSunrayForm = 0.5f;
    private float timer;

    public float maxStamina = 100f;
    private float currentStamina;
    public Slider staminaSlider;
    private bool isRunning;
    public float runStaminaCost = 5f;
    public float dashStaminaCost = 30f;
    public float staminaRecovery = 10f;

    private bool isAiming = false;

    public float maxFOV = 90f;
    private float startFOV;

    public GameObject foodGun;
    public FoodBagScript foodBagPrefab;
    private float currentShootForce = 0f;
    public float maxShootForce = 60f;
    public float shootForceIncreaseRate = 20f;
    public float minShootForce = 5f;
    public int currentFoodBag = 3;
    public Slider shootForceSlider;

    private float amplitude = 0.015f;
    private float frequency = 10f;
    private Vector3 startCameraPos;

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
        startFOV = mainCamera.fieldOfView;
        currentStamina = maxStamina;
        startCameraPos = mainCamera.transform.localPosition;
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
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, startFOV, 0.05f);

            // Camera
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            transform.Rotate(Vector3.up * mouseX);

            HandleMovement();
            // Gravity
            // For some reason, the built-in controller.isGrounded only work if
            // move the character controller downward first
            //isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, maxGravity, -2f);
            controller.Move(velocity * Time.deltaTime);
            if (controller.isGrounded)
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
            }

            HeadBob();

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

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
                if (currentStamina >= dashStaminaCost)
                {
                    currentStamina -= dashStaminaCost;
                    SunrayForm();
                    aimRenderer.positionCount = 0;
                    isAiming = false;
                    timer = Time.time;
                }
                else
                {
                    isAiming = false;
                    aimRenderer.positionCount = 0;
                    Debug.Log("Not enough stamina");
                }
            }

            // Shoot food bag
            // Hold LMB to charge
            if (Input.GetKey(KeyCode.Mouse0))
            {
                currentShootForce += shootForceIncreaseRate * Time.deltaTime;
                currentShootForce = Mathf.Clamp(currentShootForce, 0, maxShootForce);
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (currentShootForce >= minShootForce)
                {
                    FoodBagScript newFoodBag = Instantiate(foodBagPrefab, foodGun.transform.position, Quaternion.identity);
                    newFoodBag.shootDirection = mainCamera.transform.forward;
                    newFoodBag.shootForce = currentShootForce;
                    // not all momentum yet, only player "active" momentum
                    newFoodBag.bonusFromPlayerMomentum = finalMove;
                    Debug.Log("Shoot with " + currentShootForce + " force");
                    currentFoodBag--;
                }
                currentShootForce = 0f;
            }

            // Stamina management
            if (isRunning && move != Vector3.zero)
                currentStamina -= runStaminaCost * Time.deltaTime;
            else
                currentStamina += staminaRecovery * Time.deltaTime;
        }

        UpdateGUI();
    }

    private void HandleMovement()
    {
        // Movement
        move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        smoothMove = Vector3.Lerp(smoothMove, move, Time.deltaTime * 5f);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            currentSpeed = sprintSpeed;
            frequency = 20f;
            amplitude = 2f;
        }
        else
        {
            isRunning = false;
            currentSpeed = moveSpeed;
            amplitude = 1f;
            frequency = 10f;
        }

        // Directional modifier
        if (move.x != 0 && move.z == 0)
            currentSpeed *= 0.75f;
        if (move.z == -1)
            currentSpeed *= 0.5f;

        smoothSpeed = Mathf.Lerp(smoothSpeed, currentSpeed, Time.deltaTime * 5f);
        finalMove = (smoothMove.x * transform.right + smoothMove.z * transform.forward) * smoothSpeed;
        controller.Move(finalMove * Time.deltaTime);
    }

    private void HeadBob()
    {
        if (finalMove.magnitude < 8f || !controller.isGrounded)
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
        // Also prevent currentStamina from going beyond maxStamina
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina / maxStamina;
        shootForceSlider.value = currentShootForce / maxShootForce;
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
                Debug.Log("You died!");
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
}