using UnityEngine;

// Controls: Mouse look | WASD move | Space up | Left Shift sprint

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float sprintMultiplier = 2f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.5f;

    public Transform cameraTransform;

    private float xRotation = 0f; // pitch
    private float yRotation = 0f; // yaw

    private CharacterController controller;

    public Vector3 Position => transform.position;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // initialize rotation
        yRotation = transform.eulerAngles.y;
        xRotation = cameraTransform.localEulerAngles.x;

        // convert 0~360 to -180~180
        if (xRotation > 180f)
            xRotation -= 360f;
    }

    void Update()
    {
        if (!Application.isFocused)
            return;

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // pitch (camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // yaw (player)
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // movement based on camera direction (full 3D)
        Vector3 move = cameraTransform.forward * z + cameraTransform.right * x;

        // prevent faster diagonal movement
        if (move.magnitude > 1f)
            move.Normalize();

        // vertical movement
        if (Input.GetKey(KeyCode.Space))
            move += Vector3.up;
        
        if (Input.GetKey(KeyCode.LeftControl))
            move -= Vector3.down;

        // sprint
        float currentSpeed = Input.GetKey(KeyCode.LeftShift)
            ? speed * sprintMultiplier
            : speed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }
}