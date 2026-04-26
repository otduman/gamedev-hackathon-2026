using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public static HexGridManager Instance { get; private set; }
    public System.Action OnGridReady;

    // BASE TILE MESHES — hex floor tiles, must have a Tile component.
    [Header("Tile Base Meshes (hex floor — need Tile component)")]
    [SerializeField] private GameObject[] plainPrefabs;
    [SerializeField] private GameObject[] mountainPrefabs;
    [SerializeField] private GameObject[] waterPrefabs;

    // Trees placed on plain tiles to create forests.
    [Header("Forest Decorations (plain tiles)")]
    [SerializeField] private GameObject[] treePrefabs;

    // Small rocks scattered on plain tiles.
    [Header("Rock Decorations (plain tiles)")]
    [SerializeField] private GameObject[] rockPrefabs;

    // Hills placed on mountain-terrain tiles (inner mountains).
    [Header("Hill Decorations (mountain tiles)")]
    [SerializeField] private GameObject[] hillPrefabs;

    // Large mountain FBX models for the outer border ring.
    [Header("Mountain Border Decorations (outer ring)")]
    [SerializeField] private GameObject[] mountainDecoPrefabs;

    [Header("Props (small thematic items)")]
    [SerializeField] private GameObject[] propPrefabs;

    [Header("Grid Settings")]
    [SerializeField] private int   mapRadius = 4;
    [SerializeField] private float hexSize   = 1.0f;

    public Dictionary<Vector2Int, Tile> Grid { get; private set; } = new Dictionary<Vector2Int, Tile>();

    // 60 English territory names (Kazakh theme)
    private static readonly string[] TerritoryNames =
    {
        "Almaty",    "Taraz",       "Shymkent",    "Turkestan",  "Sairam",
        "Balkhash",  "Kapchagay",   "Taldykorgan", "Kopal",      "Tekeli",
        "Astana",    "Karaganda",   "Pavlodar",    "Semey",      "Oskemen",
        "Aktobe",    "Uralsk",      "Atyrau",      "Aktau",      "Zhanaozen",
        "Kostanay",  "Zhezkazgan",  "Ulytau",      "Arkalyk",    "Rudny",
        "Kyzylorda", "Aralsk",      "Shardara",    "Zhetisai",   "Arys",
        "Petropavl", "Kokshetau",   "Stepnogorsk", "Atbasar",    "Esil",
        "Shetpe",    "Mangistau",   "Beyneu",      "Makat",      "Emba",
        "Priozersk", "Saryshagan",  "Moyynkum",    "Zhusaly",    "Baikonur",
        "Khantau",   "Otegen Batyr","Qonay",       "Saykhyn",    "Shalkar",
        "Khromtau",  "Kandyagash",  "Turgay",      "Zhitikara",  "Tobyl",
        "Dzhambul",  "Makinsk",     "Ereymentau",  "Shakhtinsk", "Zhambyl"
    };

    // Borders: one LineRenderer per edge per tile
    private readonly Dictionary<Tile, LineRenderer[]> _borderSegments = new Dictionary<Tile, LineRenderer[]>();

    private static readonly Vector2Int[] EdgeNeighborDirs =
    {
        new Vector2Int( 0,  1),
        new Vector2Int(-1,  1),
        new Vector2Int(-1,  0),
        new Vector2Int( 0, -1),
        new Vector2Int( 1, -1),
        new Vector2Int( 1,  0),
    };

    // Four clan border colours + neutral
    // MUST match UIManager.GetClanColor() and Tile.PlayerTints.
    private static readonly Color BorderNeutral = new Color(0.10f, 0.06f, 0.02f, 0.40f);
    private static readonly Color[] BorderClanColors =
    {
        new Color(0.227f, 0.541f, 0.243f, 1f), // Argyn   — kazakh green  #3a8a3e
        new Color(0.784f, 0.251f, 0.180f, 1f), // Naiman  — timurid red   #c8402e
        new Color(0.165f, 0.416f, 0.667f, 1f), // Kerey   — slavic blue   #2a6aaa
        new Color(0.831f, 0.690f, 0.267f, 1f), // Kongrat  — ming gold    #d4b044
    };

    private static readonly Vector3[] DecoSpreadOffsets =
    {
        new Vector3( 0.00f, 0f,  0.00f), new Vector3( 0.30f, 0f,  0.15f),
        new Vector3(-0.30f, 0f,  0.15f), new Vector3( 0.15f, 0f, -0.30f),
        new Vector3(-0.15f, 0f, -0.30f),
    };

    private static readonly Color FallbackColorPlain    = new Color(0.20f, 0.50f, 0.20f);
    private static readonly Color FallbackColorMountain = new Color(0.50f, 0.50f, 0.50f);
    private static readonly Color FallbackColorWater    = new Color(0.10f, 0.30f, 0.70f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        EnsureEventSystem();
    }

    private void Start()
    {
        AutoPopulateAssets();
        ResourcesLoadFallback();
        LogInspectorSetup();
        GenerateHexagonalMap();
    }

    // Runtime fallback that works in BUILDS (AutoPopulateAssets uses AssetDatabase
    // which only exists in the editor). Loads tile prefabs and decorations from Resources/.
    private void ResourcesLoadFallback()
    {
        if (!HasValidPrefab(plainPrefabs))    plainPrefabs    = LoadResourcesArray("Prefabs/Tile_Grass");
        if (!HasValidPrefab(mountainPrefabs)) mountainPrefabs = LoadResourcesArray("Prefabs/Tile_Mountain");
        if (!HasValidPrefab(waterPrefabs))    waterPrefabs    = LoadResourcesArray("Prefabs/Tile_Water");

        if (!HasValidPrefab(treePrefabs))     treePrefabs     = LoadResourcesArray(
            "Decorations/nature/trees_A_medium",
            "Decorations/nature/trees_A_small",
            "Decorations/nature/tree_single_A");

        if (!HasValidPrefab(rockPrefabs))     rockPrefabs     = LoadResourcesArray(
            "Decorations/nature/rock_single_A",
            "Decorations/nature/rock_single_B",
            "Decorations/nature/rock_single_C",
            "Decorations/nature/rock_single_D",
            "Decorations/nature/rock_single_E");

        if (!HasValidPrefab(hillPrefabs))     hillPrefabs     = LoadResourcesArray(
            "Decorations/nature/hills_A",
            "Decorations/nature/hills_A_trees",
            "Decorations/nature/hills_B",
            "Decorations/nature/hills_B_trees",
            "Decorations/nature/hills_C",
            "Decorations/nature/hills_C_trees");

        if (!HasValidPrefab(mountainDecoPrefabs)) mountainDecoPrefabs = LoadResourcesArray(
            "Decorations/nature/mountain_A",
            "Decorations/nature/mountain_A_grass",
            "Decorations/nature/mountain_A_grass_trees",
            "Decorations/nature/mountain_B",
            "Decorations/nature/mountain_B_grass",
            "Decorations/nature/mountain_B_grass_trees",
            "Decorations/nature/mountain_C",
            "Decorations/nature/mountain_C_grass",
            "Decorations/nature/mountain_C_grass_trees");

        if (!HasValidPrefab(propPrefabs))     propPrefabs     = LoadResourcesArray(
            "Decorations/props/bucket_arrows",
            "Decorations/props/sack",
            "Decorations/props/target",
            "Decorations/props/tent",
            "Decorations/props/weaponrack");
    }

    private static GameObject[] LoadResourcesArray(params string[] resourcePaths)
    {
        var list = new List<GameObject>();
        foreach (string path in resourcePaths)
        {
            GameObject p = Resources.Load<GameObject>(path);
            if (p != null) list.Add(p);
        }
        return list.ToArray();
    }

    // Guarantees an EventSystem exists regardless of scene setup.
    private static void EnsureEventSystem()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null) return;
        GameObject esGo = new GameObject("EventSystem");
        DontDestroyOnLoad(esGo);
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
    }

    // Automatically loads all KayKit assets so no manual Inspector setup is needed.
    // Inspector assignments take priority — only fills slots that are still empty.
    private void AutoPopulateAssets()
    {
#if UNITY_EDITOR
        const string nat  = "Assets/KayKit/fbx(unity)/decoration/nature/";
        const string pre  = "Assets/Prefabs/";

        if (!HasValidPrefab(plainPrefabs))
            plainPrefabs = EditorLoadAll(pre + "Tile_Grass.prefab");

        if (!HasValidPrefab(mountainPrefabs))
            mountainPrefabs = EditorLoadAll(pre + "Tile_Mountain.prefab");

        if (!HasValidPrefab(waterPrefabs))
            waterPrefabs = EditorLoadAll(pre + "Tile_Water.prefab");

        if (!HasValidPrefab(treePrefabs))
            treePrefabs = EditorLoadAll(
                nat + "trees_A_medium.fbx",
                nat + "trees_A_small.fbx",
                nat + "tree_single_A.fbx");

        if (!HasValidPrefab(rockPrefabs))
            rockPrefabs = EditorLoadAll(
                nat + "rock_single_A.fbx",
                nat + "rock_single_B.fbx",
                nat + "rock_single_C.fbx",
                nat + "rock_single_D.fbx",
                nat + "rock_single_E.fbx");

        if (!HasValidPrefab(hillPrefabs))
            hillPrefabs = EditorLoadAll(
                nat + "hills_A.fbx",
                nat + "hills_A_trees.fbx",
                nat + "hills_B.fbx",
                nat + "hills_B_trees.fbx",
                nat + "hills_C.fbx",
                nat + "hills_C_trees.fbx");

        if (!HasValidPrefab(mountainDecoPrefabs))
            mountainDecoPrefabs = EditorLoadAll(
                nat + "mountain_A.fbx",
                nat + "mountain_A_grass.fbx",
                nat + "mountain_A_grass_trees.fbx",
                nat + "mountain_B.fbx",
                nat + "mountain_B_grass.fbx",
                nat + "mountain_B_grass_trees.fbx",
                nat + "mountain_C.fbx",
                nat + "mountain_C_grass.fbx",
                nat + "mountain_C_grass_trees.fbx");
                
        const string prop = "Assets/KayKit/fbx(unity)/decoration/props/";
        if (!HasValidPrefab(propPrefabs))
            propPrefabs = EditorLoadAll(
                prop + "bucket_arrows.fbx",
                prop + "sack.fbx",
                prop + "target.fbx",
                prop + "tent.fbx",
                prop + "weaponrack.fbx");
#endif
    }

