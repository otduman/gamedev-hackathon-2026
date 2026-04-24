using System.Collections;
using UnityEngine;
using TMPro;

public enum TerrainType { Plain, Mountain, Water, Pasture }

public class Tile : MonoBehaviour
{
    public Vector2Int HexCoordinates { get; private set; }
    public TerrainType Terrain       { get; private set; }
    public int OwnerPlayerId         { get; set; } = -1;
    public int CubeCount             { get; set; } = 0;
    public bool HasYurt              { get; private set; }

    [Header("Asset References")]
    [SerializeField] private GameObject  yurtPrefab;
    [SerializeField] private Renderer    meshRenderer;
    [SerializeField] private TextMeshPro cubeCountText;

    private GameObject            _activeYurtInstance;
    private MaterialPropertyBlock _propBlock;
    private bool                  _isSelected = false;
    private Color                 _baseColor  = Color.white;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");

    private static readonly Color _neutralTint  = Color.white;
    private static readonly Color _player0Tint  = new Color(0.35f, 0.45f, 1.00f, 1f);
    private static readonly Color _player1Tint  = new Color(1.00f, 0.35f, 0.35f, 1f);
    private static readonly Color _selectedTint = new Color(1.00f, 0.90f, 0.10f, 1f);

    // How large the yurt should appear in world units (diameter).
    // At hexSize=1.5 the tile face is ~1.4 units wide; 0.65 fills ~45% — readable but not cramped.
    private const float YurtTargetSize = 0.65f;

    public void Initialize(Vector2Int coords, TerrainType terrain)
    {
        HexCoordinates = coords;
        Terrain        = terrain;
        if (meshRenderer != null) _baseColor = ReadBaseColor(meshRenderer);
        UpdateVisuals();
    }

    public void SetPrimitiveMeshRenderer(Renderer r)
    {
        meshRenderer = r;
        if (meshRenderer != null) _baseColor = ReadBaseColor(meshRenderer);
    }

    public void AddCubes(int amount)    { CubeCount += amount;                       UpdateVisuals(); }
    public void RemoveCubes(int amount) { CubeCount = Mathf.Max(0, CubeCount - amount); UpdateVisuals(); }
    public void SetHasYurt(bool value)  { HasYurt = value;   UpdateVisuals(); }
    public void SetSelected(bool sel)   { _isSelected = sel; UpdateVisuals(); }

    public void UpdateVisuals()
    {
        ApplyTint();
        UpdateCubeCountText();
        UpdateYurtVisuals();
    }

    private void ApplyTint()
    {
        if (meshRenderer == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(_propBlock);

        Color tint = _isSelected        ? _selectedTint
                   : OwnerPlayerId == 0 ? _player0Tint
                   : OwnerPlayerId == 1 ? _player1Tint
                   : _neutralTint;

        _propBlock.SetColor(BaseColorId, _baseColor * tint);
        _propBlock.SetColor(ColorId,     _baseColor * tint);
        meshRenderer.SetPropertyBlock(_propBlock);
    }

    private void UpdateCubeCountText()
    {
        if (cubeCountText == null) return;
        if (CubeCount > 0) { cubeCountText.text = CubeCount.ToString(); cubeCountText.gameObject.SetActive(true); }
        else cubeCountText.gameObject.SetActive(false);
    }

    private void UpdateYurtVisuals()
    {
        if (HasYurt && _activeYurtInstance == null)
        {
            if (yurtPrefab != null)
            {
                _activeYurtInstance = Instantiate(yurtPrefab, transform);
                _activeYurtInstance.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                _activeYurtInstance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                // Auto-scale so the yurt fits the tile regardless of the source model's native size.
                StartCoroutine(AutoScaleYurt(_activeYurtInstance));
            }
            else
            {
                // No yurt model available — use a gold sphere as a visible placeholder.
                _activeYurtInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _activeYurtInstance.transform.SetParent(transform, false);
                _activeYurtInstance.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                _activeYurtInstance.transform.localScale    = new Vector3(0.3f, 0.3f, 0.3f);
                Renderer r = _activeYurtInstance.GetComponent<Renderer>();
                if (r != null) r.material.color = new Color(1f, 0.85f, 0.1f);
                Collider c = _activeYurtInstance.GetComponent<Collider>();
                if (c != null) Destroy(c);
            }
        }
        else if (!HasYurt && _activeYurtInstance != null)
        {
            Destroy(_activeYurtInstance);
            _activeYurtInstance = null;
        }
    }

    // Waits one frame after spawn so all child Renderers have initialised their bounds,
    // then uniformly scales the yurt to YurtTargetSize world units.
    private IEnumerator AutoScaleYurt(GameObject yurtGo)
    {
        yield return null;

        if (yurtGo == null) yield break;

        Renderer[] renderers = yurtGo.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) yield break;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);

        // Use the horizontal footprint (XZ) — we don't want to shrink a tall model just because it's tall.
        float maxHorizontal = Mathf.Max(bounds.size.x, bounds.size.z);
        if (maxHorizontal < 0.001f) yield break;

        float scale = YurtTargetSize / maxHorizontal;
        yurtGo.transform.localScale = Vector3.one * scale;
    }

    private static Color ReadBaseColor(Renderer r)
    {
        if (r.sharedMaterial == null) return Color.white;
        if (r.sharedMaterial.HasProperty(BaseColorId)) return r.sharedMaterial.GetColor(BaseColorId);
        if (r.sharedMaterial.HasProperty(ColorId))     return r.sharedMaterial.GetColor(ColorId);
        return Color.white;
    }
}
