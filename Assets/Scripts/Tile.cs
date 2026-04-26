using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum TerrainType { Plain, Mountain, Water }

public class Tile : MonoBehaviour
{
    public Vector2Int HexCoordinates { get; private set; }
    public TerrainType Terrain       { get; private set; }
    public int  OwnerPlayerId        { get; set; } = -1;
    public int  CubeCount            { get; set; } = 0;
    public bool HasYurt              { get; private set; }
    public bool HasForest            { get; private set; }
    public string TerritoryName      { get; private set; } = "";
    public int  ContinentId          { get; private set; } = -1;

    [Header("Asset References")]
    [SerializeField] private GameObject  yurtPrefab;
    [SerializeField] private GameObject  warriorPrefab;
    [SerializeField] private Renderer    meshRenderer;
    [SerializeField] private TextMeshPro cubeCountText;

    private GameObject        _activeYurtInstance;
    private List<GameObject>  _activeWarriors      = new List<GameObject>();
    private int               _spawnedWarriorCount = -1;
    private bool              _spawnedHadYurt      = false;
    private List<Renderer>    _decoRenderers       = new List<Renderer>();

    private MaterialPropertyBlock _propBlock;
    private bool  _isSelected    = false;
    private bool  _isHighlighted = false;
    private bool  _isHovered     = false;
    private Color _highlightColor = Color.white;
    private Color _baseColor      = Color.white;
    private float _pulseIntensity = 0f;

    private static readonly Color _hoverTint    = new Color(0.95f, 0.95f, 0.80f, 1f);
    private static readonly Color _selectedTint = new Color(1.00f, 0.90f, 0.10f, 1f);

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");

    // Continent/region colours — applied to the TILE surface (Risk board style).
    // Six regions matching GameManager.RegionNames order.
    private static readonly Color[] RegionTints =
    {
        new Color(0.95f, 0.80f, 0.20f, 1f), // Uly Zhuz  — gold
        new Color(0.95f, 0.55f, 0.15f, 1f), // Zhetysu   — orange
        new Color(0.30f, 0.65f, 0.95f, 1f), // Orta Zhuz — sky blue
        new Color(0.35f, 0.80f, 0.40f, 1f), // Kishi Zhuz— green
        new Color(0.80f, 0.65f, 0.45f, 1f), // Dala      — tan/steppe
        new Color(0.85f, 0.40f, 0.55f, 1f), // Syr Darya — dusty rose
    };

    // Player colours used ONLY for warrior/yurt pieces and border lines — not tile surfaces.
    private static readonly Color[] PlayerTints =
    {
        new Color(0.20f, 0.40f, 1.00f, 1f), // Argyn  — blue
        new Color(1.00f, 0.28f, 0.28f, 1f), // Naiman — red
        new Color(0.15f, 0.80f, 0.30f, 1f), // Kerey  — green
        new Color(0.75f, 0.20f, 0.90f, 1f), // Kongrat— purple
    };

    private const float YurtScale      = 1.1f;
    private const float WarriorScale   = 0.38f;
    private const float WarriorYOffset = -0.15f;
    private const int   MaxWarriors    = 3;
    private const float MountainYLift  = 1.10f;

    // Tight triangle formation for up to 3 units
    private static readonly Vector3[][] Formations =
    {
        new[] { Vector3.zero },
        new[] { new Vector3(-0.20f, 0f,  0.00f), new Vector3( 0.20f, 0f,  0.00f) },
        new[] { new Vector3( 0.00f, 0f, -0.22f), new Vector3(-0.18f, 0f,  0.12f), new Vector3( 0.18f, 0f,  0.12f) },
    };

    // -------------------------------------------------------------------------
    // Initialisation
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

    // -------------------------------------------------------------------------
    // State setters
    // -------------------------------------------------------------------------

    public void AddCubes(int n)    { CubeCount += n;                              UpdateVisuals(); }
    public void RemoveCubes(int n) { CubeCount = Mathf.Max(0, CubeCount - n);     UpdateVisuals(); }
    public void SetHasYurt(bool v) { HasYurt = v;                                  UpdateVisuals(); }
    public void SetHasForest(bool v) { HasForest = v; }
    public void SetTerritoryInfo(string name, int continentId) { TerritoryName = name; ContinentId = continentId; UpdateVisuals(); }
    public void SetSelected(bool v)  { _isSelected = v;    UpdateVisuals(); }
    public void SetHovered(bool v)   { _isHovered  = v;    UpdateVisuals(); }

    public void SetHighlighted(bool highlighted, Color color)
    {
        _isHighlighted  = highlighted;
        _highlightColor = color;
        UpdateVisuals();
    }

