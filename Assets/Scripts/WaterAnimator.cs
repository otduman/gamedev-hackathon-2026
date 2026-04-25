using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animates all water tiles on the hex grid with a gentle shimmering color pulse
/// and a subtle vertical bob, simulating calm water.
///
/// Added automatically by HexGridManager after grid generation — no Inspector
/// configuration required.
///
/// Design notes:
///   - Uses MaterialPropertyBlock per renderer to avoid breaking GPU batching.
///   - Calls GetPropertyBlock before every write so it never stomps tint values
///     applied by Tile.ApplyTint (owner-based coloring, selection highlight, etc.).
///   - Each tile gets a random phase offset so waves are staggered across the map.
///   - Compatible with both URP (_BaseColor) and Built-in pipeline (_Color).
/// </summary>
public class WaterAnimator : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Animation constants
    // -------------------------------------------------------------------------
    private static readonly Color WaterDeep  = new Color(0.02f, 0.12f, 0.50f, 1f);
    private static readonly Color WaterLight = new Color(0.20f, 0.55f, 0.92f, 1f);

    private const float OscillationSpeed = 1.4f;   // radians / second
    private const float BobAmplitude     = 0.07f;  // units ±

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");

    // -------------------------------------------------------------------------
    // Per-tile runtime data
    // -------------------------------------------------------------------------
    private struct WaterTileData
    {
        public Renderer          Rend;
        public float             PhaseOffset;   // 0 … 2π
        public float             BaseY;         // world-space Y at spawn time
        public Transform         TileTransform;
        public MaterialPropertyBlock PropBlock;
    }

    private WaterTileData[] _tiles;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    private void Start()
    {
        // HexGridManager.GenerateHexagonalMap() runs in its own Start().
        // Execution order between two Start() calls on different objects is
        // not guaranteed, so subscribing to OnGridReady is the safest hook.
        // If the grid is already populated by the time we arrive here
        // (e.g. HexGridManager.Start ran first), OnGridReady will never fire
        // again — so we check the grid size as a fallback.

        if (HexGridManager.Instance != null && HexGridManager.Instance.Grid.Count > 0)
        {
            // Grid is already ready.
            CollectWaterTiles();
        }
        else if (HexGridManager.Instance != null)
        {
            // Grid not ready yet — wait for the event.
            HexGridManager.Instance.OnGridReady += CollectWaterTiles;
        }
        else
        {
            // HexGridManager not available; wait one frame and try again.
            StartCoroutine(WaitAndCollect());
        }
    }

    private IEnumerator WaitAndCollect()
    {
        yield return null; // skip one frame
        CollectWaterTiles();
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid dangling delegate if this component is destroyed
        // before HexGridManager fires the event.
        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady -= CollectWaterTiles;
    }

    // -------------------------------------------------------------------------
    // Collection
    // -------------------------------------------------------------------------
    private void CollectWaterTiles()
    {
        // Unsubscribe immediately — we only need to collect once.
        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady -= CollectWaterTiles;

        var tempList = new List<WaterTileData>();

        foreach (var kvp in HexGridManager.Instance.Grid)
        {
            Tile tile = kvp.Value;
            if (tile == null || tile.Terrain != TerrainType.Water) continue;

            // GetComponentInChildren also finds a Renderer on the root itself,
            // and works for both KayKit prefab meshes and fallback cylinders.
            Renderer rend = tile.GetComponentInChildren<Renderer>();
            if (rend == null) continue;

            tempList.Add(new WaterTileData
            {
                Rend          = rend,
                PhaseOffset   = Random.Range(0f, Mathf.PI * 2f),
                BaseY         = tile.transform.position.y,
                TileTransform = tile.transform,
                PropBlock     = new MaterialPropertyBlock(),
            });
        }

        _tiles = tempList.ToArray();

        Debug.Log($"[WaterAnimator] Tracking {_tiles.Length} water tile(s).");
    }

    // -------------------------------------------------------------------------
    // Animation
    // -------------------------------------------------------------------------
    private void Update()
    {
        if (_tiles == null) return;

        float t = Time.time;

        for (int i = 0; i < _tiles.Length; i++)
        {
            ref WaterTileData data = ref _tiles[i];

            if (data.Rend == null) continue; // tile was destroyed at runtime

            // --- Color shimmer --------------------------------------------------
            // sin oscillates −1 … +1; remap to 0 … 1 for a smooth lerp factor.
            float sinVal    = Mathf.Sin(t * OscillationSpeed + data.PhaseOffset);
            float lerpFactor = sinVal * 0.5f + 0.5f;
            Color animColor = Color.Lerp(WaterDeep, WaterLight, lerpFactor);

            // Read the current block so we don't overwrite anything Tile has set
            // (selection highlight, owner tint, etc.).  We just patch the color.
            data.Rend.GetPropertyBlock(data.PropBlock);
            data.PropBlock.SetColor(BaseColorId, animColor); // URP
            data.PropBlock.SetColor(ColorId,     animColor); // Built-in fallback
            data.Rend.SetPropertyBlock(data.PropBlock);

            // --- Vertical bob ---------------------------------------------------
            Vector3 pos = data.TileTransform.position;
            pos.y = data.BaseY + Mathf.Sin(t * OscillationSpeed + data.PhaseOffset) * BobAmplitude;
            data.TileTransform.position = pos;
        }
    }
}
