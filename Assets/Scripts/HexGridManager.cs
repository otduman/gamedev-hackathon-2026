using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public static HexGridManager Instance { get; private set; }

    [Header("KayKit Tile Prefabs")]
    [SerializeField] private GameObject[] plainPrefabs;
    [SerializeField] private GameObject[] mountainPrefabs;
    [SerializeField] private GameObject[] waterPrefabs;
    [SerializeField] private GameObject[] pasturePrefabs;

    [Header("Grid Settings")]
    [SerializeField] private int mapRadius = 3;
    [SerializeField] private float hexSize = 1f;

    public Dictionary<Vector2Int, Tile> Grid { get; private set; } = new Dictionary<Vector2Int, Tile>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
            int r2 = Mathf.Min(mapRadius, -q + mapRadius);
            
            for (int r = r1; r <= r2; r++)
            {
                Vector2Int hexCoords = new Vector2Int(q, r);
                Vector3 worldPos = CalculateHexPosition(hexCoords);

                TerrainType terrain = DetermineTerrain(q, r);
                GameObject selectedPrefab = GetPrefabForTerrain(terrain);

                if (selectedPrefab == null)
                {
                    Debug.LogError($"No prefab assigned for terrain {terrain}!");
                    continue;
                }

                GameObject tileGo = Instantiate(selectedPrefab, worldPos, selectedPrefab.transform.rotation, this.transform);
                tileGo.name = $"Tile_{q}_{r}";
                
                // Add a random rotation so the grass hexes look "different" and natural
                tileGo.transform.Rotate(0, Random.Range(0, 6) * 60f, 0, Space.World);
                
                Tile tile = tileGo.GetComponent<Tile>();
                tile.Initialize(hexCoords, terrain); 
                
                Grid.Add(hexCoords, tile);
            }
        }

        // Place exactly 3 Yurts on random grass hexes
        List<Tile> allTiles = new List<Tile>(Grid.Values);
        for (int i = 0; i < 3; i++)
        {
            if (allTiles.Count > 0)
            {
                int randomIndex = Random.Range(0, allTiles.Count);
                Tile randomTile = allTiles[randomIndex];
                randomTile.HasYurt = true; // This will trigger the yurt visuals to spawn
                allTiles.RemoveAt(randomIndex); // Prevent placing 2 yurts on same tile
            }
        }
    }

    private Vector3 CalculateHexPosition(Vector2Int hexCoords)
    {
        // KayKit hexes might need slightly different math depending on their pivot, 
        // but standard pointy-top math applies. Adjust hexSize if there are gaps.
        float x = hexSize * Mathf.Sqrt(3) * (hexCoords.x + hexCoords.y / 2f);
        float z = hexSize * 3f / 2f * hexCoords.y;
        return new Vector3(x, 0, z); // Y is up, so hexes lay flat on X/Z
    }

    private TerrainType DetermineTerrain(int q, int r)
    {
        // For now, return only grass (Plain) to fulfill the 20-25 grass hexes request
        return TerrainType.Plain;
    }

    private GameObject GetPrefabForTerrain(TerrainType terrain)
    {
        GameObject[] options = null;
        switch (terrain)
        {
            case TerrainType.Plain: options = plainPrefabs; break;
            case TerrainType.Mountain: options = mountainPrefabs; break;
            case TerrainType.Water: options = waterPrefabs; break;
            case TerrainType.Pasture: options = pasturePrefabs; break;
        }

        if (options != null && options.Length > 0)
        {
            return options[Random.Range(0, options.Length)];
        }
        return null;
    }

    public List<Tile> GetNeighbors(Vector2Int hexCoords)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborCoords = hexCoords + dir;
            if (Grid.TryGetValue(neighborCoords, out Tile neighborTile))
            {
                neighbors.Add(neighborTile);
            }
        }
        return neighbors;
    }
}