    public void ClearHighlight() { _isHighlighted = false; UpdateVisuals(); }

    public void TriggerConquestPulse()
    {
        StopCoroutine("PulseCoroutine");
        StartCoroutine(PulseCoroutine());
    }

    private IEnumerator PulseCoroutine()
    {
        float t = 0f, dur = 0.55f;
        while (t < dur) { t += Time.deltaTime; _pulseIntensity = Mathf.Sin(t / dur * Mathf.PI) * 0.75f; ApplyTint(); yield return null; }
        _pulseIntensity = 0f;
        ApplyTint();
    }

    // -------------------------------------------------------------------------
    // Visuals
    // -------------------------------------------------------------------------

    public void UpdateVisuals()
    {
        ApplyTint();
        UpdateWarriorVisuals();
        UpdateYurtVisuals();
        UpdateDecoOpacity();
    }

    private void ApplyTint()
    {
        if (meshRenderer == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(_propBlock);

        // Base: tint the tile by its continent colour (Risk-style — the TILE shows the region,
        // the PIECES and BORDERS show the owner).
        Color regionBase = ContinentId >= 0 && ContinentId < RegionTints.Length
            ? Color.Lerp(_baseColor, RegionTints[ContinentId], 0.38f)
            : _baseColor;

        Color tint;
        if (_isSelected)
            tint = regionBase * _selectedTint;
        else if (_isHighlighted)
            tint = Color.Lerp(regionBase, _highlightColor, 0.82f);
        else if (_isHovered)
            tint = Color.Lerp(regionBase, _hoverTint, 0.45f);
        else
            tint = regionBase;

        if (_pulseIntensity > 0f)
            tint = Color.Lerp(tint, Color.white, _pulseIntensity);

        _propBlock.SetColor(BaseColorId, tint);
        _propBlock.SetColor(ColorId,     tint);
        meshRenderer.SetPropertyBlock(_propBlock);
    }

    private void UpdateWarriorVisuals()
    {
        if (_spawnedWarriorCount == CubeCount && _spawnedHadYurt == HasYurt)
        {
            foreach (GameObject w in _activeWarriors)
                if (w != null) ApplyOwnerTintToObject(w);
            return;
        }
        _spawnedWarriorCount = CubeCount;
        _spawnedHadYurt      = HasYurt;

        foreach (GameObject w in _activeWarriors)
            if (w != null) Destroy(w);
        _activeWarriors.Clear();

        if (CubeCount <= 0 || Terrain == TerrainType.Water) { HideCountLabel(); return; }

        int toShow = Mathf.Min(CubeCount, MaxWarriors);
        Vector3[] positions = Formations[toShow - 1];

        for (int i = 0; i < toShow; i++)
        {
            GameObject w = SpawnWarrior(positions[i]);
            ApplyOwnerTintToObject(w);
            _activeWarriors.Add(w);
        }

        if (CubeCount > MaxWarriors) ShowCountLabel(CubeCount);
        else                         HideCountLabel();
    }

    private GameObject SpawnWarrior(Vector3 localPos)
    {
        float lift = Terrain == TerrainType.Mountain ? MountainYLift : 0f;
        GameObject w;

        if (warriorPrefab != null)
        {
            w = Instantiate(warriorPrefab, transform);
            w.transform.localPosition = localPos + new Vector3(0f, WarriorYOffset + lift, 0f);
            w.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            w.transform.localScale    = Vector3.one * WarriorScale;
            foreach (Collider c in w.GetComponentsInChildren<Collider>(true)) Destroy(c);
            PlayIdleAnimation(w);
        }
        else
        {
            w = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            w.transform.SetParent(transform, false);
            w.transform.localPosition = localPos + new Vector3(0f, 0.22f + lift, 0f);
            w.transform.localScale    = new Vector3(0.13f, 0.18f, 0.13f);
            Destroy(w.GetComponent<Collider>());
        }
        return w;
    }

    private static void PlayIdleAnimation(GameObject w)
    {
        Animator anim = w.GetComponentInChildren<Animator>(true);
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var clips = anim.runtimeAnimatorController.animationClips;
            if (clips.Length > 0) anim.Play(clips[0].name);
            return;
        }
        Animation leg = w.GetComponentInChildren<Animation>(true);
        if (leg != null && leg.clip != null) leg.Play();
    }

    public void PlayAttackAnimation() => StartCoroutine(TemporaryAnim(1, 1.1f));
    public void PlayWalkAnimation()   => StartCoroutine(TemporaryAnim(2, 0.7f));

