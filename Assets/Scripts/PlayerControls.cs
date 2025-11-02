using UnityEngine;

public class PlayerControls : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalRotation = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (characterController == null)
        {
            Debug.LogError("CharacterController not found on " + gameObject.name);
        }

        if (playerCamera == null)
        {
            Debug.LogError("Camera not found as a child of " + gameObject.name);

            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Debug.Log("Using Camera.main instead: " + playerCamera.name);
            }
        }
        else
        {
            Debug.Log("Camera found: " + playerCamera.name);
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // Prevent over-rotation

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;
        characterController.SimpleMove(moveDirection * moveSpeed);
    }
}