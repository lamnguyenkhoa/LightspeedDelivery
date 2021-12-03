using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Camera mainCamera;

    public float moveSpeed = 7f;
    public float jumpHeight = 4f;
    private float gravity = -9.8f;
    public LayerMask groundMask;
    private float xRotation;
    private Vector3 move; // player controlled movement
    private Vector3 velocity; // environmental stuff affect player movement
    private CharacterController controller;
    public bool isGrounded = false;
    public Transform groundCheck;
    private bool inSunrayForm = false;
    public float sunraySpeed = 20f;

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
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, maxFOV, 0.1f);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, startFOV, 0.1f);

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }

    private IEnumerator SunrayDash()
    {
        inSunrayForm = true;
        float duration = 0.5f;
        float timer = 0;
        while (timer < duration)
        {
            controller.Move(mainCamera.transform.forward * sunraySpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        inSunrayForm = false;
        yield return null;
    }
}