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
    public Vector3 move; // player controlled movement
    public Vector3 velocity; // environmental stuff affect player movement
    private CharacterController controller;
    public bool isGrounded = false;
    public Transform groundCheck;

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        velocity = Vector3.zero;
    }

    private void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }
}