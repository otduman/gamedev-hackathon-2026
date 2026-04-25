using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnimator : MonoBehaviour
{
    private static readonly Color WaterDeep  = new Color(0.02f, 0.12f, 0.50f, 1f);
    private static readonly Color WaterLight = new Color(0.20f, 0.55f, 0.92f, 1f);

    private const float OscillationSpeed = 1.4f;
    private const float BobAmplitude     = 0.07f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");

    private struct WaterTileData
    {
        public Renderer              Rend;
        public float                 PhaseOffset;
        public float                 BaseY;
        public Transform             TileTransform;
        public MaterialPropertyBlock PropBlock;
    }

    private WaterTileData[] _tiles;

    private void Start()
    {
        if (HexGridManager.Instance != null && HexGridManager.Instance.Grid.Count > 0)
            CollectWaterTiles();
        else if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady += CollectWaterTiles;
        else
            StartCoroutine(WaitAndCollect());
    }

    private IEnumerator WaitAndCollect()
    {
        yield return null;
        CollectWaterTiles();
    }

    private void OnDestroy()
    {
        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady -= CollectWaterTiles;
    }

    private void CollectWaterTiles()
    {
        if (HexGridManager.Instance != null)
            HexGridManager.Instance.OnGridReady -= CollectWaterTiles;

        var tempList = new List<WaterTileData>();

        foreach (var kvp in HexGridManager.Instance.Grid)
        {
            Tile tile = kvp.Value;
            if (tile == null || tile.Terrain != TerrainType.Water) continue;

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
    }

    private void Update()
    {
        if (_tiles == null) return;

        float t = Time.time;

        for (int i = 0; i < _tiles.Length; i++)
        {
            ref WaterTileData data = ref _tiles[i];
            if (data.Rend == null) continue;

            float lerpFactor = Mathf.Sin(t * OscillationSpeed + data.PhaseOffset) * 0.5f + 0.5f;
            Color animColor  = Color.Lerp(WaterDeep, WaterLight, lerpFactor);

            data.Rend.GetPropertyBlock(data.PropBlock);
            data.PropBlock.SetColor(BaseColorId, animColor);
            data.PropBlock.SetColor(ColorId,     animColor);
            data.Rend.SetPropertyBlock(data.PropBlock);

            Vector3 pos = data.TileTransform.position;
            pos.y = data.BaseY + Mathf.Sin(t * OscillationSpeed + data.PhaseOffset) * BobAmplitude;
            data.TileTransform.position = pos;
        }
    }
}
