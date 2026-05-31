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
    
    [SerializeField] private float autoOrbitSpeed = 5f;
    [SerializeField] private float idleDelay = 3f;
    private float lastInputTime;
    [SerializeField] private float orbitSpeed = 5f;
    [SerializeField] private float orbitDistance = 35f;
    [SerializeField] private float orbitHeight = 18f;
    private float orbitAngle;
    
    private float currentDistance = 50f;
    private float yaw;
    private float pitch;


    [Header("Environment Reference")]
    [SerializeField] private BattleEnvironment env;
    [SerializeField] private Transform playerMothership;
    [SerializeField] private Transform enemyMothership;



    private void Start()
    {
        //Vector3 euler = transform.eulerAngles;
        //yaw = euler.y;
        //pitch = euler.x;

        GameManager gm = FindObjectOfType<GameManager>();
        int maxSize = Mathf.Max(gm.GetSizeX(), gm.GetSizeY(), gm.GetSizeZ());
        orbitDistance = gm.GetSpacing() *  maxSize * 1.5f;
        orbitHeight = orbitDistance * 0.5f;

        Vector3 center = GetBattleCenter();
        Vector3 playerDir = (playerMothership.position - center).normalized;
        orbitAngle = Mathf.Atan2(playerDir.z, playerDir.x) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        bool hasInput = 
            Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f;

        if (hasInput)
        {
            lastInputTime = Time.time;
            HandleMouseLook();
            HandleMovement();
            UpdateFocusPoint();
            HandleZoom();
        } else {
            AutoOrbit();
        }
    }

    private Vector3 GetBattleCenter()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        return gm.GetGridWorldCenter();
        
    }


    private void AutoOrbit()
    {
        Vector3 center = GetBattleCenter();

        orbitAngle += orbitSpeed * Time.deltaTime;

        float radians = orbitAngle * Mathf.Deg2Rad;

        // Slight vertical bob
        float height = orbitHeight + Mathf.Sin(Time.time * 0.5f) * 20f;

        Vector3 offset = new Vector3(
                Mathf.Cos(radians) * orbitDistance,
                height,
                Mathf.Sin(radians) * orbitDistance
                );

        transform.position = center + offset;


        transform.LookAt(center);
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
