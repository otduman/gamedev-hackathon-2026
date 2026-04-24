using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Reinforce,
    Action,
    MigrationCheck,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------
    public GameState CurrentState    { get; private set; }
    public int       CurrentPlayerId { get; private set; } = 0;

    // Prestige per player: prestige = owned tiles + (yurt tiles * 2)
    public int[] PlayerPrestige = new int[2];

    private const int PrestigeWinThreshold = 15;

    // -------------------------------------------------------------------------
    // Reinforce phase
    // -------------------------------------------------------------------------
    private int _cubesLeftToPlace = 0;

    // -------------------------------------------------------------------------
    // Action phase
    // -------------------------------------------------------------------------
    private Tile _selectedTile = null;

    // -------------------------------------------------------------------------
    // Migration phase
    // -------------------------------------------------------------------------
    private Tile _migrationSourceTile = null;
    private bool _migrationUsed       = false;  // one move per turn

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Subscribe BEFORE HexGridManager.Start() runs so we never miss the event.
        // Both are on the same GameObject (or at least in the same scene Awake pass),
        // but subscribing in Awake guarantees ordering safety.
        HexGridManager hexGrid = GetComponent<HexGridManager>();
        if (hexGrid == null)
            hexGrid = FindAnyObjectByType<HexGridManager>();

        if (hexGrid != null)
            hexGrid.OnGridReady += InitializeGame;
        else
            Debug.LogError("GameManager: could not find HexGridManager to subscribe to OnGridReady.");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    // -------------------------------------------------------------------------
    // Grid-ready callback
    // -------------------------------------------------------------------------

    private void InitializeGame()
    {
        Dictionary<Vector2Int, Tile> grid = HexGridManager.Instance.Grid;

        // Collect west tiles (q <= -2) for Player 0 and east tiles (q >= 2) for Player 1.
        List<Tile> westTiles = new List<Tile>();
        List<Tile> eastTiles = new List<Tile>();

        foreach (KeyValuePair<Vector2Int, Tile> entry in grid)
        {
            int q = entry.Key.x;
            if (q <= -2) westTiles.Add(entry.Value);
            else if (q >= 2) eastTiles.Add(entry.Value);
        }

        AssignStartingTiles(0, westTiles, 5, 3);
        AssignStartingTiles(1, eastTiles, 5, 3);

        ChangeState(GameState.Reinforce);
    }

    // Picks 'count' random tiles from 'candidates', claims them for 'playerId',
    // and places 'cubesPerTile' on each.
    private void AssignStartingTiles(int playerId, List<Tile> candidates, int count, int cubesPerTile)
    {
        // Shuffle in place using Fisher-Yates.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Tile tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
        }

        int assigned = 0;
        foreach (Tile tile in candidates)
        {
            if (assigned >= count) break;
            tile.OwnerPlayerId = playerId;
            tile.AddCubes(cubesPerTile);
            tile.UpdateVisuals();
            assigned++;
        }

        if (assigned < count)
            Debug.LogWarning($"GameManager: only {assigned} starting tiles found for Player {playerId} (wanted {count}).");
    }

    // -------------------------------------------------------------------------
    // State machine
    // -------------------------------------------------------------------------

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] State -> {CurrentState}  (Player {CurrentPlayerId})");

        switch (CurrentState)
        {
            case GameState.Reinforce:
                EnterReinforce();
                break;

            case GameState.Action:
                EnterAction();
                break;

            case GameState.MigrationCheck:
                EnterMigration();
                break;

            case GameState.GameOver:
                // UI notification is handled by CheckWinCondition before reaching here.
                break;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();
    }

    // -------------------------------------------------------------------------
    // Reinforce phase
    // -------------------------------------------------------------------------

    private void EnterReinforce()
    {
        int owned = CountOwnedTiles(CurrentPlayerId);
        _cubesLeftToPlace = Mathf.Max(3, owned / 3);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage($"Place {_cubesLeftToPlace} warrior(s) on your tiles.");

        // If the player somehow owns no tiles (edge case), skip straight to Action.
        if (_cubesLeftToPlace == 0)
            ChangeState(GameState.Action);
    }

    // Called by click handling during Reinforce.
    private void HandleReinforceClick(Tile clickedTile)
    {
        if (clickedTile.OwnerPlayerId != CurrentPlayerId) return;

        clickedTile.AddCubes(1);
        _cubesLeftToPlace--;

        string msg = _cubesLeftToPlace > 0
            ? $"Place {_cubesLeftToPlace} more warrior(s)."
            : "All warriors placed!";

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage(msg);

        if (_cubesLeftToPlace <= 0)
            ChangeState(GameState.Action);
    }

    // -------------------------------------------------------------------------
    // Action phase
    // -------------------------------------------------------------------------

    private void EnterAction()
    {
        ClearSelection();
        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Select your tile to attack or press End Turn.");
    }

    private void HandleActionClick(Tile clickedTile)
    {
        if (_selectedTile == null)
        {
            // First click: select an owned tile that has more than 1 cube.
            if (clickedTile.OwnerPlayerId == CurrentPlayerId && clickedTile.CubeCount > 1)
            {
                _selectedTile = clickedTile;
                _selectedTile.SetSelected(true);

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Now click an adjacent enemy tile to attack.");
            }
        }
        else
        {
            if (clickedTile == _selectedTile)
            {
                // Clicking the same tile again deselects it.
                ClearSelection();
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Deselected. Pick a tile to attack from.");
                return;
            }

            if (clickedTile.OwnerPlayerId == CurrentPlayerId)
            {
                // Clicking a different own tile swaps the selection.
                ClearSelection();
                if (clickedTile.CubeCount > 1)
                {
                    _selectedTile = clickedTile;
                    _selectedTile.SetSelected(true);
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowMessage("Now click an adjacent enemy tile to attack.");
                }
                return;
            }

            // Clicking an enemy tile: check adjacency before attacking.
            List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(_selectedTile.HexCoordinates);
            if (!neighbors.Contains(clickedTile))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("That tile is not adjacent. Choose a neighbour.");
                ClearSelection();
                return;
            }

            // Execute attack.
            int attackingDice = Mathf.Min(3, _selectedTile.CubeCount - 1);
            if (attackingDice > 0)
            {
                bool conquered = CombatManager.Instance.ResolveCombat(_selectedTile, clickedTile, attackingDice);

                string combatMsg = conquered
                    ? $"Conquered! Tile taken by Clan {CurrentPlayerId + 1}."
                    : $"Attack failed. The defenders held.";

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowCombatResult(combatMsg);

                CheckWinCondition();
            }

            ClearSelection();
        }
    }

    private void ClearSelection()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            _selectedTile = null;
        }
    }

    // -------------------------------------------------------------------------
    // Migration phase
    // -------------------------------------------------------------------------

    private void EnterMigration()
    {
        _migrationSourceTile = null;
        _migrationUsed       = false;
        ClearSelection();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Migration: move cubes to an adjacent owned tile, or press End Turn to skip.");
    }

    private void HandleMigrationClick(Tile clickedTile)
    {
        if (_migrationUsed)
        {
            // Migration move already spent — wait for End Turn.
            return;
        }

        if (_migrationSourceTile == null)
        {
            // Select source tile (must be owned and have at least 2 cubes so 1 stays).
            if (clickedTile.OwnerPlayerId == CurrentPlayerId && clickedTile.CubeCount >= 2)
            {
                _migrationSourceTile = clickedTile;
                _migrationSourceTile.SetSelected(true);

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Now click an adjacent owned tile to move cubes to.");
            }
        }
        else
        {
            if (clickedTile == _migrationSourceTile)
            {
                // Deselect.
                _migrationSourceTile.SetSelected(false);
                _migrationSourceTile = null;
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Deselected. Pick a source tile for migration.");
                return;
            }

            // Destination must be owned and adjacent.
            if (clickedTile.OwnerPlayerId != CurrentPlayerId)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Destination must be your own tile.");
                return;
            }

            List<Tile> neighbors = HexGridManager.Instance.GetNeighbors(_migrationSourceTile.HexCoordinates);
            if (!neighbors.Contains(clickedTile))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Tiles are not adjacent. Try again.");
                _migrationSourceTile.SetSelected(false);
                _migrationSourceTile = null;
                return;
            }

            // Move half the cubes (rounded down), always leaving at least 1 behind.
            int moveCount = Mathf.Max(1, _migrationSourceTile.CubeCount / 2);
            if (moveCount >= _migrationSourceTile.CubeCount)
                moveCount = _migrationSourceTile.CubeCount - 1;

            _migrationSourceTile.RemoveCubes(moveCount);
            clickedTile.AddCubes(moveCount);

            _migrationSourceTile.SetSelected(false);
            _migrationSourceTile = null;
            _migrationUsed = true;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage($"Moved {moveCount} warrior(s). Press End Turn.");

            // Auto-end the turn after the single migration move.
            EndTurn();
        }
    }

    // -------------------------------------------------------------------------
    // Click dispatcher
    // -------------------------------------------------------------------------

    private void HandleClick()
    {
        if (CurrentState == GameState.GameOver) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Tile clickedTile = hit.collider.GetComponent<Tile>();
        if (clickedTile == null) return;

        switch (CurrentState)
        {
            case GameState.Reinforce:      HandleReinforceClick(clickedTile); break;
            case GameState.Action:         HandleActionClick(clickedTile);    break;
            case GameState.MigrationCheck: HandleMigrationClick(clickedTile); break;
        }
    }

    // -------------------------------------------------------------------------
    // End Turn button (wired from UIManager)
    // -------------------------------------------------------------------------

    public void OnEndTurnButtonClicked()
    {
        switch (CurrentState)
        {
            case GameState.Action:
                ChangeState(GameState.MigrationCheck);
                break;

            case GameState.MigrationCheck:
                EndTurn();
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Turn end
    // -------------------------------------------------------------------------

    public void EndTurn()
    {
        ClearSelection();
        if (_migrationSourceTile != null)
        {
            _migrationSourceTile.SetSelected(false);
            _migrationSourceTile = null;
        }

        CurrentPlayerId = (CurrentPlayerId + 1) % 2;
        ChangeState(GameState.Reinforce);
    }

    // -------------------------------------------------------------------------
    // Win condition
    // -------------------------------------------------------------------------

    private void CheckWinCondition()
    {
        int tiles0 = 0, tiles1 = 0;
        int yurts0 = 0, yurts1 = 0;

        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
        {
            if (tile.OwnerPlayerId == 0) { tiles0++; if (tile.HasYurt) yurts0++; }
            else if (tile.OwnerPlayerId == 1) { tiles1++; if (tile.HasYurt) yurts1++; }
        }

        PlayerPrestige[0] = tiles0 + yurts0 * 2;
        PlayerPrestige[1] = tiles1 + yurts1 * 2;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();

        // Elimination check — player owns zero tiles.
        if (tiles0 == 0)
        {
            DeclareWinner(1);
            return;
        }
        if (tiles1 == 0)
        {
            DeclareWinner(0);
            return;
        }

        // Prestige threshold check.
        if (PlayerPrestige[0] >= PrestigeWinThreshold)
        {
            DeclareWinner(0);
            return;
        }
        if (PlayerPrestige[1] >= PrestigeWinThreshold)
        {
            DeclareWinner(1);
        }
    }

    private void DeclareWinner(int winningPlayerId)
    {
        Debug.Log($"[GameManager] Clan {winningPlayerId + 1} wins with {PlayerPrestige[winningPlayerId]} prestige!");
        ChangeState(GameState.GameOver);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(winningPlayerId);
    }

    // -------------------------------------------------------------------------
    // Utility
    // -------------------------------------------------------------------------

    private int CountOwnedTiles(int playerId)
    {
        int count = 0;
        foreach (Tile tile in HexGridManager.Instance.Grid.Values)
        {
            if (tile.OwnerPlayerId == playerId) count++;
        }
        return count;
    }
}
