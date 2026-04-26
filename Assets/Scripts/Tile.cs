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
    // MUST match UIManager.GetClanColor() and HexGridManager.BorderClanColors.
    private static readonly Color[] PlayerTints =
    {
        new Color(0.227f, 0.541f, 0.243f, 1f), // Argyn  — kazakh green  #3a8a3e
        new Color(0.784f, 0.251f, 0.180f, 1f), // Naiman — timurid red   #c8402e
        new Color(0.165f, 0.416f, 0.667f, 1f), // Kerey  — slavic blue   #2a6aaa
        new Color(0.831f, 0.690f, 0.267f, 1f), // Kongrat— ming gold     #d4b044
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
        // the PIECES and BORDERS show the owner).  65 % blend for clearly visible regions.
        Color regionBase = ContinentId >= 0 && ContinentId < RegionTints.Length
            ? Color.Lerp(_baseColor, RegionTints[ContinentId], 0.65f)
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

    public void PlayAttackAnimation() => StartCoroutine(TemporaryAnim("Attack", 1, 1.1f));
    public void PlayWalkAnimation()   => StartCoroutine(TemporaryAnim("Walk",   2, 0.7f));

    // Duration constants so callers can match their wait times.
    public const float WalkAnimDuration   = 0.7f;
    public const float AttackAnimDuration = 1.1f;
    public const float TravelDuration     = 1.0f;

    // Spawns a single warrior at `worldFrom`, plays the walking animation from
    // Assets/animation/Animation_Walking_withSkin.fbx (or the prefab's Mecanim Walk
    // clip if available), lerps it to `worldTo` over `duration` seconds with a
    // bounce + sway for liveliness, then destroys it.
    public IEnumerator SpawnTraveler(Vector3 worldFrom, Vector3 worldTo, float duration)
    {
        GameObject traveler;
        if (warriorPrefab != null)
        {
            traveler = Instantiate(warriorPrefab);
            traveler.transform.localScale = Vector3.one * WarriorScale;
        }
        else
        {
            traveler = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            traveler.transform.localScale = new Vector3(0.13f, 0.18f, 0.13f);
        }

        foreach (Collider c in traveler.GetComponentsInChildren<Collider>(true)) Destroy(c);

        Vector3 yOff = new Vector3(0f, WarriorYOffset, 0f);
        traveler.transform.position = worldFrom + yOff;

        Vector3 flatDir = worldTo - worldFrom; flatDir.y = 0f;
        Quaternion baseRot = flatDir.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(flatDir.normalized)
            : traveler.transform.rotation;
        traveler.transform.rotation = baseRot;

        ApplyOwnerTintToObject(traveler);
        StartWalkLoopOn(traveler);

        float t = 0f;
        while (t < duration && traveler != null)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            // Travel along a slight arc so it feels like real walking, not a slide.
            Vector3 pos = Vector3.Lerp(worldFrom, worldTo, u) + yOff;
            pos.y += Mathf.Sin(u * Mathf.PI) * 0.10f;            // overall hop
            pos.y += Mathf.Sin(u * Mathf.PI * 8f) * 0.018f;      // step bob

            // Subtle body sway — left/right rocking as the soldier strides.
            float sway = Mathf.Sin(u * Mathf.PI * 8f) * 4f;
            traveler.transform.SetPositionAndRotation(pos, baseRot * Quaternion.Euler(0f, 0f, sway));

            yield return null;
        }

        if (traveler != null) Destroy(traveler);
    }

    // Drives a looping walk on the given object. Tries Mecanim's Walk clip first,
    // and if none is found loads Animation_Walking_withSkin.fbx from Assets/animation/
    // and plays it via the Legacy Animation component (disabling Mecanim so it
    // doesn't fight the legacy driver).
    private static void StartWalkLoopOn(GameObject obj)
    {
        if (obj == null) return;

        Animator anim = obj.GetComponentInChildren<Animator>(true);
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (var c in anim.runtimeAnimatorController.animationClips)
            {
                if (c == null) continue;
                if (c.name.IndexOf("Walk", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    anim.speed = 1f;
                    anim.Play(c.name, 0, 0f);
                    anim.Update(0f);
                    return;
                }
            }
        }

        // Mecanim has no walk clip — load the FBX explicitly and drive via Legacy.
        AnimationClip walkClip = LoadLegacyClipByKeyword("Walk");
        if (walkClip == null) return;
        walkClip.wrapMode = WrapMode.Loop;

        if (anim != null) anim.enabled = false;

        GameObject host = anim != null ? anim.gameObject : obj;
        Animation legAnim = host.GetComponent<Animation>();
        if (legAnim == null) legAnim = host.AddComponent<Animation>();
        if (legAnim.GetClip(walkClip.name) == null) legAnim.AddClip(walkClip, walkClip.name);
        legAnim.clip             = walkClip;
        legAnim.wrapMode         = WrapMode.Loop;
        legAnim.playAutomatically = true;
        legAnim.Play(walkClip.name);
    }

    private IEnumerator TemporaryAnim(string keyword, int fallbackIdx, float dur)
    {
        foreach (GameObject w in _activeWarriors)
        {
            if (w == null) continue;

            // --- Mecanim path: search by name keyword, fall back to index ---
            Animator anim = w.GetComponentInChildren<Animator>(true);
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                var clips = anim.runtimeAnimatorController.animationClips;
                AnimationClip target = null;
                foreach (var c in clips)
                    if (c.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    { target = c; break; }
                if (target == null && clips.Length > fallbackIdx) target = clips[fallbackIdx];
                if (target != null) anim.Play(target.name);
                continue;
            }

            // --- Legacy Animation path: find clip already registered or load from FBX ---
            Animation legAnim = w.GetComponentInChildren<Animation>(true);
            if (legAnim == null) continue;

            // Check if the clip is already registered on this Animation component.
            bool found = false;
            foreach (AnimationState state in legAnim)
            {
                if (state.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                { legAnim.Play(state.name); found = true; break; }
            }
            if (!found)
            {
                AnimationClip clip = LoadLegacyClipByKeyword(keyword);
                if (clip != null)
                {
                    if (legAnim.GetClip(clip.name) == null) legAnim.AddClip(clip, clip.name);
                    legAnim.Play(clip.name);
                }
            }
        }
        yield return new WaitForSeconds(dur);
        foreach (GameObject w in _activeWarriors)
            if (w != null) PlayIdleAnimation(w);
    }

    // Loads an AnimationClip from the project FBX files by keyword (Editor only).
    private static AnimationClip LoadLegacyClipByKeyword(string keyword)
    {
#if UNITY_EDITOR
        string[] candidates = {
            "Assets/animation/Animation_Walking_withSkin.fbx",
            "Assets/animation/Animation_Attack_withSkin.fbx",
            "Assets/animation/Animation_Alert_withSkin.fbx",
            "Assets/KayKit/Meshy_AI_biped_Animation_Walking_withSkin.fbx",
        };
        foreach (string path in candidates)
        {
            if (path.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
            foreach (var asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is AnimationClip c && !c.name.StartsWith("__preview__"))
                {
                    // Legacy Animation requires the clip to have legacy=true.
                    AnimationClip copy = UnityEngine.Object.Instantiate(c);
                    copy.legacy = true;
                    return copy;
                }
            }
        }
#endif
        return null;
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
