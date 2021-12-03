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

            // Sunray
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                StopAllCoroutines();
                StartCoroutine(SunrayDash());
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

    private void OnControllerColliderHit(ControllerColliderHit colliderHit)
    {
        if (colliderHit.gameObject.tag == "Mirror")
        {
            Debug.Log("Mirror");
            RaycastHit hit;
            if (Physics.Raycast(transform.position + transform.forward * 2, transform.forward,
                    out hit, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Vector3 inDirection = transform.forward;
                Vector3 reflectDirection = Vector3.Reflect(inDirection, hit.normal);
                transform.forward = reflectDirection.normalized;
                StopAllCoroutines();
                StartCoroutine(SunrayDash());
                Debug.Log("Reflection direction: " + reflectDirection);
            }
        }
        if (colliderHit.gameObject.tag == "SolarPanel")
        {
            Debug.Log("SolarPanel, died!");
        }
    }

    private IEnumerator SunrayDash()
    {
        inSunrayForm = true;
        sunrayModel.SetActive(true);
        characterModel.SetActive(false);
        controller.height = 0f;
        controller.radius = 0.2f;

        GameObject sunTrail = Instantiate(sunTrailPrefab, this.transform, true);
        sunTrail.transform.localPosition = Vector3.zero;
        float duration = 0.5f;
        float timer = 0;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Glass"), this.gameObject.layer, true);
        while (timer < duration)
        {
            controller.Move(mainCamera.transform.forward * sunraySpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Glass"), this.gameObject.layer, false);
        inSunrayForm = false;
        sunrayModel.SetActive(false);
        characterModel.SetActive(true);
        controller.height = 2f;
        controller.radius = 0.5f;
        sunTrail.transform.parent = null;
        yield return null;
    }

    /* https://www.youtube.com/watch?v=GttdLYKEJAM&ab_channel=WorldofZero */

    private void DrawReflectDirectionDebug(Vector3 position, Vector3 direction, List<GameObject> bouncedMirror)
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startingPosition, position);

        if (continueBounce)
        {
            DrawReflectDirectionDebug(position, direction, bouncedMirror);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        DrawReflectDirectionDebug(transform.position, transform.forward * 2, new List<GameObject>());
    }
}