#if UNITY_EDITOR
    private static GameObject[] EditorLoadAll(params string[] paths)
    {
        var list = new System.Collections.Generic.List<GameObject>();
        foreach (string p in paths)
        {
            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go != null) list.Add(go);
            else Debug.LogWarning($"[HexGridManager] Asset not found: {p}");
        }
        return list.ToArray();
    }
#endif

    // Prints every Inspector array to the Console so you can verify what's assigned.
    [ContextMenu("Log Inspector Setup")]
    private void LogInspectorSetup()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[HexGridManager] Inspector array contents:");
        AppendArray(sb, "plainPrefabs    (BASE tile mesh — flat hex, e.g. Tile_Grass)",    plainPrefabs);
        AppendArray(sb, "mountainPrefabs (BASE tile mesh — e.g. Tile_Mountain)",           mountainPrefabs);
        AppendArray(sb, "waterPrefabs    (BASE tile mesh — e.g. Tile_Water)",              waterPrefabs);
        AppendArray(sb, "treePrefabs          (forest deco on plains)", treePrefabs);
        AppendArray(sb, "rockPrefabs          (rock scatter on plains)", rockPrefabs);
        AppendArray(sb, "hillPrefabs          (hill deco on inner mountains)", hillPrefabs);
        AppendArray(sb, "mountainDecoPrefabs  (large mountain FBX on outer border ring)", mountainDecoPrefabs);
        AppendArray(sb, "propPrefabs          (small thematic props on plains)", propPrefabs);
        Debug.Log(sb.ToString());
    }

    private static void AppendArray(System.Text.StringBuilder sb, string label, GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) { sb.AppendLine($"  {label}: EMPTY"); return; }
        sb.AppendLine($"  {label}: {arr.Length} slot(s)");
        for (int i = 0; i < arr.Length; i++)
            sb.AppendLine($"    [{i}] {(arr[i] == null ? "<null>" : arr[i].name)}");
    }

    private void GenerateHexagonalMap()
    {
        // Generate two extra rings of mountain border beyond the playable area.
        int genRadius = mapRadius + 2;

        for (int q = -genRadius; q <= genRadius; q++)
        {
            int r1 = Mathf.Max(-genRadius, -q - genRadius);
            int r2 = Mathf.Min(genRadius,  -q + genRadius);

            for (int r = r1; r <= r2; r++)
            {
                Vector2Int hexCoords = new Vector2Int(q, r);
                Vector3    worldPos  = CalculateHexPosition(hexCoords);
                TerrainType terrain  = RemapTerrainToAvailable(DetermineTerrain(q, r));

                GameObject selectedPrefab = GetPrefabForTerrain(terrain);
                GameObject tileGo;

                if (selectedPrefab != null)
                {
                    tileGo = Instantiate(selectedPrefab, worldPos, selectedPrefab.transform.rotation, transform);
                    tileGo.name = $"Tile_{q}_{r}";
                    tileGo.transform.Rotate(0, Random.Range(0, 6) * 60f, 0, Space.World);

                    int dist = HexDistanceFromOrigin(q, r);
                    if (terrain == TerrainType.Mountain)
                    {
                        // Progressive height: inner border modest, outermost taller.
                        float yScale = dist > mapRadius ? 1.3f : 1.0f;
                        tileGo.transform.localScale = new Vector3(1f, yScale, 1f);
                    }

                    Tile tile = tileGo.GetComponent<Tile>();
                    if (tile == null) { Debug.LogError($"Prefab '{selectedPrefab.name}' missing Tile component."); Destroy(tileGo); continue; }

                    tile.Initialize(hexCoords, terrain);
                    ForceTileRendersOpaque(tileGo);
                    Grid.Add(hexCoords, tile);
                }
                else
                {
                    GameObject fallbackGo = CreateFallbackTile(worldPos, terrain);
                    fallbackGo.name = $"Tile_{q}_{r}";
                    Tile tile = fallbackGo.GetComponent<Tile>();
                    tile.Initialize(hexCoords, terrain);
                    Grid.Add(hexCoords, tile);
                }
            }
        }

        PlaceDecorations();
        AssignTerritoryNames();
        AddHexBorders();
        CreateBackgroundPlane();

        OnGridReady?.Invoke();
        if (GetComponent<AmbientEnvironment>() == null) gameObject.AddComponent<AmbientEnvironment>();
        if (GetComponent<WaterAnimator>()      == null) gameObject.AddComponent<WaterAnimator>();
        if (GetComponent<AudioManager>()       == null) gameObject.AddComponent<AudioManager>();
    }

    // Assigns English territory names and continent IDs to all passable tiles.
    private void AssignTerritoryNames()
    {
        List<Tile> passable = new List<Tile>();
        foreach (Tile t in Grid.Values)
            if (t.Terrain != TerrainType.Water && t.Terrain != TerrainType.Mountain)
                passable.Add(t);

        // Shuffle so names aren't always assigned to the same positions
        for (int i = passable.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Tile tmp = passable[i]; passable[i] = passable[j]; passable[j] = tmp;
        }

        for (int i = 0; i < passable.Count; i++)
        {
            string name = i < TerritoryNames.Length ? TerritoryNames[i] : $"Territory {i + 1}";
            int continent = GetContinentForTile(passable[i].HexCoordinates);
            passable[i].SetTerritoryInfo(name, continent);
        }
    }

    // Six angular sectors → six continents (regions).
    private int GetContinentForTile(Vector2Int hexCoords)
    {
        Vector3 world = CalculateHexPosition(hexCoords);
        float angle = Mathf.Atan2(world.z, world.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return (int)(angle / 60f) % 6;
    }

    private void PlaceDecorations()
    {
        foreach (Tile tile in Grid.Values)
        {
            if (tile.HasYurt) continue;

            GameObject[] primaryPool   = null;
            GameObject[] secondaryPool = null;
            int   maxCount    = 1;
            float spawnChance = 0f;

            switch (tile.Terrain)
            {
                case TerrainType.Plain:
                    if (Random.value < 0.20f && HasValidPrefab(propPrefabs))
                    {
                        primaryPool = propPrefabs; secondaryPool = rockPrefabs; maxCount = 2; spawnChance = 0.75f;
                    }
                    else
                    {
                        primaryPool = treePrefabs; secondaryPool = rockPrefabs; maxCount = 2; spawnChance = 0.55f;
                    }
                    break;
                case TerrainType.Mountain:
                    int dist = HexDistanceFromOrigin(tile.HexCoordinates.x, tile.HexCoordinates.y);
                    if (dist >= mapRadius)
                    {
                        // Progressive border: inner ring = sparse hills,
                        // middle = hills + some mountains, outer = mostly mountains.
                        int ringDepth = dist - mapRadius; // 0, 1, 2
                        if (ringDepth == 0)
                        {
                            // First border ring — sparse hills only
                            primaryPool = hillPrefabs; secondaryPool = rockPrefabs;
                            maxCount = 1; spawnChance = 0.55f;
                        }
                        else if (ringDepth == 1)
                        {
                            // Middle ring — hills with occasional mountains
                            primaryPool = hillPrefabs;
                            secondaryPool = HasValidPrefab(mountainDecoPrefabs) ? mountainDecoPrefabs : hillPrefabs;
                            maxCount = 1; spawnChance = 0.75f;
                        }
                        else
                        {
                            // Outer ring — mostly mountains
                            primaryPool = HasValidPrefab(mountainDecoPrefabs) ? mountainDecoPrefabs : hillPrefabs;
                            secondaryPool = hillPrefabs;
                            maxCount = 1; spawnChance = 0.85f;
                        }
                    }
                    else
                    {
                        // Inner mountain tiles (in the playable transition zone)
                        primaryPool   = HasValidPrefab(hillPrefabs) ? hillPrefabs : rockPrefabs;
                        secondaryPool = rockPrefabs;
                        maxCount = 1; spawnChance = 0.65f;
                    }
                    break;
                case TerrainType.Water:
                    continue;
            }

            if (Random.value > spawnChance) continue;

            int count = Random.Range(1, maxCount + 1);
            List<Vector3> offsets = new List<Vector3>(DecoSpreadOffsets);
            for (int i = offsets.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Vector3 tmp = offsets[i]; offsets[i] = offsets[j]; offsets[j] = tmp;
            }

            bool usePrimary = secondaryPool == null || secondaryPool.Length == 0 || Random.value < 0.6f;
            GameObject[] tilePool = usePrimary ? primaryPool : secondaryPool;
            if (tilePool == null || tilePool.Length == 0) continue;

            bool isForest = treePrefabs != null && treePrefabs.Length > 0 && tilePool == treePrefabs
                         && tile.Terrain == TerrainType.Plain;
            if (isForest) tile.SetHasForest(true);

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = PickValid(tilePool);
                if (prefab == null) continue;
                Vector3 jitter = new Vector3(Random.Range(-0.08f, 0.08f), 0f, Random.Range(-0.08f, 0.08f));
                GameObject deco = Instantiate(prefab, tile.transform);
                deco.transform.localPosition = offsets[i % offsets.Count] + jitter;
                deco.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                
                if (tilePool == propPrefabs)
                    deco.transform.localScale = Vector3.one * Random.Range(0.18f, 0.28f);
                else
                    deco.transform.localScale = Vector3.one * Random.Range(0.65f, 0.85f);
                    
                foreach (Collider c in deco.GetComponentsInChildren<Collider>()) Destroy(c);
                MakeDecoTransparent(deco, 0.52f);
                foreach (Renderer rr in deco.GetComponentsInChildren<Renderer>())
                    tile.RegisterDecoRenderer(rr);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Borders — all 6 edges always visible
    // -------------------------------------------------------------------------

    private void AddHexBorders()
    {
        const float borderRadius = 0.97f;
        const float yOffset      = 0.06f;

        Shader chosen = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");

        foreach (Tile tile in Grid.Values)
        {
            GameObject root = new GameObject("HexBorderRoot");
            root.transform.SetParent(tile.transform, false);
            root.transform.localPosition = new Vector3(0f, yOffset, 0f);

            LineRenderer[] segs = new LineRenderer[6];
            float r = hexSize * borderRadius;

            for (int i = 0; i < 6; i++)
            {
                GameObject seg = new GameObject($"Edge{i}");
                seg.transform.SetParent(root.transform, false);
                LineRenderer lr = seg.AddComponent<LineRenderer>();
                lr.useWorldSpace     = false;
                lr.positionCount     = 2;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows    = false;

                float a0 = Mathf.Deg2Rad * (60f * i + 30f);
                float a1 = Mathf.Deg2Rad * (60f * (i + 1) + 30f);
                lr.SetPosition(0, new Vector3(Mathf.Cos(a0) * r, 0f, Mathf.Sin(a0) * r));
                lr.SetPosition(1, new Vector3(Mathf.Cos(a1) * r, 0f, Mathf.Sin(a1) * r));

                if (chosen != null) lr.material = new Material(chosen);
                segs[i] = lr;
            }
            _borderSegments[tile] = segs;
        }

        RefreshAllBorders();
    }

    public void RefreshAllBorders()
    {
        foreach (Tile tile in _borderSegments.Keys) RefreshBorders(tile);
    }

    public void RefreshBorders(Tile tile)
    {
        if (tile == null || !_borderSegments.TryGetValue(tile, out LineRenderer[] segs)) return;

        // Water and Mountain tiles have no borders — water isn't territory,
        // mountains form natural boundaries and don't need outlines.
        if (tile.Terrain == TerrainType.Water || tile.Terrain == TerrainType.Mountain)
        {
            foreach (LineRenderer lr in segs) if (lr != null) lr.enabled = false;
            return;
        }

        // Region colour for the border — based on ContinentId, not player.
        Color regionCol = tile.ContinentId >= 0 && tile.ContinentId < Tile.RegionTints.Length
            ? Tile.RegionTints[tile.ContinentId]
            : BorderNeutral;

        Color targetColor = regionCol;
        float widthMult = 1f;
        
        if (tile.IsSelected)
        {
            targetColor = new Color(1f, 0.98f, 0.85f); // Neutral bright cream/white
            widthMult = 2.5f;
        }
        else if (tile.IsHighlighted)
        {
            targetColor = tile.HighlightColor;
            widthMult = 2.0f;
        }

        for (int i = 0; i < 6; i++)
        {
            LineRenderer lr = segs[i];
            if (lr == null) continue;

            lr.enabled = true;
            lr.startWidth = lr.endWidth = 0.05f * widthMult;

            Vector2Int neighborCoord = tile.HexCoordinates + EdgeNeighborDirs[i];
            bool boundary = !Grid.TryGetValue(neighborCoord, out Tile neighbor)
                         || neighbor.ContinentId != tile.ContinentId
                         || neighbor.Terrain == TerrainType.Water
                         || neighbor.Terrain == TerrainType.Mountain;

            if (tile.IsSelected || tile.IsHighlighted)
            {
                // Always show full border brightly if highlighted
                lr.startColor = lr.endColor = targetColor;
                if (lr.material.HasProperty("_EmissionColor"))
                    lr.material.SetColor("_EmissionColor", targetColor * 1.5f);
                lr.startWidth = lr.endWidth = 0.05f * widthMult;
                lr.transform.localPosition = new Vector3(0f, 0.02f, 0f); // lift slightly to avoid z-fighting with weak neighbors
            }
            else
            {
                lr.startColor = lr.endColor = boundary
                    ? regionCol
                    : new Color(regionCol.r, regionCol.g, regionCol.b, 0.25f);
                if (lr.material.HasProperty("_EmissionColor"))
                    lr.material.SetColor("_EmissionColor", Color.black);
                lr.startWidth = lr.endWidth = boundary ? 0.055f : 0.010f;
                lr.transform.localPosition = Vector3.zero;
            }

            if (lr.material != null)
            {
                Color c = lr.startColor;
                if (lr.material.HasProperty("_BaseColor")) lr.material.SetColor("_BaseColor", c);
                if (lr.material.HasProperty("_Color"))     lr.material.SetColor("_Color",     c);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Terrain generation
    // -------------------------------------------------------------------------

    private TerrainType DetermineTerrain(int q, int r)
    {
        int   dist = HexDistanceFromOrigin(q, r);
        float roll = Random.value;

        // Anything at or beyond the playable radius is mountain border.
        if (dist >= mapRadius) return TerrainType.Mountain;

        if (dist <= 2) return TerrainType.Plain;

        // dist == 3 (transition zone) — mostly plains with rare features
        if (roll < 0.80f) return TerrainType.Plain;
        if (roll < 0.88f) return TerrainType.Mountain;
        return TerrainType.Water;
    }

    private TerrainType RemapTerrainToAvailable(TerrainType terrain)
    {
        // Mountain and Water fall through to CreateFallbackTile so they always look distinct.
        return terrain;
    }

    private static bool HasValidPrefab(GameObject[] arr)
    {
        if (arr == null) return false;
        foreach (GameObject go in arr) if (go != null) return true;
        return false;
    }

    private GameObject GetPrefabForTerrain(TerrainType terrain)
    {
        GameObject[] raw = terrain switch
        {
            TerrainType.Plain    => plainPrefabs,
            TerrainType.Mountain => mountainPrefabs,
            TerrainType.Water    => waterPrefabs,
            _                    => plainPrefabs,
        };
        // Mountain and Water must look visually distinct — never fall back to plain prefab.
        // Return null so GenerateHexagonalMap uses CreateFallbackTile instead.
        if (terrain == TerrainType.Mountain || terrain == TerrainType.Water)
            return PickValid(raw);
        return PickValid(raw) ?? PickValid(plainPrefabs);
    }

    private static GameObject PickValid(GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        int count = 0;
        foreach (GameObject go in arr) if (go != null) count++;
        if (count == 0) return null;
        int target = Random.Range(0, count), idx = 0;
        foreach (GameObject go in arr) { if (go == null) continue; if (idx == target) return go; idx++; }
        return null;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public Vector3 CalculateHexPosition(Vector2Int hexCoords)
    {
        const float spread = 1.06f;
        float x = hexSize * spread * Mathf.Sqrt(3f) * (hexCoords.x + hexCoords.y / 2f);
        float z = hexSize * spread * 1.5f * hexCoords.y;
        return new Vector3(x, 0f, z);
    }

    private int HexDistanceFromOrigin(int q, int r) =>
        Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(-q - r));

    public List<Tile> GetNeighbors(Vector2Int hexCoords)
    {
        List<Tile> result = new List<Tile>();
        Vector2Int[] dirs = { new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(0,-1),
                              new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,1) };
        foreach (Vector2Int d in dirs)
            if (Grid.TryGetValue(hexCoords + d, out Tile t)) result.Add(t);
        return result;
    }

    // -------------------------------------------------------------------------
    // Fallback tile (no prefab assigned)
    // -------------------------------------------------------------------------

    private GameObject CreateFallbackTile(Vector3 worldPos, TerrainType terrain)
    {
        GameObject root = new GameObject("FallbackTile");
        root.transform.position = worldPos;
        root.transform.SetParent(transform, true);

        float disc    = hexSize * 0.95f;
        bool  isMtn   = terrain == TerrainType.Mountain;
        bool  isWater = terrain == TerrainType.Water;

        // Base hex disc
        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.transform.SetParent(root.transform, false);
        cyl.transform.localPosition = Vector3.zero;
        cyl.transform.localScale = new Vector3(disc, isMtn ? 0.55f : (isWater ? 0.05f : 0.12f), disc);
        Destroy(cyl.GetComponent<Collider>());

        Renderer rend = cyl.GetComponent<Renderer>();
        Color tileColor = terrain switch
        {
            TerrainType.Mountain => FallbackColorMountain,
            TerrainType.Water    => FallbackColorWater,
            _                    => FallbackColorPlain,
        };
        if (rend != null)
        {
            Material mat = new Material(rend.sharedMaterial);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tileColor);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     tileColor);
            rend.material = mat;
        }

        // Peak cone on top of mountain fallback so it reads clearly
        if (isMtn)
        {
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cone.transform.SetParent(root.transform, false);
            cone.transform.localPosition = new Vector3(0f, 0.70f, 0f);
            cone.transform.localScale    = new Vector3(disc * 0.55f, 0.40f, disc * 0.55f);
            Destroy(cone.GetComponent<Collider>());
            Renderer cr = cone.GetComponent<Renderer>();
            if (cr != null)
            {
                Material m = new Material(cr.sharedMaterial);
                Color snow = new Color(0.82f, 0.82f, 0.82f);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", snow);
                if (m.HasProperty("_Color"))     m.SetColor("_Color",     snow);
                cr.material = m;
            }
        }

        float colHeight = isMtn ? 1.6f : 0.6f;
        float colCenter = isMtn ? 0.50f : 0.15f;
        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.radius = hexSize * 0.9f; col.height = colHeight; col.center = new Vector3(0f, colCenter, 0f);

        Tile tile = root.AddComponent<Tile>();
        tile.SetPrimitiveMeshRenderer(rend);
        return root;
    }

    // -------------------------------------------------------------------------
    // Material helpers
    // -------------------------------------------------------------------------

    private static void ForceTileRendersOpaque(GameObject go)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r.sharedMaterial == null || !r.sharedMaterial.HasProperty("_Surface")) continue;
            Material mat = new Material(r.sharedMaterial);
            mat.SetFloat("_Surface", 0f);
            mat.SetFloat("_ZWrite",  1f);
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            if (mat.HasProperty("_BaseColor")) { Color c = mat.GetColor("_BaseColor"); c.a = 1f; mat.SetColor("_BaseColor", c); }
            if (mat.HasProperty("_Color"))     { Color c = mat.GetColor("_Color");     c.a = 1f; mat.SetColor("_Color",     c); }
            r.material = mat;
        }
    }

    private static void MakeDecoTransparent(GameObject go, float alpha)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            Material mat = new Material(r.sharedMaterial);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
            if (mat.HasProperty("_ZWrite"))  mat.SetFloat("_ZWrite",  0);
            mat.renderQueue = 3000;
            if (mat.HasProperty("_BaseColor")) { Color c = mat.GetColor("_BaseColor"); c.a = alpha; mat.SetColor("_BaseColor", c); }
            if (mat.HasProperty("_Color"))     { Color c = mat.GetColor("_Color");     c.a = alpha; mat.SetColor("_Color",     c); }
            r.material = mat;
        }
    }

    // -------------------------------------------------------------------------
    // Background plane
    // -------------------------------------------------------------------------

    private void CreateBackgroundPlane()
    {
        // Loaded from Assets/Resources/ so it ships with built players.
        // System.IO.File access doesn't work in WebGL and breaks for non-editor
        // platforms in general — Resources.Load is the portable path.
        Texture2D tex = Resources.Load<Texture2D>("background-image");
        if (tex == null) { Debug.LogWarning("[HexGridManager] background-image not found in Resources."); return; }

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "BackgroundPlane";
        plane.transform.SetParent(transform, false);
        plane.transform.position  = new Vector3(0f, -0.8f, 0f);
        plane.transform.rotation  = Quaternion.Euler(90f, 0f, 0f);
        plane.transform.localScale = new Vector3(50f, 50f, 1f);
        Destroy(plane.GetComponent<Collider>());

        Renderer r = plane.GetComponent<Renderer>();
        Shader sh  = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
        if (sh == null)
        {
            Debug.LogWarning("[HexGridManager] No shader found for background plane — skipping background.");
            Destroy(plane);
            return;
        }
        Material mat = new Material(sh);
        mat.mainTexture = tex;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
        r.material = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows    = false;
    }
}
