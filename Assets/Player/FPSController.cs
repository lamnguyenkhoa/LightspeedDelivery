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
    public float jumpHeight = 4f;
    public float gravity = -9.8f;
    public LayerMask groundMask;
    public LayerMask glassMask;
    private float xRotation;
    private Vector3 move; // player controlled movement
    private Vector3 velocity; // environmental stuff affect player movement
    private Vector3 dashDirection;
    private CharacterController controller;
    public bool isGrounded = false;
    public Transform groundCheck;
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

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
        startFOV = mainCamera.fieldOfView;
        currentStamina = maxStamina;
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
            mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, Quaternion.Euler(xRotation, 0f, 0f), 0.2f);
            transform.Rotate(Vector3.up * mouseX);

            // Movement
            float xInput = Input.GetAxisRaw("Horizontal");
            float zInput = Input.GetAxisRaw("Vertical");

            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }

            if (isRunning)
            {
                move = (xInput * transform.right + zInput * transform.forward) * sprintSpeed;
            }
            else
            {
                move = (xInput * transform.right + zInput * transform.forward) * moveSpeed;
            }
            controller.Move(move * Time.deltaTime);

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Aim Sunray
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                isAiming = true;
            }

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

            // Gravity
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

            // Stamina management
            if (isRunning && move != Vector3.zero)
            {
                currentStamina -= runStaminaCost * Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRecovery * Time.deltaTime;
            }
        }

        UpdateGUI();
    }

    private void UpdateGUI()
    {
        // Also prevent currentStamina to overload
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        staminaSlider.value = currentStamina / maxStamina;
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
            xRotation = 0f;
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
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward);
    }
}