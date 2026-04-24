using UnityEngine;
using TMPro;

public enum TerrainType { Plain, Mountain, Water, Pasture }

public class Tile : MonoBehaviour
{
    public Vector2Int HexCoordinates { get; private set; }
    public TerrainType Terrain { get; private set; }
    
    // Core state
    public int OwnerPlayerId { get; set; } = -1; // -1 means neutral
    public int CubeCount { get; set; } = 0;
    public bool HasYurt { get; set; }

    // Visuals
    [Header("Asset Models")]
    [SerializeField] private GameObject yurtPrefab; // The low-poly yurt
    private GameObject activeYurtInstance;
    
    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private TextMeshPro cubeCountText;

    // Colors for different players
    [SerializeField] private Color neutralColor = Color.gray;
    [SerializeField] private Color player1Color = Color.blue;
    [SerializeField] private Color player2Color = Color.red;

    public void Initialize(Vector2Int coords, TerrainType terrain)
    {
        HexCoordinates = coords;
        Terrain = terrain;
        UpdateVisuals();
    }

    public void AddCubes(int amount)
    {
        CubeCount += amount;
        UpdateVisuals();
    }

    public void RemoveCubes(int amount)
    {
        CubeCount -= amount;
        if (CubeCount < 0) CubeCount = 0;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (meshRenderer != null)
        {
            // Note: In a real project, consider using MaterialPropertyBlocks for performance instead of material instances
            if (OwnerPlayerId == 0) meshRenderer.material.color = player1Color;
            else if (OwnerPlayerId == 1) meshRenderer.material.color = player2Color;
            else meshRenderer.material.color = neutralColor;
        }

        if (cubeCountText != null)
        {
            if (CubeCount > 0)
            {
                cubeCountText.text = CubeCount.ToString();
                cubeCountText.gameObject.SetActive(true);
            }
            else
            {
                cubeCountText.gameObject.SetActive(false);
            }
        }

        UpdateYurtVisuals();
    }

    private void UpdateYurtVisuals()
    {
        if (HasYurt && activeYurtInstance == null && yurtPrefab != null)
        {
            // Spawn the yurt slightly above the tile center
            activeYurtInstance = Instantiate(yurtPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, this.transform);
        }
        else if (!HasYurt && activeYurtInstance != null)
        {
            Destroy(activeYurtInstance);
        }
    }
}
