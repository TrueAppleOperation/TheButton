using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float drag = 20f;
    public float mouseSensitivity = 2f;

    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private Vector3 currentVelocity;

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
        currentVelocity = Vector3.zero;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 worldMovement = transform.TransformDirection(movement);
        Vector3 desiredVelocity = worldMovement * moveSpeed;

        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, moveSpeed * Time.deltaTime);
 
        if (movement.magnitude < 0.1f)
        {
            currentVelocity -= currentVelocity * drag * Time.deltaTime; 
            if (currentVelocity.magnitude < 0.1f)
            {
                currentVelocity = Vector3.zero;
            }
        }

        transform.Translate(currentVelocity * Time.deltaTime, Space.World);
    }
}