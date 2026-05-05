using UnityEngine;

public class SpaceRTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float fastMultiplier = 3f;
    [SerializeField] private float verticalSpeed = 15f;

    [Header("Mouse Look")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private bool invertY = false;

    [Header("Zoom / Speed")]
    [SerializeField] private float zoomSpeed = 50f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 200f;

    private Vector3 focusPoint = Vector3.zero;

private float currentDistance = 50f;
    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        UpdateFocusPoint();
        HandleZoom();
    }

    private void HandleMouseLook()
    {
        if (!Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch += invertY ? mouseY : -mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= fastMultiplier;
        }

        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) input += transform.forward;
        if (Input.GetKey(KeyCode.S)) input -= transform.forward;
        if (Input.GetKey(KeyCode.A)) input -= transform.right;
        if (Input.GetKey(KeyCode.D)) input += transform.right;

        if (Input.GetKey(KeyCode.E)) input += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) input -= Vector3.up;

        transform.position += input.normalized * speed * Time.deltaTime;
    }

    void UpdateFocusPoint()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                focusPoint = hit.point;
            }
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float zoomAmount = scroll * zoomSpeed * Time.deltaTime;

        Vector3 dir = (transform.position - focusPoint).normalized;

        float distance = Vector3.Distance(transform.position, focusPoint);
        distance -= zoomAmount;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        transform.position = focusPoint + dir * distance;
    }
}
