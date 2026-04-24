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

    private bool _usingFallback = false;
    public bool UsingFallbackTiles => _usingFallback;

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
                    _usingFallback = true;
                }
            }
        }

        List<Tile> yurtCandidates = new List<Tile>();
        foreach (Tile t in Grid.Values)
            if (t.Terrain == TerrainType.Plain || t.Terrain == TerrainType.Pasture)
                yurtCandidates.Add(t);

        for (int i = 0; i < 3 && yurtCandidates.Count > 0; i++)
        {
            int idx = Random.Range(0, yurtCandidates.Count);
            yurtCandidates[idx].SetHasYurt(true);
            yurtCandidates.RemoveAt(idx);
        }

        PlaceDecorations();

        Debug.Log($"[HexGridManager] Grid generated: {Grid.Count} tiles. Fallback mode: {_usingFallback}");
        OnGridReady?.Invoke();
    }

    private void PlaceDecorations()
    {
        foreach (Tile tile in Grid.Values)
        {
            // Skip yurt tiles and tiles that already have occupants
            if (tile.HasYurt) continue;

            GameObject[] pool = null;
            float chance = 0f;

            switch (tile.Terrain)
            {
                case TerrainType.Plain:
                case TerrainType.Pasture:
                    pool = treePrefabs;
                    chance = 0.25f;
                    break;
                case TerrainType.Mountain:
                    pool = rockPrefabs;
                    chance = 0.55f;
                    break;
                case TerrainType.Water:
                    continue; // no decorations on water
            }

            if (pool == null || pool.Length == 0) continue;
            if (Random.value > chance) continue;

            GameObject prefab = pool[Random.Range(0, pool.Length)];
            GameObject deco = Instantiate(prefab, tile.transform);
            deco.transform.localPosition = new Vector3(
                Random.Range(-0.2f, 0.2f), 0f, Random.Range(-0.2f, 0.2f));
            deco.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            // Remove any colliders on decorations so they don't block tile clicks
            foreach (Collider c in deco.GetComponentsInChildren<Collider>())
                Destroy(c);
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
        col.radius = hexSize * 0.48f;
        col.height = 0.25f;
        col.center = Vector3.zero;

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
        float x = hexSize * Mathf.Sqrt(3f) * (hexCoords.x + hexCoords.y / 2f);
        float z = hexSize * 1.5f * hexCoords.y;
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

        if (dist <= 1)
            return roll < 0.70f ? TerrainType.Plain : TerrainType.Pasture;

        if (dist == 2)
        {
            if (roll < 0.60f) return TerrainType.Plain;
            if (roll < 0.80f) return TerrainType.Pasture;
            return TerrainType.Mountain;
        }

        if (roll < 0.50f) return TerrainType.Plain;
        if (roll < 0.70f) return TerrainType.Mountain;
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
