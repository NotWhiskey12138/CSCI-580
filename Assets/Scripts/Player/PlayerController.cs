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
        controller.minMoveDistance = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yRotation = transform.eulerAngles.y;
        xRotation = cameraTransform.localEulerAngles.x;

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

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void HandleMovement()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A))
            x -= 1f;
        if (Input.GetKey(KeyCode.D))
            x += 1f;
        if (Input.GetKey(KeyCode.S))
            z -= 1f;
        if (Input.GetKey(KeyCode.W))
            z += 1f;

        Vector3 move = cameraTransform.forward * z + cameraTransform.right * x;

        if (move.magnitude > 1f)
            move.Normalize();

        if (Input.GetKey(KeyCode.Space))
            move += Vector3.up;

        if (Input.GetKey(KeyCode.LeftControl))
            move += Vector3.down;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift)
            ? speed * sprintMultiplier
            : speed;

        controller.Move(move * currentSpeed * Time.unscaledDeltaTime);
    }

    // Called when CharacterController hits a collider
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"Hit {hit.gameObject.name} at {hit.point}");

        // Example: detect ground
        if (hit.normal.y > 0.5f)
        {
            Debug.Log("Standing on surface");
        }

        // Example: detect wall
        if (Mathf.Abs(hit.normal.y) < 0.3f)
        {
            Debug.Log("Hit wall");
        }
    }
}
