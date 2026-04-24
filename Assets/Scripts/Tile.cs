using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject  warriorPrefab;
    [SerializeField] private Renderer    meshRenderer;
    [SerializeField] private TextMeshPro cubeCountText;

    private GameObject            _activeYurtInstance;
    private List<GameObject>      _activeWarriors  = new List<GameObject>();
    private int                   _spawnedWarriorCount = -1;

    private MaterialPropertyBlock _propBlock;
    private bool                  _isSelected = false;
    private Color                 _baseColor  = Color.white;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");

    private static readonly Color _neutralTint  = Color.white;
    private static readonly Color _player0Tint  = new Color(0.35f, 0.45f, 1.00f, 1f);
    private static readonly Color _player1Tint  = new Color(1.00f, 0.35f, 0.35f, 1f);
    private static readonly Color _selectedTint = new Color(1.00f, 0.90f, 0.10f, 1f);

    private const float YurtScale    = 0.15f;
    private const float WarriorScale = 0.35f;
    private const int   MaxShownWarriors = 5;

    // Formation positions (local XZ, Y=0) for 1-5 warriors on a KayKit hex tile.
    private static readonly Vector3[][] Formations = new Vector3[][]
    {
        // 1 — centre
        new[] { Vector3.zero },
        // 2 — side by side
        new[] { new Vector3(-0.28f, 0f,  0.00f), new Vector3( 0.28f, 0f,  0.00f) },
        // 3 — equilateral triangle
        new[] { new Vector3( 0.00f, 0f, -0.28f), new Vector3(-0.25f, 0f,  0.15f), new Vector3( 0.25f, 0f,  0.15f) },
        // 4 — square
        new[] { new Vector3(-0.24f, 0f, -0.24f), new Vector3( 0.24f, 0f, -0.24f),
                new Vector3(-0.24f, 0f,  0.24f), new Vector3( 0.24f, 0f,  0.24f) },
        // 5 — centre + cardinal cross
        new[] { new Vector3( 0.00f, 0f,  0.00f), new Vector3( 0.00f, 0f, -0.32f),
                new Vector3( 0.32f, 0f,  0.00f), new Vector3( 0.00f, 0f,  0.32f),
                new Vector3(-0.32f, 0f,  0.00f) },
    };

    // -------------------------------------------------------------------------
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

    public void AddCubes(int amount)    { CubeCount += amount;                           UpdateVisuals(); }
    public void RemoveCubes(int amount) { CubeCount = Mathf.Max(0, CubeCount - amount);  UpdateVisuals(); }
    public void SetHasYurt(bool value)  { HasYurt = value;   UpdateVisuals(); }
    public void SetSelected(bool sel)   { _isSelected = sel; UpdateVisuals(); }

    public void UpdateVisuals()
    {
        ApplyTint();
        UpdateWarriorVisuals();
        UpdateYurtVisuals();
    }

    // -------------------------------------------------------------------------
    // Tint
    // -------------------------------------------------------------------------
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

    // -------------------------------------------------------------------------
    // Warriors
    // -------------------------------------------------------------------------
    private void UpdateWarriorVisuals()
    {
        if (_spawnedWarriorCount == CubeCount) return;
        _spawnedWarriorCount = CubeCount;

        // Destroy existing warrior instances.
        foreach (GameObject w in _activeWarriors)
            if (w != null) Destroy(w);
        _activeWarriors.Clear();

        if (CubeCount <= 0)
        {
            HideCountLabel();
            return;
        }

        int toShow = Mathf.Min(CubeCount, MaxShownWarriors);
        Vector3[] positions = Formations[toShow - 1];

        for (int i = 0; i < toShow; i++)
        {
            GameObject warrior = SpawnWarrior(positions[i]);
            _activeWarriors.Add(warrior);
        }

        // Show count label for 6+ warriors.
        if (CubeCount > MaxShownWarriors)
            ShowCountLabel(CubeCount);
        else
            HideCountLabel();
    }

    private GameObject SpawnWarrior(Vector3 localPos)
    {
        GameObject warrior;
        if (warriorPrefab != null)
        {
            warrior = Instantiate(warriorPrefab, transform);
            warrior.transform.localPosition = localPos;
            warrior.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            warrior.transform.localScale    = Vector3.one * WarriorScale;

            // Remove colliders so warriors don't block tile click raycasts.
            foreach (Collider c in warrior.GetComponentsInChildren<Collider>(true))
                Destroy(c);

            PlayIdleAnimation(warrior);
        }
        else
        {
            // Fallback: coloured capsule.
            warrior = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            warrior.transform.SetParent(transform, false);
            warrior.transform.localPosition = localPos + new Vector3(0f, 0.25f, 0f);
            warrior.transform.localScale    = new Vector3(0.18f, 0.25f, 0.18f);
            Destroy(warrior.GetComponent<Collider>());
        }
        return warrior;
    }

    private static void PlayIdleAnimation(GameObject warrior)
    {
        // Try Mecanim Animator first.
        Animator anim = warrior.GetComponentInChildren<Animator>(true);
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.Play(0);
            return;
        }

        // Fallback: legacy Animation component.
        Animation legacyAnim = warrior.GetComponentInChildren<Animation>(true);
        if (legacyAnim != null && legacyAnim.clip != null)
            legacyAnim.Play();
    }

    private void ShowCountLabel(int count)
    {
        if (cubeCountText == null) return;
        cubeCountText.text = count.ToString();
        cubeCountText.gameObject.SetActive(true);
    }

    private void HideCountLabel()
    {
        if (cubeCountText != null)
            cubeCountText.gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Yurt
    // -------------------------------------------------------------------------
    private void UpdateYurtVisuals()
    {
        if (HasYurt && _activeYurtInstance == null)
        {
            if (yurtPrefab != null)
            {
                _activeYurtInstance = Instantiate(yurtPrefab, transform);
                _activeYurtInstance.transform.localPosition = Vector3.zero;
                _activeYurtInstance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                StartCoroutine(AutoScaleYurt(_activeYurtInstance));
            }
            else
            {
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

    private IEnumerator AutoScaleYurt(GameObject yurtGo)
    {
        yield return null;
        if (yurtGo == null) yield break;

        Transform searchRoot = yurtGo.transform.childCount > 0
            ? yurtGo.transform.GetChild(0)
            : yurtGo.transform;

        bool keptOne = false;
        for (int i = 0; i < searchRoot.childCount; i++)
        {
            Transform child = searchRoot.GetChild(i);
            if (child.GetComponentInChildren<Renderer>(true) != null)
            {
                if (!keptOne) keptOne = true;
                else child.gameObject.SetActive(false);
            }
        }

        yurtGo.transform.localScale = Vector3.one * YurtScale;
    }

    // -------------------------------------------------------------------------
    private static Color ReadBaseColor(Renderer r)
    {
        if (r.sharedMaterial == null) return Color.white;
        if (r.sharedMaterial.HasProperty(BaseColorId)) return r.sharedMaterial.GetColor(BaseColorId);
        if (r.sharedMaterial.HasProperty(ColorId))     return r.sharedMaterial.GetColor(ColorId);
        return Color.white;
    }
}
