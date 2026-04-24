using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minZoom = 5f;   // Closest zoom (Isometric)
    public float maxZoom = 20f;  // Furthest zoom (Top-Down)
    public float zoomSpeed = 2f;
    
    [Header("Angles")]
    public float topDownAngle = 90f; // Looking straight down
    public float isometricAngle = 45f; // Angled view

    [Header("Pan Settings")]
    public float panSpeed = 10f;
    public Vector2 panLimit = new Vector2(20, 20);

    private float currentZoom;

    private void Start()
    {
        currentZoom = maxZoom; // Start fully zoomed out
        
        // Force the camera to center over the map on start
        Vector3 pos = transform.position;
        pos.x = 0;
        pos.z = 0;
        transform.position = pos;
        
        UpdateCameraTransform();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            UpdateCameraTransform();
        }
    }

    private void HandlePan()
    {
        Vector3 pos = transform.position;

        // WASD or Arrow Keys for panning
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }

    private void UpdateCameraTransform()
    {
        // 1. Calculate Y position based on zoom
        Vector3 newPos = transform.position;
        newPos.y = currentZoom;
        transform.position = newPos;

        // 2. Interpolate the X rotation based on the zoom level
        // Normalized zoom: 0 is minZoom (isometric), 1 is maxZoom (top-down)
        float normalizedZoom = (currentZoom - minZoom) / (maxZoom - minZoom);
        
        float targetAngleX = Mathf.Lerp(isometricAngle, topDownAngle, normalizedZoom);
        
        // Apply rotation (we only rotate on the X axis to tilt up/down)
        transform.rotation = Quaternion.Euler(targetAngleX, 0, 0);
    }
}
