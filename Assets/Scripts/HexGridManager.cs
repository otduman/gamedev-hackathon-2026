using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public static HexGridManager Instance { get; private set; }
    public System.Action OnGridReady;

    [Header("KayKit Tile Prefabs")]
    [SerializeField] private GameObject[] plainPrefabs;
    [SerializeField] private GameObject[] mountainPrefabs;
    [SerializeField] private GameObject[] waterPrefabs;
    [SerializeField] private GameObject[] pasturePrefabs;

    [Header("Decoration Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;

    [Header("Grid Settings")]
    [SerializeField] private int mapRadius = 3;
    [SerializeField] private float hexSize = 1.0f;

    public Dictionary<Vector2Int, Tile> Grid { get; private set; } = new Dictionary<Vector2Int, Tile>();

    private static readonly Vector3[] DecoSpreadOffsets = new Vector3[]
    {
        new Vector3( 0.00f, 0f,  0.00f),
        new Vector3( 0.30f, 0f,  0.15f),
        new Vector3(-0.30f, 0f,  0.15f),
        new Vector3( 0.15f, 0f, -0.30f),
        new Vector3(-0.15f, 0f, -0.30f),
    };

    private static readonly Color FallbackColorPlain    = new Color(0.20f, 0.50f, 0.20f);
    private static readonly Color FallbackColorMountain = new Color(0.50f, 0.50f, 0.50f);
    private static readonly Color FallbackColorWater    = new Color(0.10f, 0.30f, 0.70f);
    private static readonly Color FallbackColorPasture  = new Color(0.40f, 0.70f, 0.20f);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        GenerateHexagonalMap();
    }

    private void GenerateHexagonalMap()
    {
        for (int q = -mapRadius; q <= mapRadius; q++)
        {
            int r1 = Mathf.Max(-mapRadius, -q - mapRadius);
            int r2 = Mathf.Min(mapRadius,  -q + mapRadius);

            for (int r = r1; r <= r2; r++)
            {
                Vector2Int hexCoords = new Vector2Int(q, r);
                Vector3 worldPos     = CalculateHexPosition(hexCoords);
                TerrainType terrain  = DetermineTerrain(q, r);

                GameObject selectedPrefab = GetPrefabForTerrain(terrain);
                GameObject tileGo;

                if (selectedPrefab != null)
                {
                    tileGo = Instantiate(selectedPrefab, worldPos, selectedPrefab.transform.rotation, transform);
                    tileGo.name = $"Tile_{q}_{r}";
                    tileGo.transform.Rotate(0, Random.Range(0, 6) * 60f, 0, Space.World);
                    // Compress mountain tiles so their tall mesh doesn't bleed into adjacent hexes.
                    if (terrain == TerrainType.Mountain)
                        tileGo.transform.localScale = new Vector3(1f, 0.62f, 1f);

                    Tile tile = tileGo.GetComponent<Tile>();
                    if (tile == null)
                    {
                        Debug.LogError($"[HexGridManager] Prefab '{selectedPrefab.name}' is missing the Tile component — skipping ({q},{r}).");
                        Destroy(tileGo);
                        continue;
                    }

                    tile.Initialize(hexCoords, terrain);
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
        AddHexBorders();
        CreateBackgroundPlane();

        OnGridReady?.Invoke();
        if (GetComponent<AmbientEnvironment>() == null)
            gameObject.AddComponent<AmbientEnvironment>();
        if (GetComponent<WaterAnimator>() == null)
            gameObject.AddComponent<WaterAnimator>();
        if (GetComponent<AudioManager>() == null)
            gameObject.AddComponent<AudioManager>();
    }

    private void PlaceDecorations()
    {
        foreach (Tile tile in Grid.Values)
        {
            if (tile.HasYurt) continue;

            GameObject[] primaryPool   = null;
            GameObject[] secondaryPool = null;
            int maxCount = 1;
            float spawnChance = 0f;

            switch (tile.Terrain)
            {
                case TerrainType.Plain:
                    primaryPool   = treePrefabs;
                    secondaryPool = rockPrefabs;
                    maxCount      = 1;
                    spawnChance   = 0.30f;
                    break;
                case TerrainType.Pasture:
                    primaryPool   = treePrefabs;
                    secondaryPool = rockPrefabs;
                    maxCount      = 2;
                    spawnChance   = 0.40f;
                    break;
                case TerrainType.Mountain:
                    primaryPool   = rockPrefabs;
                    secondaryPool = rockPrefabs;
                    maxCount      = 1;
                    spawnChance   = 0.45f;
                    break;
                case TerrainType.Water:
                    continue;
            }

            if (Random.value > spawnChance) continue;

            // Pick how many decorations (1..maxCount)
            int count = Random.Range(1, maxCount + 1);

            // Shuffle offsets so each run gets a different spread
            List<Vector3> offsets = new List<Vector3>(DecoSpreadOffsets);
            for (int i = offsets.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Vector3 tmp = offsets[i]; offsets[i] = offsets[j]; offsets[j] = tmp;
            }

            // Pick one pool for the entire tile so trees and rocks never mix on the same hex.
            bool usePrimary = secondaryPool == null || secondaryPool.Length == 0 || Random.value < 0.6f;
            GameObject[] tilePool = usePrimary ? primaryPool : secondaryPool;
            if (tilePool == null || tilePool.Length == 0) continue;

            // Mark tile as forested once, before spawning individual decorations.
            bool isTileForest = treePrefabs != null && treePrefabs.Length > 0 && tilePool == treePrefabs
                                && (tile.Terrain == TerrainType.Plain || tile.Terrain == TerrainType.Pasture);
            if (isTileForest) tile.SetHasForest(true);

            for (int i = 0; i < count; i++)
            {
                GameObject[] pool = tilePool;

                if (pool == null || pool.Length == 0) continue;

                GameObject prefab = pool[Random.Range(0, pool.Length)];
                Vector3 baseOffset = offsets[i % offsets.Count];
                Vector3 jitter = new Vector3(Random.Range(-0.08f, 0.08f), 0f, Random.Range(-0.08f, 0.08f));

                GameObject deco = Instantiate(prefab, tile.transform);
                deco.transform.localPosition = baseOffset + jitter;
                deco.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                deco.transform.localScale    = Vector3.one * Random.Range(0.85f, 1.15f);

                foreach (Collider c in deco.GetComponentsInChildren<Collider>())
                    Destroy(c);

                MakeDecoTransparent(deco, 0.52f);

                foreach (Renderer r in deco.GetComponentsInChildren<Renderer>())
                    tile.RegisterDecoRenderer(r);
            }
        }
    }

    private void CreateBackgroundPlane()
    {
        string path = Application.dataPath + "/background-image.jpg";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning("[HexGridManager] background-image.jpg not found in Assets/");
            return;
        }

        byte[] bytes = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
        {
            Debug.LogWarning("[HexGridManager] Failed to decode background-image.jpg");
            return;
        }

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "BackgroundPlane";
        plane.transform.SetParent(transform, false);
        plane.transform.position = new Vector3(0f, -0.8f, 0f);
        plane.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // lie flat
        plane.transform.localScale = new Vector3(40f, 40f, 1f);

        Destroy(plane.GetComponent<Collider>());

        Renderer r = plane.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Texture")
                     ?? Shader.Find("Standard");
        Material mat = new Material(shader);
        mat.mainTexture = tex;
        if (mat.HasProperty("_BaseMap"))  mat.SetTexture("_BaseMap",  tex);
        if (mat.HasProperty("_MainTex"))  mat.SetTexture("_MainTex",  tex);
        r.material = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

    }

    private void AddHexBorders()
    {
        // Pointy-top hex corners at 30°, 90°, 150° … (0° offset + 30° shift for KayKit tiles).
        // Radius slightly inside hexSize so the border sits within the tile edge.
        const float borderRadius = 0.97f;
        const float lineWidth    = 0.012f;
        const float yOffset      = 0.06f;

        Color borderColor = new Color(0.08f, 0.05f, 0.02f, 0.18f);

        Shader urpUnlit     = Shader.Find("Universal Render Pipeline/Unlit");
        Shader builtinUnlit = Shader.Find("Unlit/Color");
        Shader chosen       = urpUnlit ?? builtinUnlit;

        foreach (Tile tile in Grid.Values)
        {
            GameObject borderGo = new GameObject("HexBorder");
            borderGo.transform.SetParent(tile.transform, false);
            borderGo.transform.localPosition = new Vector3(0f, yOffset, 0f);

            LineRenderer lr = borderGo.AddComponent<LineRenderer>();
            lr.loop             = true;
            lr.positionCount    = 6;
            lr.useWorldSpace    = false;
            lr.startWidth       = lineWidth;
            lr.endWidth         = lineWidth;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows   = false;

            float r = hexSize * borderRadius;
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i + 30f); // 30° offset: pointy-top to match KayKit tiles
                lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r));
            }

            if (chosen != null)
            {
                Material mat = new Material(chosen);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", borderColor);
                if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     borderColor);
                lr.material = mat;
            }
            lr.startColor = lr.endColor = borderColor;
        }
    }

    private static void MakeDecoTransparent(GameObject go, float alpha)
    {
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            Material mat = new Material(r.sharedMaterial);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
            if (mat.HasProperty("_ZWrite"))  mat.SetFloat("_ZWrite", 0);
            mat.renderQueue = 3000;
            if (mat.HasProperty("_BaseColor")) { Color c = mat.GetColor("_BaseColor"); c.a = alpha; mat.SetColor("_BaseColor", c); }
            if (mat.HasProperty("_Color"))     { Color c = mat.GetColor("_Color");     c.a = alpha; mat.SetColor("_Color", c); }
            r.material = mat;
        }
    }

    private GameObject CreateFallbackTile(Vector3 worldPos, TerrainType terrain)
    {
        GameObject root = new GameObject("FallbackTile");
        root.transform.position = worldPos;
        root.transform.SetParent(transform, true);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(root.transform, false);
        cylinder.transform.localPosition = Vector3.zero;
        // Scale to a flat disc. X/Z = 95% of hexSize so tiles have a small gap between them.
        float discRadius = hexSize * 0.95f;
        cylinder.transform.localScale = new Vector3(discRadius, 0.12f, discRadius);

        Collider primitiveCollider = cylinder.GetComponent<Collider>();
        if (primitiveCollider != null) Destroy(primitiveCollider);

        Renderer rend = cylinder.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.sharedMaterial);
            Color terrainColor = TerrainToFallbackColor(terrain);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", terrainColor);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     terrainColor);
            rend.material = mat;
        }

        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.radius = hexSize * 0.9f;
        col.height = 0.6f;
        col.center = new Vector3(0f, 0.15f, 0f);

        Tile tile = root.AddComponent<Tile>();
        tile.SetPrimitiveMeshRenderer(rend);
        return root;
    }

    private Color TerrainToFallbackColor(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Plain:    return FallbackColorPlain;
            case TerrainType.Mountain: return FallbackColorMountain;
            case TerrainType.Water:    return FallbackColorWater;
            case TerrainType.Pasture:  return FallbackColorPasture;
            default:                   return FallbackColorPlain;
        }
    }

    private Vector3 CalculateHexPosition(Vector2Int hexCoords)
    {
        // Multiplier > 1.0 spreads tiles apart from the natural touching distance.
        // At 1.0 flat-top hex edges meet exactly; 1.06 gives ~5% visible gap.
        const float spread = 1.06f;
        float x = hexSize * spread * Mathf.Sqrt(3f) * (hexCoords.x + hexCoords.y / 2f);
        float z = hexSize * spread * 1.5f * hexCoords.y;
        return new Vector3(x, 0f, z);
    }

    private int HexDistanceFromOrigin(int q, int r)
    {
        return Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(-q - r));
    }

    private TerrainType DetermineTerrain(int q, int r)
    {
        int   dist = HexDistanceFromOrigin(q, r);
        float roll = Random.value;

        // dist 0-2 are always passable so every tile on the map is reachable
        // (outer ring tiles always border at least one inner-ring passable tile)
        if (dist <= 1)
            return roll < 0.65f ? TerrainType.Plain : TerrainType.Pasture;

        if (dist == 2)
            return roll < 0.55f ? TerrainType.Plain : TerrainType.Pasture;

        // Outer ring — allow Mountains and Water for variety
        if (roll < 0.45f) return TerrainType.Plain;
        if (roll < 0.65f) return TerrainType.Pasture;
        if (roll < 0.82f) return TerrainType.Mountain;
        return TerrainType.Water;
    }

    private GameObject GetPrefabForTerrain(TerrainType terrain)
    {
        GameObject[] options = null;
        switch (terrain)
        {
            case TerrainType.Plain:    options = plainPrefabs;    break;
            case TerrainType.Mountain: options = mountainPrefabs; break;
            case TerrainType.Water:    options = waterPrefabs;    break;
            case TerrainType.Pasture:  options = pasturePrefabs;  break;
        }

        if (options == null || options.Length == 0) options = plainPrefabs;
        if (options != null && options.Length > 0)
            return options[Random.Range(0, options.Length)];
        return null;
    }

    public List<Tile> GetNeighbors(Vector2Int hexCoords)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int( 1,  0), new Vector2Int( 1, -1), new Vector2Int( 0, -1),
            new Vector2Int(-1,  0), new Vector2Int(-1,  1), new Vector2Int( 0,  1)
        };
        foreach (Vector2Int dir in directions)
        {
            if (Grid.TryGetValue(hexCoords + dir, out Tile neighborTile))
                neighbors.Add(neighborTile);
        }
        return neighbors;
    }
}
