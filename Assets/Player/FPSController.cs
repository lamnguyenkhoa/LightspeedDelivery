using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Camera mainCamera;

    public float moveSpeed = 7f;
    public float jumpHeight = 4f;
    private float gravity = -9.8f;
    public LayerMask groundMask;
    public LayerMask glassMask;
    private float xRotation;
    private Vector3 move; // player controlled movement
    private Vector3 velocity; // environmental stuff affect player movement
    private CharacterController controller;
    public bool isGrounded = false;
    public Transform groundCheck;
    private bool inSunrayForm = false;
    public float sunraySpeed = 20f;
    public GameObject characterModel;
    public GameObject sunrayModel;
    public GameObject sunTrailPrefab;

    public LineRenderer aimRenderer;

    public float timeInSunrayForm = 0.5f;
    public float timer;

    private bool isAiming = false;

    public float maxFOV = 90f;
    private float startFOV;

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
        startFOV = mainCamera.fieldOfView;
    }

    private void Update()
    {
        if (inSunrayForm)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, maxFOV, 0.05f);
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
            mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            // Movement
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            move = (x * transform.right + z * transform.forward) * moveSpeed;
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
                SunrayForm();
                aimRenderer.positionCount = 0;
                isAiming = false;
                timer = Time.time;
            }

            // Gravity
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }
    }

    private void RenderAiming()
    {
        if (isAiming)
        {
            List<Vector3> drawPoints = new List<Vector3>();
            drawPoints.Add(transform.position);
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

        if (Physics.Raycast(ray, out hit, sunraySpeed))
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
            position += direction * sunraySpeed;
        }
        drawPoints.Add(position);

        if (continueBounce)
        {
            MirrorRaycast(position, direction, bouncedMirror, drawPoints);
        }
    }

    private void SunrayDash()
    {
        SunrayForm();
        //GameObject sunTrail = Instantiate(sunTrailPrefab, this.transform, true);
        //sunTrail.transform.localPosition = Vector3.zero;
        controller.Move(mainCamera.transform.forward * sunraySpeed * Time.deltaTime);

        // Check to see if in front of player is mirror
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward,
                out hit, 1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Mirror")
            {
                Vector3 inDirection = transform.forward;
                Vector3 reflectDirection = Vector3.Reflect(inDirection, hit.normal);
                transform.position = hit.point;
                transform.forward = reflectDirection;
                timer = Time.time;
                Debug.Log("Reflection direction: " + reflectDirection);
            }
        }

        //sunTrail.transform.parent = null;
    }

    private void SunrayForm()
    {
        inSunrayForm = true;
        sunrayModel.SetActive(true);
        characterModel.SetActive(false);
        controller.height = 0f;
        controller.radius = 0.2f;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Glass"), this.gameObject.layer, true);
    }

    private void HumanForm()
    {
        inSunrayForm = false;
        sunrayModel.SetActive(false);
        characterModel.SetActive(true);
        controller.height = 2f;
        controller.radius = 0.5f;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Glass"), this.gameObject.layer, false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward);
    }
}