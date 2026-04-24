using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float minZoom   = 5f;
    [SerializeField] private float maxZoom   = 14f;
    [SerializeField] private float zoomSpeed = 3f;

    [Header("Angles (degrees from horizontal)")]
    [SerializeField] private float topDownAngle   = 72f;
    [SerializeField] private float isometricAngle = 45f;

    [Header("Pan Settings")]
    [SerializeField] private float   panSpeed = 12f;
    [SerializeField] private Vector2 panLimit = new Vector2(25f, 25f);

    private Vector3 _targetPosition = Vector3.zero;
    private float   _currentZoom;
    private float   _lastFitAspect = -1f;

    private void Awake()
    {
        // Hard-clamp zoom and angle — prevents stale serialized values from breaking the camera.
        minZoom       = Mathf.Clamp(minZoom, 3f, 10f);
        maxZoom       = Mathf.Clamp(maxZoom, minZoom + 5f, 40f);
        topDownAngle   = 72f;   // civ-style tilt: 72° from horizontal
        isometricAngle = 45f;

        // Always start fully zoomed out so the whole board is visible.
        _currentZoom = maxZoom;
        Debug.Log($"[CameraController] Awake: zoom={_currentZoom:F1}  min={minZoom}  max={maxZoom}");
    }

    private void OnValidate()
    {
        minZoom = Mathf.Max(minZoom, 2f);
        if (maxZoom < minZoom + 5f) maxZoom = minZoom + 9f;
    }

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane  = 300f;
        }

        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady += FitToGrid;

        CenterOnGrid();
    }

    private void OnDestroy()
    {
        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady -= FitToGrid;
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();

        // Re-fit whenever the Game view aspect ratio changes (resolution switch).
        Camera cam = GetComponent<Camera>();
        if (cam != null &&
            HexGridManager.Instance != null &&
            HexGridManager.Instance.Grid.Count > 0 &&
            Mathf.Abs(cam.aspect - _lastFitAspect) > 0.005f)
        {
            FitToGrid();
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;
        _currentZoom -= scroll * zoomSpeed;
        _currentZoom  = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
        UpdateCameraTransform();
    }

    private void HandlePan()
    {
        Vector3 delta = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    delta += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  delta -= Vector3.forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) delta += Vector3.right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  delta -= Vector3.right;
        if (delta == Vector3.zero) return;

        Vector3 camForwardFlat = transform.forward; camForwardFlat.y = 0f; camForwardFlat.Normalize();
        Vector3 camRightFlat   = transform.right;   camRightFlat.y   = 0f; camRightFlat.Normalize();

        _targetPosition += (camForwardFlat * delta.z + camRightFlat * delta.x) * (panSpeed * Time.deltaTime);
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, -panLimit.x, panLimit.x);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, -panLimit.y, panLimit.y);
        _targetPosition.y = 0f;
        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        float normalizedZoom = Mathf.InverseLerp(minZoom, maxZoom, _currentZoom);
        float targetAngleDeg = Mathf.Lerp(isometricAngle, topDownAngle, normalizedZoom);
        float angleRad       = targetAngleDeg * Mathf.Deg2Rad;

        float tanAngle       = Mathf.Tan(Mathf.Max(angleRad, 0.01f));
        float horizontalDist = _currentZoom / tanAngle;

        transform.position = _targetPosition + new Vector3(0f, _currentZoom, -horizontalDist);
        transform.LookAt(_targetPosition);
    }

    public void CenterOnGrid()
    {
        _targetPosition = Vector3.zero;
        UpdateCameraTransform();
    }

    // Called once after the grid is generated. Computes the exact zoom needed to fit
    // every tile inside the viewport with no background showing.
    private void FitToGrid()
    {
        if (HexGridManager.Instance == null || HexGridManager.Instance.Grid.Count == 0) return;

        Camera cam = GetComponent<Camera>();
        if (cam == null) return;

        // Find the world-space bounding box of all tile centres.
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
        {
            Vector3 p = tile.transform.position;
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.z < minZ) minZ = p.z;
            if (p.z > maxZ) maxZ = p.z;
        }

        // Add half a tile width of padding so edge tiles aren't clipped.
        const float tilePadding = 1.1f;
        float halfW = (maxX - minX) * 0.5f + tilePadding;
        float halfH = (maxZ - minZ) * 0.5f + tilePadding;

        // Centre the look-at point on the grid (should be ~0,0,0 for a symmetric grid).
        _targetPosition = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);

        // At near-top-down angles the ground plane is ~perpendicular to the view ray.
        // Visible half-extents ≈ zoom * tan(fov/2) vertically, * aspect horizontally.
        float tanHalfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float zoomForH   = halfH / tanHalfFov;
        float zoomForW   = halfW / (tanHalfFov * cam.aspect);

        // maxZoom is expanded to whatever the grid needs; minZoom stays as the closest scroll limit.
        float needed = Mathf.Max(zoomForH, zoomForW);
        maxZoom      = needed * 1.2f;            // 20% breathing room — accounts for tilted view
        _currentZoom = maxZoom;

        _lastFitAspect = cam.aspect;
        UpdateCameraTransform();
        Debug.Log($"[CameraController] FitToGrid: zoom={_currentZoom:F2}  halfW={halfW:F2}  halfH={halfH:F2}  aspect={cam.aspect:F3}");
    }
}
