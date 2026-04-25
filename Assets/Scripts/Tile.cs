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
    private List<GameObject>      _activeWarriors      = new List<GameObject>();
    private int                   _spawnedWarriorCount = -1;

    private MaterialPropertyBlock _propBlock;
    private bool                  _isSelected     = false;
    private bool                  _isHighlighted  = false;
    private Color                 _highlightColor = Color.white;
    private Color                 _baseColor      = Color.white;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");

    private static readonly Color _neutralTint  = Color.white;
    private static readonly Color _player0Tint  = new Color(0.55f, 0.70f, 1.00f, 1f);
    private static readonly Color _player1Tint  = new Color(1.00f, 0.55f, 0.55f, 1f);
    private static readonly Color _selectedTint = new Color(1.00f, 0.90f, 0.10f, 1f);

    private const float YurtScale        = 1.0f;
    private const float WarriorScale     = 0.6f;
    private const float WarriorYOffset   = -0.15f;
    private const int   MaxShownWarriors = 5;

    private static readonly Vector3[][] Formations = new Vector3[][]
    {
        new[] { Vector3.zero },
        new[] { new Vector3(-0.28f, 0f,  0.00f), new Vector3( 0.28f, 0f,  0.00f) },
        new[] { new Vector3( 0.00f, 0f, -0.28f), new Vector3(-0.25f, 0f,  0.15f), new Vector3( 0.25f, 0f,  0.15f) },
        new[] { new Vector3(-0.24f, 0f, -0.24f), new Vector3( 0.24f, 0f, -0.24f),
                new Vector3(-0.24f, 0f,  0.24f), new Vector3( 0.24f, 0f,  0.24f) },
        new[] { new Vector3( 0.00f, 0f,  0.00f), new Vector3( 0.00f, 0f, -0.32f),
                new Vector3( 0.32f, 0f,  0.00f), new Vector3( 0.00f, 0f,  0.32f),
                new Vector3(-0.32f, 0f,  0.00f) },
    };

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

    public void SetHighlighted(bool highlighted, Color color)
    {
        _isHighlighted  = highlighted;
        _highlightColor = color;
        UpdateVisuals();
    }

    public void ClearHighlight()
    {
        _isHighlighted = false;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        ApplyTint();
        UpdateWarriorVisuals();
        UpdateYurtVisuals();
    }

    private void ApplyTint()
    {
        if (meshRenderer == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(_propBlock);

        Color tint;
        if (_isSelected)
            tint = _baseColor * _selectedTint;
        else if (_isHighlighted)
            tint = Color.Lerp(_baseColor, _highlightColor, 0.60f);
        else if (OwnerPlayerId == 0)
            tint = Color.Lerp(_baseColor, _baseColor * _player0Tint, 0.55f);
        else if (OwnerPlayerId == 1)
            tint = Color.Lerp(_baseColor, _baseColor * _player1Tint, 0.55f);
        else
            tint = _baseColor;

        _propBlock.SetColor(BaseColorId, tint);
        _propBlock.SetColor(ColorId,     tint);
        meshRenderer.SetPropertyBlock(_propBlock);
    }

    private void UpdateWarriorVisuals()
    {
        if (_spawnedWarriorCount == CubeCount) return;
        _spawnedWarriorCount = CubeCount;

        foreach (GameObject w in _activeWarriors)
            if (w != null) Destroy(w);
        _activeWarriors.Clear();

        if (CubeCount <= 0 || Terrain == TerrainType.Water)
        {
            HideCountLabel();
            return;
        }

        int toShow = Mathf.Min(CubeCount, MaxShownWarriors);
        Vector3[] positions = Formations[toShow - 1];

        for (int i = 0; i < toShow; i++)
            _activeWarriors.Add(SpawnWarrior(positions[i]));

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
            warrior.transform.localPosition = localPos + new Vector3(0f, WarriorYOffset, 0f);
            warrior.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            warrior.transform.localScale    = Vector3.one * WarriorScale;

            foreach (Collider c in warrior.GetComponentsInChildren<Collider>(true))
                Destroy(c);

            PlayIdleAnimation(warrior);
        }
        else
        {
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
        Animator anim = warrior.GetComponentInChildren<Animator>(true);
        if (anim != null && anim.runtimeAnimatorController != null) { anim.Play(0); return; }
        Animation legacyAnim = warrior.GetComponentInChildren<Animation>(true);
        if (legacyAnim != null && legacyAnim.clip != null) legacyAnim.Play();
    }

    public void PlayAttackAnimation() => StartCoroutine(TemporaryAnim(1, 1.1f));
    public void PlayWalkAnimation()   => StartCoroutine(TemporaryAnim(2, 0.7f));

    private System.Collections.IEnumerator TemporaryAnim(int stateIndex, float duration)
    {
        foreach (GameObject w in _activeWarriors)
        {
            if (w == null) continue;
            Animator anim = w.GetComponentInChildren<Animator>(true);
            if (anim != null && anim.runtimeAnimatorController != null &&
                anim.runtimeAnimatorController.animationClips.Length > stateIndex)
                anim.Play(stateIndex);
        }
        yield return new WaitForSeconds(duration);
        foreach (GameObject w in _activeWarriors)
            if (w != null) PlayIdleAnimation(w);
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

    private void UpdateYurtVisuals()
    {
        if (HasYurt && _activeYurtInstance == null)
        {
            if (yurtPrefab != null)
            {
                _activeYurtInstance = Instantiate(yurtPrefab, transform);
                _activeYurtInstance.transform.localPosition = Vector3.zero;
                _activeYurtInstance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                MakeSemiTransparent(_activeYurtInstance);
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

    private static void MakeSemiTransparent(GameObject go, float alpha = 0.45f)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
        {
            Material mat = new Material(r.sharedMaterial);
            if (mat.HasProperty("_Surface"))    mat.SetFloat("_Surface", 1);
            if (mat.HasProperty("_Blend"))      mat.SetFloat("_Blend", 0);
            if (mat.HasProperty("_ZWrite"))     mat.SetFloat("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            if (mat.HasProperty("_BaseColor"))
            { Color c = mat.GetColor("_BaseColor"); c.a = alpha; mat.SetColor("_BaseColor", c); }
            if (mat.HasProperty("_Color"))
            { Color c = mat.GetColor("_Color"); c.a = alpha; mat.SetColor("_Color", c); }
            r.material = mat;
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

    private static Color ReadBaseColor(Renderer r)
    {
        if (r.sharedMaterial == null) return Color.white;
        if (r.sharedMaterial.HasProperty(BaseColorId)) return r.sharedMaterial.GetColor(BaseColorId);
        if (r.sharedMaterial.HasProperty(ColorId))     return r.sharedMaterial.GetColor(ColorId);
        return Color.white;
    }
}
