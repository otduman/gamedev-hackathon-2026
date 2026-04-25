using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Reinforce, Action, MigrationCheck, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState    { get; private set; } = GameState.Reinforce;
    public int       CurrentPlayerId { get; private set; } = 0;
    public int[]     PlayerPrestige  = new int[2];

    private static readonly string[] ClanNames = { "Argyn", "Naiman" };

    private static readonly Color _attackHighlight    = new Color(1.00f, 0.20f, 0.20f, 1f);
    private static readonly Color _migrationHighlight = new Color(0.20f, 0.90f, 0.40f, 1f);
    private static readonly Color _yurtMoveHighlight  = new Color(1.00f, 0.80f, 0.00f, 1f);
    private static readonly Color _neutralHighlight   = new Color(0.90f, 0.85f, 0.10f, 1f);

    private Tile _selectedTile    = null;
    private Tile _hoveredTile     = null;
    private int  _moveCount       = 0;
    private int  _reinforceLeft   = 0;
    private bool _migrationUsed   = false;

    // Reachable tiles from the current migration selection (BFS result)
    private Dictionary<Tile, int> _reachableTiles = new Dictionary<Tile, int>();

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

        if (_selectedTile != null && CurrentState == GameState.MigrationCheck && !_selectedTile.HasYurt)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0f)
            {
                int dir = scroll > 0 ? 1 : -1;
                int max = Mathf.Max(1, _selectedTile.CubeCount - 1);
                _moveCount = Mathf.Clamp(_moveCount + dir, 1, max);
                ShowMigrationSelectionMessage();
            }
        }

        HandleHover();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick();
    }

    private void HandleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Tile newHover = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
            newHover = hit.collider.GetComponentInParent<Tile>();

        if (newHover == _selectedTile)
            newHover = null;

        if (newHover == _hoveredTile) return;

        if (_hoveredTile != null)
        {
            _hoveredTile.SetHovered(false);
            if (UIManager.Instance != null)
                UIManager.Instance.HideTooltip();
        }

        _hoveredTile = newHover;

        if (_hoveredTile != null)
        {
            _hoveredTile.SetHovered(true);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowTooltip(_hoveredTile);
        }
    }

    private void InitializeGame()
    {
        List<Tile> passable = new List<Tile>();
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.Terrain != TerrainType.Water && t.Terrain != TerrainType.Mountain)
                passable.Add(t);

        passable.Sort((a, b) => a.HexCoordinates.x.CompareTo(b.HexCoordinates.x));

        List<Tile> cluster0 = FindStartCluster(passable, fromWest: true, excluded: null);

        HashSet<Tile> exclusion = new HashSet<Tile>(cluster0);
        foreach (Tile t in cluster0)
            foreach (Tile n in HexGridManager.Instance.GetNeighbors(t.HexCoordinates))
                exclusion.Add(n);

        List<Tile> cluster1 = FindStartCluster(passable, fromWest: false, excluded: exclusion);

        PlaceStartCluster(0, cluster0);
        PlaceStartCluster(1, cluster1);

        RecalculatePrestige();
        AudioManager.Instance?.PlayTurnStart();
        StartReinforce();
    }

    private List<Tile> FindStartCluster(List<Tile> sortedPassable, bool fromWest, HashSet<Tile> excluded)
    {
        IEnumerable<Tile> seeds = fromWest
            ? (IEnumerable<Tile>)sortedPassable
            : System.Linq.Enumerable.Reverse(sortedPassable);

        HashSet<Tile> available = new HashSet<Tile>(sortedPassable);
        if (excluded != null)
            foreach (Tile t in excluded) available.Remove(t);

        foreach (Tile seed in seeds)
        {
            if (!available.Contains(seed)) continue;
            List<Tile> cluster = GrowCluster(seed, available, 3);
            if (cluster.Count == 3) return cluster;
        }

        List<Tile> fallback = new List<Tile>();
        foreach (Tile t in available) { fallback.Add(t); if (fallback.Count == 3) break; }
        return fallback;
    }

    private List<Tile> GrowCluster(Tile seed, HashSet<Tile> available, int targetCount)
    {
        List<Tile> cluster = new List<Tile> { seed };
        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(seed);

        while (cluster.Count < targetCount && frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            foreach (Tile n in HexGridManager.Instance.GetNeighbors(current.HexCoordinates))
            {
                if (!available.Contains(n) || cluster.Contains(n)) continue;
                cluster.Add(n);
                frontier.Enqueue(n);
                if (cluster.Count >= targetCount) break;
            }
        }
        return cluster;
    }

    // Places starting warriors and one yurt per clan (on the first tile of the cluster).
    private void PlaceStartCluster(int playerId, List<Tile> cluster)
    {
        for (int i = 0; i < cluster.Count; i++)
        {
            Tile tile = cluster[i];
            tile.OwnerPlayerId = playerId;
            tile.AddCubes(1);
            if (i == 0) tile.SetHasYurt(true); // nomadic base: 1 yurt per clan
            tile.UpdateVisuals();
        }
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
        // Confirm contested tiles the current player still holds (second-conquest rule)
        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
            if (tile.IsContested && tile.OwnerPlayerId == CurrentPlayerId)
                tile.SetContested(false);

        CurrentState   = GameState.Reinforce;
        _reinforceLeft = CalculateReinforcements(CurrentPlayerId);
        ClearSelection();

        RecalculatePrestige();
        AudioManager.Instance?.PlayTurnStart();

        HighlightReinforceTargets();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            bool hasYurt = PlayerHasYurt(CurrentPlayerId);
            string targetDesc = hasYurt ? "your yurt" : "any of your tiles";
            UIManager.Instance.ShowMessage(
                $"REINFORCE: Place {_reinforceLeft} warrior(s) on {targetDesc}. Click to place one.");
        }
    }

    private int CalculateReinforcements(int playerId)
    {
        int ownedCount = 0;
        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
            if (tile.OwnerPlayerId == playerId && !tile.IsContested) ownedCount++;
        return Mathf.Max(3, ownedCount / 3);
    }

    // Highlights valid reinforce targets: yurt tiles when available, all owned tiles as fallback.
    private void HighlightReinforceTargets()
    {
        ClearHighlights();
        bool hasYurt = PlayerHasYurt(CurrentPlayerId);
        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
        {
            if (tile.OwnerPlayerId != CurrentPlayerId) continue;
            if (tile.Terrain == TerrainType.Water || tile.Terrain == TerrainType.Mountain) continue;
            bool isValidTarget = hasYurt ? tile.HasYurt : true;
            if (!isValidTarget) continue;
            tile.SetHighlighted(true, _yurtMoveHighlight);
            _highlightedTiles.Add(tile);
        }
    }

    private bool PlayerHasYurt(int playerId)
    {
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.HasYurt && t.OwnerPlayerId == playerId) return true;
        return false;
    }

    private void HandleReinforceClick(Tile clicked)
    {
        if (clicked.OwnerPlayerId != CurrentPlayerId) return;
        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

        // Warriors spawn at the yurt (nomadic base). Fall back to any own tile if yurt is lost.
        bool hasYurt = PlayerHasYurt(CurrentPlayerId);
        if (hasYurt && !clicked.HasYurt)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    $"Warriors must be placed at your yurt. {_reinforceLeft} left to place.");
            return;
        }

        clicked.AddCubes(1);
        _reinforceLeft--;

        if (UIManager.Instance != null)
        {
            if (_reinforceLeft > 0)
                UIManager.Instance.ShowMessage($"REINFORCE: {_reinforceLeft} warrior(s) left to place at yurt.");
            else
                UIManager.Instance.ShowMessage("All warriors placed. Moving to Action phase...");
        }

        if (_reinforceLeft <= 0)
        {
            ClearHighlights();
            StartAction();
        }
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

            string forestNote = clicked.HasForest ? " [Forest: +1 bonus die]" : "";
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    $"Selected tile with {clicked.CubeCount} warriors{forestNote}. Click a red enemy tile to attack.");
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
                string forestNote = clicked.HasForest ? " [Forest: +1 bonus die]" : "";
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage(
                        $"Selected tile with {clicked.CubeCount} warriors{forestNote}. Click a red enemy tile to attack.");
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

        if (clicked.OwnerPlayerId == -1)
        {
            ExpandIntoNeutral(_selectedTile, clicked);
            return;
        }

        ExecuteAttack(_selectedTile, clicked);
    }

    private void ExpandIntoNeutral(Tile source, Tile target)
    {
        int moving = source.CubeCount - 1;
        source.RemoveCubes(moving);
        target.OwnerPlayerId = CurrentPlayerId;
        target.AddCubes(moving);
        target.UpdateVisuals();
        source.UpdateVisuals();

        target.TriggerConquestPulse();
        AudioManager.Instance?.PlayConquer();

        RecalculatePrestige();
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();

        if (CheckWinCondition()) return;

        ClearSelection();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage(
                "Expanded into neutral territory. Select another tile or press End Turn.");
    }

    private void ExecuteAttack(Tile attacker, Tile defender)
    {
        int attackingDice = Mathf.Min(3, attacker.CubeCount - 1);
        bool forestBonus  = attacker.HasForest;

        attacker.PlayAttackAnimation();
        defender.PlayAttackAnimation();

        bool conquered = CombatManager.Instance.ResolveCombat(attacker, defender, attackingDice, forestBonus);

        if (conquered)
        {
            // Second-conquest rule: tile is contested until the player survives their next turn
            defender.OwnerPlayerId = CurrentPlayerId;
            defender.SetContested(true);
            defender.TriggerConquestPulse();
            AudioManager.Instance?.PlayConquer();
        }
        else
        {
            AudioManager.Instance?.PlayAttack();
        }

        attacker.UpdateVisuals();
        defender.UpdateVisuals();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowCombatResult(
                CombatManager.Instance.LastResult,
                ClanNames[CurrentPlayerId],
                ClanNames[1 - CurrentPlayerId]);

        RecalculatePrestige();
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();

        if (CheckWinCondition()) return;

        ClearSelection();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("ACTION: Select another tile to attack, or press End Turn.");
    }

    private void HighlightAttackTargets(Tile source)
    {
        ClearHighlights();
        List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(source.HexCoordinates);
        foreach (Tile neighbor in neighbors)
        {
            if (neighbor.OwnerPlayerId == CurrentPlayerId) continue;
            if (neighbor.Terrain == TerrainType.Water || neighbor.Terrain == TerrainType.Mountain) continue;
            Color highlight = neighbor.OwnerPlayerId == -1 ? _neutralHighlight : _attackHighlight;
            neighbor.SetHighlighted(true, highlight);
            _highlightedTiles.Add(neighbor);
        }
    }

    // -------------------------------------------------------------------------
    // MIGRATION (civ-style 2-point movement + yurt relocation)
    // -------------------------------------------------------------------------

    private void StartMigration()
    {
        CurrentState   = GameState.MigrationCheck;
        _migrationUsed = false;
        ClearSelection();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "MIGRATION: Move warriors (2 MP budget) or relocate your yurt, then End Turn.");
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
            if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

            // Yurt tiles: enter yurt-move mode regardless of warrior count
            if (clicked.HasYurt)
            {
                _selectedTile = clicked;
                _selectedTile.SetSelected(true);
                HighlightYurtTargets(_selectedTile);
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage(
                        $"YURT: Click an adjacent own tile to relocate your camp.");
                return;
            }

            // Warrior migration requires at least 2 (leave 1 behind)
            if (clicked.CubeCount < 2) return;

            _selectedTile = clicked;
            _selectedTile.SetSelected(true);
            _moveCount = Mathf.Max(1, clicked.CubeCount - 1);
            HighlightMigrationTargets(_selectedTile);
            ShowMigrationSelectionMessage();
            return;
        }

        if (clicked == _selectedTile)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    "MIGRATION: Move warriors (2 MP budget) or relocate your yurt, then End Turn.");
            return;
        }

        if (clicked.OwnerPlayerId != CurrentPlayerId)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Migration only moves to your own tiles. Select again.");
            return;
        }

        if (!_reachableTiles.ContainsKey(clicked))
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Destination not reachable. Select again.");
            return;
        }

        if (_selectedTile.HasYurt)
        {
            ExecuteYurtMove(_selectedTile, clicked);
        }
        else
        {
            int count = Mathf.Clamp(_moveCount, 1, _selectedTile.CubeCount - 1);
            _selectedTile.PlayWalkAnimation();
            _selectedTile.RemoveCubes(count);
            clicked.AddCubes(count);
            clicked.UpdateVisuals();
            _migrationUsed = true;
            ClearSelection();
            AudioManager.Instance?.PlayMigration();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage($"Moved {count} warrior(s). Press End Turn to finish.");
        }
    }

    private void ExecuteYurtMove(Tile source, Tile destination)
    {
        source.SetHasYurt(false);
        destination.SetHasYurt(true);
        // Always move 1 warrior with the yurt to garrison the new camp
        if (source.CubeCount > 1)
        {
            source.RemoveCubes(1);
            destination.AddCubes(1);
        }
        source.UpdateVisuals();
        destination.UpdateVisuals();
        destination.TriggerConquestPulse();
        _migrationUsed = true;
        ClearSelection();
        AudioManager.Instance?.PlayMigration();
        RecalculatePrestige();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage("Yurt relocated! Press End Turn to finish.");
        }
    }

    // BFS through own tiles with a 2-movement-point budget.
    // Plain/Pasture = 1 point; forest tile = 2 points.
    private Dictionary<Tile, int> GetMigrationReachable(Tile source)
    {
        const int moveBudget = 2;
        Dictionary<Tile, int> costMap = new Dictionary<Tile, int>();
        // Use priority-aware search: process each (tile, cost) pair
        var queue = new Queue<(Tile tile, int cost)>();
        queue.Enqueue((source, 0));
        var visited = new HashSet<Tile> { source };

        while (queue.Count > 0)
        {
            var (current, cost) = queue.Dequeue();
            foreach (Tile neighbor in HexGridManager.Instance.GetNeighbors(current.HexCoordinates))
            {
                if (visited.Contains(neighbor)) continue;
                if (neighbor.OwnerPlayerId != CurrentPlayerId) continue;
                if (neighbor.Terrain == TerrainType.Water || neighbor.Terrain == TerrainType.Mountain) continue;

                int newCost = cost + neighbor.MoveCost;
                if (newCost <= moveBudget)
                {
                    visited.Add(neighbor);
                    costMap[neighbor] = newCost;
                    queue.Enqueue((neighbor, newCost));
                }
            }
        }
        return costMap;
    }

    private void HighlightMigrationTargets(Tile source)
    {
        ClearHighlights();
        _reachableTiles = GetMigrationReachable(source);
        foreach (Tile target in _reachableTiles.Keys)
        {
            target.SetHighlighted(true, _migrationHighlight);
            _highlightedTiles.Add(target);
        }
    }

    private void HighlightYurtTargets(Tile source)
    {
        ClearHighlights();
        _reachableTiles = new Dictionary<Tile, int>();
        List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(source.HexCoordinates);
        foreach (Tile n in neighbors)
        {
            if (n.OwnerPlayerId != CurrentPlayerId) continue;
            if (n.Terrain == TerrainType.Water || n.Terrain == TerrainType.Mountain) continue;
            _reachableTiles[n] = 1;
            n.SetHighlighted(true, _yurtMoveHighlight);
            _highlightedTiles.Add(n);
        }
    }

    private void ShowMigrationSelectionMessage()
    {
        if (UIManager.Instance == null || _selectedTile == null) return;
        int max = Mathf.Max(1, _selectedTile.CubeCount - 1);
        string forestNote = _selectedTile.HasForest ? " | forest=2MP" : "";
        UIManager.Instance.ShowMessage(
            $"Moving {_moveCount}/{_selectedTile.CubeCount} warriors  |  scroll to adjust  |  2 MP budget{forestNote}");
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
                if (tile.IsContested) continue; // contested tiles don't count until confirmed
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
        _reachableTiles.Clear();
        ClearHighlights();
    }
}
