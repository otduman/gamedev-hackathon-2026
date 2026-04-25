using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { Reinforce, Action, CombatSetup, MigrationCheck, MigrationChoice, MigrationCount, GameOver }

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

    private Tile _selectedTile           = null;
    private Tile _hoveredTile            = null;
    private int  _moveCount              = 0;
    private int  _reinforceLeft          = 0;
    private bool _migrationUsed          = false;

    // Pending state for the modal combat-setup panel
    private Tile _pendingAttacker        = null;
    private Tile _pendingDefender        = null;

    // Pending destination while the migration count panel is open
    private Tile _pendingMigrationDest   = null;

    // True when the active migration is moving the yurt; false for warrior migration.
    // Set in HandleMigrationClick / OnMigrationChoose* and consumed in the destination handler.
    private bool _yurtMigrationMode      = false;

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
            int yurtCount = CountYurts(CurrentPlayerId);
            string targetDesc;
            if (yurtCount > 1)      targetDesc = $"any of your {yurtCount} yurts (split however you like)";
            else if (yurtCount == 1) targetDesc = "your yurt";
            else                     targetDesc = "any of your tiles";
            string bonusNote = yurtCount > 1 ? $" — includes +{yurtCount - 1} from conquered yurt(s)" : "";
            UIManager.Instance.ShowMessage(
                $"REINFORCE: Place {_reinforceLeft} warrior(s) on {targetDesc}{bonusNote}.");
        }
    }

    private int CalculateReinforcements(int playerId)
    {
        int ownedCount = 0;
        int yurtCount  = 0;
        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
        {
            if (tile.OwnerPlayerId != playerId) continue;
            if (tile.IsContested) continue;
            ownedCount++;
            if (tile.HasYurt) yurtCount++;
        }
        // Base is max(3, tiles/3). Each *extra* yurt beyond the first adds +1
        // (conquering an enemy yurt goes from 3 to 4, two conquests → 5, etc.).
        return Mathf.Max(3, ownedCount / 3) + Mathf.Max(0, yurtCount - 1);
    }

    private int CountYurts(int playerId)
    {
        int n = 0;
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.HasYurt && t.OwnerPlayerId == playerId && !t.IsContested) n++;
        return n;
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
                    $"Warriors must be placed on a yurt tile. {_reinforceLeft} left to place.");
            return;
        }

        clicked.AddCubes(1);
        _reinforceLeft--;

        if (UIManager.Instance != null)
        {
            if (_reinforceLeft > 0)
                UIManager.Instance.ShowMessage(
                    $"REINFORCE: {_reinforceLeft} warrior(s) left — click any of your yurts.");
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

        BeginCombatSetup(_selectedTile, clicked);
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

    // Opens the modal combat-setup panel so each side can pick its dice count.
    private void BeginCombatSetup(Tile attacker, Tile defender)
    {
        _pendingAttacker = attacker;
        _pendingDefender = defender;

        int maxAtk = Mathf.Min(3, attacker.CubeCount - 1);
        int maxDef = Mathf.Min(2, defender.CubeCount);

        CurrentState = GameState.CombatSetup;
        ClearHighlights();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowCombatSetup(
                CurrentPlayerId, 1 - CurrentPlayerId,
                maxAtk, maxDef,
                ClanNames[CurrentPlayerId],
                ClanNames[1 - CurrentPlayerId]);
        }
    }

    // Wired to the Resolve button on the combat-setup panel.
    public void OnCombatSetupConfirm(int atkDice, int defDice)
    {
        if (CurrentState != GameState.CombatSetup) return;
        if (_pendingAttacker == null || _pendingDefender == null) return;

        Tile attacker = _pendingAttacker;
        Tile defender = _pendingDefender;
        bool forestBonus = attacker.HasForest;

        attacker.PlayAttackAnimation();
        defender.PlayAttackAnimation();

        bool conquered = CombatManager.Instance.ResolveCombat(
            attacker, defender, atkDice, defDice, forestBonus);

        if (conquered)
        {
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
        {
            UIManager.Instance.HideCombatSetup();
            UIManager.Instance.ShowCombatResult(
                CombatManager.Instance.LastResult,
                CurrentPlayerId, 1 - CurrentPlayerId,
                ClanNames[CurrentPlayerId],
                ClanNames[1 - CurrentPlayerId]);
        }

        _pendingAttacker = null;
        _pendingDefender = null;
        CurrentState = GameState.Action;

        RecalculatePrestige();
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();

        if (CheckWinCondition()) return;

        ClearSelection();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("ACTION: Select another tile to attack, or press End Turn.");
    }

    // Wired to the Cancel button on the combat-setup panel.
    public void OnCombatSetupCancel()
    {
        if (CurrentState != GameState.CombatSetup) return;
        _pendingAttacker = null;
        _pendingDefender = null;
        CurrentState = GameState.Action;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideCombatSetup();
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage("Combat cancelled. Select another tile or press End Turn.");
        }

        ClearSelection();
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

            bool canMoveYurt     = clicked.HasYurt;
            bool canMoveWarriors = clicked.CubeCount >= 2;
            if (!canMoveYurt && !canMoveWarriors) return;

            // Both options available — let the player choose.
            if (canMoveYurt && canMoveWarriors)
            {
                _selectedTile = clicked;
                _selectedTile.SetSelected(true);
                CurrentState = GameState.MigrationChoice;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                    UIManager.Instance.ShowMigrationChoice();
                    UIManager.Instance.ShowMessage("Move soldiers or relocate the yurt?");
                }
                return;
            }

            // Single-option tiles enter the matching mode directly.
            _selectedTile = clicked;
            _selectedTile.SetSelected(true);
            if (canMoveYurt)
            {
                _yurtMigrationMode = true;
                HighlightYurtTargets(_selectedTile);
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage(
                        "YURT: Click an adjacent own tile to relocate (1-tile range).");
            }
            else
            {
                _yurtMigrationMode = false;
                HighlightMigrationTargets(_selectedTile);
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage(
                        "Click any of your tiles to migrate warriors. You will pick how many.");
            }
            return;
        }

        if (clicked == _selectedTile)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage(
                    "MIGRATION: Move warriors or relocate your yurt, then End Turn.");
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
                UIManager.Instance.ShowMessage("Destination not reachable through your territory. Select again.");
            return;
        }

        if (_yurtMigrationMode)
        {
            ExecuteYurtMove(_selectedTile, clicked);
        }
        else
        {
            BeginMigrationCount(clicked);
        }
    }

    // Wired to the "Move Soldiers" button on the migration choice panel.
    public void OnMigrationChooseSoldiers()
    {
        if (CurrentState != GameState.MigrationChoice || _selectedTile == null) return;
        _yurtMigrationMode = false;
        CurrentState = GameState.MigrationCheck;
        HighlightMigrationTargets(_selectedTile);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMigrationChoice();
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "Click any of your tiles to migrate warriors. You will pick how many.");
        }
    }

    // Wired to the "Move Yurta" button on the migration choice panel.
    public void OnMigrationChooseYurta()
    {
        if (CurrentState != GameState.MigrationChoice || _selectedTile == null) return;
        _yurtMigrationMode = true;
        CurrentState = GameState.MigrationCheck;
        HighlightYurtTargets(_selectedTile);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMigrationChoice();
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "YURT: Click an adjacent own tile to relocate (1-tile range).");
        }
    }

    // Wired to the Cancel button on the migration choice panel.
    public void OnMigrationChoiceCancel()
    {
        if (CurrentState != GameState.MigrationChoice) return;
        CurrentState = GameState.MigrationCheck;
        ClearSelection();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMigrationChoice();
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "MIGRATION: Move warriors or relocate your yurt, then End Turn.");
        }
    }

    // Opens the migration count panel for warrior migration.
    private void BeginMigrationCount(Tile destination)
    {
        if (_selectedTile == null) return;

        _pendingMigrationDest = destination;
        int max = Mathf.Max(1, _selectedTile.CubeCount - 1);
        CurrentState = GameState.MigrationCount;
        ClearHighlights();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMigrationCount(max, max);
            UIManager.Instance.ShowMessage("Pick how many warriors to migrate, then Confirm.");
        }
    }

    // Wired to the Confirm button on the migration count panel.
    public void OnMigrationCountConfirm(int count)
    {
        if (CurrentState != GameState.MigrationCount) return;
        if (_selectedTile == null || _pendingMigrationDest == null) return;

        int actual = Mathf.Clamp(count, 1, _selectedTile.CubeCount - 1);
        _selectedTile.PlayWalkAnimation();
        _selectedTile.RemoveCubes(actual);
        _pendingMigrationDest.AddCubes(actual);
        _pendingMigrationDest.UpdateVisuals();

        _migrationUsed = true;
        AudioManager.Instance?.PlayMigration();

        _pendingMigrationDest = null;
        CurrentState = GameState.MigrationCheck;
        ClearSelection();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMigrationCount();
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage($"Moved {actual} warrior(s). Press End Turn to finish.");
        }
    }

    // Wired to the Cancel button on the migration count panel.
    public void OnMigrationCountCancel()
    {
        if (CurrentState != GameState.MigrationCount) return;
        _pendingMigrationDest = null;
        CurrentState = GameState.MigrationCheck;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMigrationCount();
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage(
                "MIGRATION: Move warriors or relocate your yurt, then End Turn.");
        }

        ClearSelection();
    }

    private void ExecuteYurtMove(Tile source, Tile destination)
    {
        // Safety: never overwrite an existing yurt — one per tile only.
        if (destination.HasYurt)
        {
            ClearSelection();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("That tile already has a yurt. Pick another destination.");
            return;
        }

        // Yurt migrates alone — soldiers stay where they are.
        source.SetHasYurt(false);
        destination.SetHasYurt(true);
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

    // BFS through own tiles. Any tile connected to the source by a chain of own
    // (non-water, non-mountain) tiles is reachable — no movement-point cap.
    private Dictionary<Tile, int> GetMigrationReachable(Tile source)
    {
        Dictionary<Tile, int> costMap = new Dictionary<Tile, int>();
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(source);
        HashSet<Tile> visited = new HashSet<Tile> { source };

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            foreach (Tile neighbor in HexGridManager.Instance.GetNeighbors(current.HexCoordinates))
            {
                if (visited.Contains(neighbor)) continue;
                if (neighbor.OwnerPlayerId != CurrentPlayerId) continue;
                if (neighbor.Terrain == TerrainType.Water || neighbor.Terrain == TerrainType.Mountain) continue;

                visited.Add(neighbor);
                costMap[neighbor] = 1;
                queue.Enqueue(neighbor);
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
        // Yurta migration is capped at 1 tile per turn, and only one yurt per tile is allowed.
        ClearHighlights();
        _reachableTiles = new Dictionary<Tile, int>();
        foreach (Tile n in HexGridManager.Instance.GetNeighbors(source.HexCoordinates))
        {
            if (n.OwnerPlayerId != CurrentPlayerId) continue;
            if (n.Terrain == TerrainType.Water || n.Terrain == TerrainType.Mountain) continue;
            if (n.HasYurt) continue; // unique-building rule: one yurt per tile
            _reachableTiles[n] = 1;
            n.SetHighlighted(true, _yurtMoveHighlight);
            _highlightedTiles.Add(n);
        }
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
        _moveCount         = 0;
        _yurtMigrationMode = false;
        _reachableTiles.Clear();
        ClearHighlights();
    }
}
