using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minZoom  = 5f;
    public float maxZoom  = 22f;
    public float zoomSpeed = 3f;

    [Header("Angles (degrees from horizontal)")]
    public float topDownAngle   = 85f;
    public float isometricAngle = 45f;

    [Header("Pan Settings")]
    public float panSpeed   = 15f;
    public Vector2 panLimit = new Vector2(30f, 30f);

    private Vector3 _targetPosition = Vector3.zero;
    private float   _currentZoom;

    private void Start()
    {
        _currentZoom = maxZoom;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane  = 300f;
        }

        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady += CenterOnGrid;

        CenterOnGrid();
    }

    private void OnDestroy()
    {
        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady -= CenterOnGrid;
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
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

        // Camera sits behind and above the target so it always looks at _targetPosition.
        // horizontalDist = zoom / tan(angle): at 85° this is near-zero (almost top-down),
        // at 45° this equals zoom (perfect diagonal).
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
}