    private IEnumerator TemporaryAnim(int idx, float dur)
    {
        foreach (GameObject w in _activeWarriors)
        {
            if (w == null) continue;
            Animator anim = w.GetComponentInChildren<Animator>(true);
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                var clips = anim.runtimeAnimatorController.animationClips;
                if (clips.Length > idx) anim.Play(clips[idx].name);
            }
        }
        yield return new WaitForSeconds(dur);
        foreach (GameObject w in _activeWarriors)
            if (w != null) PlayIdleAnimation(w);
    }

    private void ShowCountLabel(int n) { if (cubeCountText != null) { cubeCountText.text = n.ToString(); cubeCountText.gameObject.SetActive(true); } }
    private void HideCountLabel()      { if (cubeCountText != null) cubeCountText.gameObject.SetActive(false); }

    private void UpdateYurtVisuals()
    {
        if (HasYurt && _activeYurtInstance == null)
        {
            float lift = Terrain == TerrainType.Mountain ? MountainYLift : 0f;
            if (yurtPrefab != null)
            {
                _activeYurtInstance = Instantiate(yurtPrefab, transform);
                _activeYurtInstance.transform.localPosition = new Vector3(0f, lift, 0f);
                _activeYurtInstance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                MakeSemiTransparent(_activeYurtInstance);
                ApplyOwnerTintToObject(_activeYurtInstance);
                StartCoroutine(AutoScaleYurt(_activeYurtInstance));
            }
            else
            {
                _activeYurtInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _activeYurtInstance.transform.SetParent(transform, false);
                _activeYurtInstance.transform.localPosition = new Vector3(0f, 0.6f + lift, 0f);
                _activeYurtInstance.transform.localScale    = Vector3.one * 0.3f;
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
        if (_activeYurtInstance != null) ApplyOwnerTintToObject(_activeYurtInstance);
    }

    public void RegisterDecoRenderer(Renderer r) { if (r != null) _decoRenderers.Add(r); }

    private void UpdateDecoOpacity()
    {
        if (_decoRenderers.Count == 0) return;
        float alpha = _isHovered ? 0.70f
                    : (CubeCount > 0 || HasYurt) ? (Terrain == TerrainType.Mountain ? 0.05f : 0.22f)
                    : 0.52f;

        foreach (Renderer r in _decoRenderers)
        {
            if (r == null || r.material == null) continue;
            if (r.material.HasProperty(BaseColorId)) { Color c = r.material.GetColor(BaseColorId); c.a = alpha; r.material.SetColor(BaseColorId, c); }
            if (r.material.HasProperty(ColorId))     { Color c = r.material.GetColor(ColorId);     c.a = alpha; r.material.SetColor(ColorId,     c); }
        }
    }

    private void ApplyOwnerTintToObject(GameObject go)
    {
        if (go == null) return;
        Color tint = (OwnerPlayerId >= 0 && OwnerPlayerId < PlayerTints.Length)
            ? PlayerTints[OwnerPlayerId]
            : new Color(0.55f, 0.55f, 0.55f);

        MaterialPropertyBlock pb = new MaterialPropertyBlock();
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
        {
            r.GetPropertyBlock(pb);
            pb.SetColor(BaseColorId, tint);
            pb.SetColor(ColorId,     tint);
            r.SetPropertyBlock(pb);
        }
    }

    private static void MakeSemiTransparent(GameObject go, float alpha = 0.45f)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
        {
            Material mat = new Material(r.sharedMaterial);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
            if (mat.HasProperty("_ZWrite"))  mat.SetFloat("_ZWrite",  0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            if (mat.HasProperty("_BaseColor")) { Color c = mat.GetColor("_BaseColor"); c.a = alpha; mat.SetColor("_BaseColor", c); }
            if (mat.HasProperty("_Color"))     { Color c = mat.GetColor("_Color");     c.a = alpha; mat.SetColor("_Color",     c); }
            r.material = mat;
        }
    }

    private IEnumerator AutoScaleYurt(GameObject go)
    {
        yield return null;
        if (go == null) yield break;
        Transform root = go.transform.childCount > 0 ? go.transform.GetChild(0) : go.transform;
        bool kept = false;
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.GetComponentInChildren<Renderer>(true) != null)
            { if (!kept) kept = true; else child.gameObject.SetActive(false); }
        }
        go.transform.localScale = Vector3.one * YurtScale;
    }

    private static Color ReadBaseColor(Renderer r)
    {
        if (r.sharedMaterial == null) return Color.white;
        if (r.sharedMaterial.HasProperty(BaseColorId)) return r.sharedMaterial.GetColor(BaseColorId);
        if (r.sharedMaterial.HasProperty(ColorId))     return r.sharedMaterial.GetColor(ColorId);
        return Color.white;
    }
}
