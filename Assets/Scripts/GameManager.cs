using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Reinforce, Action, MigrationCheck, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState    { get; private set; } = GameState.Reinforce;
    public int       CurrentPlayerId { get; private set; } = 0;
    public int[]     PlayerPrestige  = new int[2];

    private static readonly Color _attackHighlight   = new Color(1.00f, 0.20f, 0.20f, 1f);
    private static readonly Color _migrationHighlight = new Color(0.20f, 0.90f, 0.40f, 1f);

    private Tile _selectedTile    = null;
    private int  _moveCount       = 0;
    private int  _reinforceLeft   = 0;
    private bool _migrationUsed   = false;

    private List<Tile> _highlightedTiles = new List<Tile>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        QualitySettings.anisotropicFiltering     = AnisotropicFiltering.ForceEnable;
        QualitySettings.antiAliasing             = 4;
        QualitySettings.globalTextureMipmapLimit = 0;

        HexGridManager hexGrid = GetComponent<HexGridManager>() ?? FindAnyObjectByType<HexGridManager>();
        if (hexGrid != null) hexGrid.OnGridReady += InitializeGame;
        else Debug.LogError("GameManager: could not find HexGridManager.");
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (_selectedTile != null && CurrentState == GameState.MigrationCheck)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                int dir = scroll > 0 ? 1 : -1;
                int max = Mathf.Max(1, _selectedTile.CubeCount / 2);
                _moveCount = Mathf.Clamp(_moveCount + dir, 1, max);
                ShowMigrationSelectionMessage();
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick();
    }

    private void InitializeGame()
    {
        Dictionary<Vector2Int, Tile> grid = HexGridManager.Instance.Grid;

        List<Tile> westTiles = new List<Tile>();
        List<Tile> eastTiles = new List<Tile>();

        foreach (KeyValuePair<Vector2Int, Tile> entry in grid)
        {
            int q = entry.Key.x;
            if (q <= -2) westTiles.Add(entry.Value);
            else if (q >= 2) eastTiles.Add(entry.Value);
        }

        AssignStartingTiles(0, westTiles, 5);
        AssignStartingTiles(1, eastTiles, 5);

        RecalculatePrestige();
        StartReinforce();
    }

    private void AssignStartingTiles(int playerId, List<Tile> candidates, int count)
    {
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Tile tmp = candidates[i]; candidates[i] = candidates[j]; candidates[j] = tmp;
        }

        int assigned = 0;
        foreach (Tile tile in candidates)
        {
            if (assigned >= count) break;
            if (tile.Terrain == TerrainType.Water || tile.Terrain == TerrainType.Mountain) continue;
            tile.OwnerPlayerId = playerId;
            tile.AddCubes(Random.Range(2, 5));
            tile.UpdateVisuals();
            assigned++;
        }

        if (assigned < count)
            Debug.LogWarning($"GameManager: only {assigned} starting tiles for Player {playerId} (wanted {count}).");
    }

    private void HandleClick()
    {
        if (CurrentState == GameState.GameOver) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Tile clicked = hit.collider.GetComponentInParent<Tile>();
        if (clicked == null) return;

        switch (CurrentState)
        {
            case GameState.Reinforce:      HandleReinforceClick(clicked);  break;
            case GameState.Action:         HandleActionClick(clicked);     break;
            case GameState.MigrationCheck: HandleMigrationClick(clicked);  break;
        }
    }

    // -------------------------------------------------------------------------
    // REINFORCE
    // -------------------------------------------------------------------------

    private void StartReinforce()
    {
        CurrentState    = GameState.Reinforce;
        _reinforceLeft  = CalculateReinforcements(CurrentPlayerId);
        ClearSelection();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                $"REINFORCE: Place {_reinforceLeft} warrior(s) on your tiles. Click a tile to place one.");
        }
    }

    private int CalculateReinforcements(int playerId)
    {
        int ownedCount = 0;
        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
            if (tile.OwnerPlayerId == playerId) ownedCount++;

        return Mathf.Max(3, ownedCount / 3);
    }

    private void HandleReinforceClick(Tile clicked)
    {
        if (clicked.OwnerPlayerId != CurrentPlayerId) return;
        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

        clicked.AddCubes(1);
        _reinforceLeft--;

        if (UIManager.Instance != null)
        {
            if (_reinforceLeft > 0)
                UIManager.Instance.ShowMessage($"REINFORCE: {_reinforceLeft} warrior(s) left to place.");
            else
                UIManager.Instance.ShowMessage("All warriors placed. Moving to Action phase...");
        }

        if (_reinforceLeft <= 0)
            StartAction();
    }

    // -------------------------------------------------------------------------
    // ACTION
    // -------------------------------------------------------------------------

    private void StartAction()
    {
        CurrentState = GameState.Action;
        ClearSelection();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "ACTION: Select one of your tiles with 2+ warriors to attack. Press End Turn when done.");
        }
    }

    private void HandleActionClick(Tile clicked)
    {
        if (_selectedTile == null)
        {
            if (clicked.OwnerPlayerId != CurrentPlayerId) return;
            if (clicked.CubeCount < 2) return;
            if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

            _selectedTile = clicked;
            _selectedTile.SetSelected(true);
            HighlightAttackTargets(_selectedTile);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    $"Selected tile with {clicked.CubeCount} warriors. Click a red enemy tile to attack.");
            return;
        }

        if (clicked == _selectedTile)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    "ACTION: Select one of your tiles with 2+ warriors to attack. Press End Turn when done.");
            return;
        }

        if (clicked.OwnerPlayerId == CurrentPlayerId)
        {
            ClearSelection();
            if (clicked.CubeCount >= 2 &&
                clicked.Terrain != TerrainType.Water &&
                clicked.Terrain != TerrainType.Mountain)
            {
                _selectedTile = clicked;
                _selectedTile.SetSelected(true);
                HighlightAttackTargets(_selectedTile);
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage(
                        $"Selected tile with {clicked.CubeCount} warriors. Click a red enemy tile to attack.");
            }
            else
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage(
                        "ACTION: Select one of your tiles with 2+ warriors to attack. Press End Turn when done.");
            }
            return;
        }

        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Cannot attack water or mountain tiles.");
            return;
        }

        List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(_selectedTile.HexCoordinates);
        if (!neighbors.Contains(clicked))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Target must be adjacent to the selected tile.");
            return;
        }

        ExecuteAttack(_selectedTile, clicked);
    }

    private void ExecuteAttack(Tile attacker, Tile defender)
    {
        int attackingDice = Mathf.Min(3, attacker.CubeCount - 1);

        attacker.PlayAttackAnimation();
        defender.PlayAttackAnimation();

        bool conquered = CombatManager.Instance.ResolveCombat(attacker, defender, attackingDice);

        attacker.UpdateVisuals();
        defender.UpdateVisuals();

        string result;
        if (conquered)
            result = $"Victory! You conquered the tile.\nAttacker used {attackingDice} dice.";
        else
            result = $"Battle ongoing — tile held.\nAttacker used {attackingDice} dice.";

        if (UIManager.Instance != null)
            UIManager.Instance.ShowCombatResult(result);

        RecalculatePrestige();

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();

        if (CheckWinCondition()) return;

        ClearSelection();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage(
                "ACTION: Select another tile to attack, or press End Turn.");
    }

    private void HighlightAttackTargets(Tile source)
    {
        ClearHighlights();
        List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(source.HexCoordinates);
        foreach (Tile neighbor in neighbors)
        {
            if (neighbor.OwnerPlayerId == CurrentPlayerId) continue;
            if (neighbor.Terrain == TerrainType.Water || neighbor.Terrain == TerrainType.Mountain) continue;
            neighbor.SetHighlighted(true, _attackHighlight);
            _highlightedTiles.Add(neighbor);
        }
    }

    // -------------------------------------------------------------------------
    // MIGRATION
    // -------------------------------------------------------------------------

    private void StartMigration()
    {
        CurrentState    = GameState.MigrationCheck;
        _migrationUsed  = false;
        ClearSelection();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "MIGRATION: Move warriors from one tile to an adjacent own tile, or press End Turn to skip.");
        }
    }

    private void HandleMigrationClick(Tile clicked)
    {
        if (_migrationUsed)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Migration already used this turn. Press End Turn.");
            return;
        }

        if (_selectedTile == null)
        {
            if (clicked.OwnerPlayerId != CurrentPlayerId) return;
            if (clicked.CubeCount < 2) return;
            if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

            _selectedTile = clicked;
            _selectedTile.SetSelected(true);
            _moveCount = Mathf.Max(1, clicked.CubeCount / 2);
            HighlightMigrationTargets(_selectedTile);
            ShowMigrationSelectionMessage();
            return;
        }

        if (clicked == _selectedTile)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    "MIGRATION: Move warriors from one tile to an adjacent own tile, or press End Turn to skip.");
            return;
        }

        if (clicked.OwnerPlayerId != CurrentPlayerId)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Migration only moves to your own tiles. Select again.");
            return;
        }

        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Cannot migrate to water or mountain. Select again.");
            return;
        }

        List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(_selectedTile.HexCoordinates);
        if (!neighbors.Contains(clicked))
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Destination must be adjacent. Select again.");
            return;
        }

        int count = Mathf.Clamp(_moveCount, 1, _selectedTile.CubeCount - 1);
        _selectedTile.PlayWalkAnimation();
        _selectedTile.RemoveCubes(count);
        clicked.AddCubes(count);
        clicked.UpdateVisuals();

        _migrationUsed = true;
        ClearSelection();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage($"Moved {count} warrior(s). Press End Turn to finish.");
    }

    private void HighlightMigrationTargets(Tile source)
    {
        ClearHighlights();
        List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(source.HexCoordinates);
        foreach (Tile neighbor in neighbors)
        {
            if (neighbor.OwnerPlayerId != CurrentPlayerId) continue;
            if (neighbor.Terrain == TerrainType.Water || neighbor.Terrain == TerrainType.Mountain) continue;
            neighbor.SetHighlighted(true, _migrationHighlight);
            _highlightedTiles.Add(neighbor);
        }
    }

    private void ShowMigrationSelectionMessage()
    {
        if (UIManager.Instance == null || _selectedTile == null) return;
        int max = Mathf.Max(1, _selectedTile.CubeCount / 2);
        UIManager.Instance.ShowMessage(
            $"Moving {_moveCount} of {_selectedTile.CubeCount} warriors (max {max})  |  scroll to adjust  |  click adjacent own tile");
    }

    // -------------------------------------------------------------------------
    // TURN END / WIN
    // -------------------------------------------------------------------------

    public void OnEndTurnButtonClicked()
    {
        if (CurrentState == GameState.Action)
        {
            StartMigration();
            return;
        }

        if (CurrentState == GameState.MigrationCheck)
        {
            EndTurn();
            return;
        }
    }

    public void EndTurn()
    {
        ClearSelection();
        CurrentPlayerId = CurrentPlayerId == 0 ? 1 : 0;
        StartReinforce();
    }

    public void ChangeState(GameState s)
    {
        CurrentState = s;
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
    }

    // -------------------------------------------------------------------------
    // PRESTIGE / WIN CONDITION
    // -------------------------------------------------------------------------

    private void RecalculatePrestige()
    {
        for (int pid = 0; pid < 2; pid++)
        {
            int prestige = 0;
            foreach (Tile tile in HexGridManager.Instance.Grid.Values)
            {
                if (tile.OwnerPlayerId != pid) continue;
                prestige += 1;
                if (tile.HasYurt) prestige += 2;
            }
            PlayerPrestige[pid] = prestige;
        }
    }

    private bool CheckWinCondition()
    {
        for (int pid = 0; pid < 2; pid++)
        {
            int opponent = pid == 0 ? 1 : 0;

            bool opponentHasNoTiles = true;
            foreach (Tile tile in HexGridManager.Instance.Grid.Values)
            {
                if (tile.OwnerPlayerId == opponent) { opponentHasNoTiles = false; break; }
            }

            if (PlayerPrestige[pid] >= 15 || opponentHasNoTiles)
            {
                TriggerGameOver(pid);
                return true;
            }
        }
        return false;
    }

    private void TriggerGameOver(int winnerId)
    {
        CurrentState = GameState.GameOver;
        ClearSelection();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowGameOver(winnerId);
        }
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    private void ClearHighlights()
    {
        foreach (Tile tile in _highlightedTiles)
            if (tile != null) tile.ClearHighlight();
        _highlightedTiles.Clear();
    }

    private void ClearSelection()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            _selectedTile.UpdateVisuals();
            _selectedTile = null;
        }
        _moveCount = 0;
        ClearHighlights();
    }
}
