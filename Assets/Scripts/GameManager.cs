using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { MainMenu, InitialPlacement, Reinforce, Attack, CombatSetup, PostConquest, Fortify, GameOver }

public enum MissionType { ControlTerritories, DestroyPlayer, ControlTwoRegions }

public class Mission
{
    public MissionType Type;
    public int    TargetCount;
    public int    TargetPlayer;
    public int[]  TargetRegions;
    public string Description;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const int PlayerCount = 4;

    public GameState CurrentState    { get; private set; } = GameState.MainMenu;
    public int       CurrentPlayerId { get; private set; } = 0;

    private static readonly string[] ClanNames = { "Kazakh", "Timurid", "Rusya", "Ming" };

    public static readonly string[] RegionNames =
        { "Desht-i Kipchak", "Mawarannahr", "Khorasan", "Cathay", "Rus'", "Sibir" };

    private static readonly int[] RegionBonuses = { 5, 5, 4, 4, 3, 3 };

    private static readonly Color _attackHighlight   = new Color(1.00f, 0.55f, 0.00f, 1f);
    private static readonly Color _fortifyHighlight  = new Color(0.25f, 1.00f, 0.50f, 1f);
    private static readonly Color _reinforceHighlight = new Color(0.95f, 0.30f, 0.85f, 1f); // bright pink — no clan conflict

    // Missions
    private Mission[] _playerMissions = new Mission[PlayerCount];

    // Initial placement
    private const int ExtraTroopsPerPlayer = 15;
    private int[] _extraTroopsLeft = new int[PlayerCount];
    private int   _placementTurn   = 0;

    // Turn state
    private Tile _selectedTile    = null;
    private Tile _hoveredTile     = null;
    private int  _reinforceLeft   = 0;
    private bool _fortifyUsed     = false;
    private Tile _fortifySource   = null;
    private Tile _fortifyDest     = null;
    private bool _waitingForCount = false; // true while the troop-count panel is open

    // Combat pending
    private Tile _pendingAttacker    = null;
    private Tile _pendingDefender    = null;
    private Tile _postConquestSource = null;
    private Tile _postConquestDest   = null;

    private List<Tile> _highlightedTiles = new List<Tile>();
    private bool  _gridReady      = false;
    private float _idleTimer      = 0f;
    private bool  _idleHintActive = false;
    private const float IdleHintDelay = 12f;

    private static readonly string[] IdleHints =
    {
        "",  // MainMenu
        "Click a highlighted territory to place a troop — or press SKIP TURN",
        "Click your territories to place reinforcement troops",
        "Select one of your territories with 2+ troops, then click an enemy",
        "",  // CombatSetup
        "",  // PostConquest
        "Move troops between connected territories, or press END TURN to skip",
        "",  // GameOver
    };

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        QualitySettings.anisotropicFiltering     = AnisotropicFiltering.ForceEnable;
        QualitySettings.antiAliasing             = 4;
        QualitySettings.globalTextureMipmapLimit = 0;

        HexGridManager hexGrid = GetComponent<HexGridManager>() ?? FindAnyObjectByType<HexGridManager>();
        if (hexGrid != null) hexGrid.OnGridReady += OnGridReadyHandler;
        else Debug.LogError("GameManager: could not find HexGridManager.");
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        HandleHover();
        if (Mouse.current.leftButton.wasPressedThisFrame) HandleClick();
        TickIdleHint();
    }

    private void TickIdleHint()
    {
        bool actionable = CurrentState == GameState.InitialPlacement
                       || CurrentState == GameState.Reinforce
                       || CurrentState == GameState.Attack
                       || CurrentState == GameState.Fortify;
        if (!actionable) { _idleTimer = 0f; _idleHintActive = false; return; }

        _idleTimer += Time.deltaTime;
        if (!_idleHintActive && _idleTimer >= IdleHintDelay)
        {
            _idleHintActive = true;
            int idx = (int)CurrentState;
            string hint = idx >= 0 && idx < IdleHints.Length ? IdleHints[idx] : "";
            if (!string.IsNullOrEmpty(hint))
                UIManager.Instance?.ShowIdleHint(hint);
        }
    }

    private void ResetIdleTimer()
    {
        _idleTimer      = 0f;
        _idleHintActive = false;
        UIManager.Instance?.HideIdleHint();
    }

    private void HandleHover()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (_hoveredTile != null) { _hoveredTile.SetHovered(false); UIManager.Instance?.HideTooltip(); _hoveredTile = null; }
            return;
        }

        Ray  ray      = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Tile newHover = null;
        if (Physics.Raycast(ray, out RaycastHit hit))
            newHover = hit.collider.GetComponentInParent<Tile>();
        if (newHover == _selectedTile) newHover = null;
        if (newHover == _hoveredTile) return;
        if (_hoveredTile != null) { _hoveredTile.SetHovered(false); UIManager.Instance?.HideTooltip(); }
        _hoveredTile = newHover;
        if (_hoveredTile != null) { _hoveredTile.SetHovered(true); UIManager.Instance?.ShowTooltip(_hoveredTile); }
    }

    private void HandleClick()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.MainMenu) return;
        if (_waitingForCount) return; // count panel is open — ignore tile clicks
        ResetIdleTimer();

        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        Tile clicked = hit.collider.GetComponentInParent<Tile>();
        if (clicked == null) return;

        switch (CurrentState)
        {
            case GameState.InitialPlacement: HandlePlacementClick(clicked); break;
            case GameState.Reinforce:        HandleReinforceClick(clicked); break;
            case GameState.Attack:           HandleAttackClick(clicked);    break;
            case GameState.Fortify:          HandleFortifyClick(clicked);   break;
        }
    }

    // -------------------------------------------------------------------------
    // INITIALIZATION
    // -------------------------------------------------------------------------

    private void OnGridReadyHandler()
    {
        _gridReady = true;
        CurrentState = GameState.MainMenu;
        UIManager.Instance?.ShowMainMenu();
    }

    private void InitializeGame()
    {
        // Reset every tile so a rematch starts clean — no leftover troops/yurts
        // from the previous game.
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
        {
            t.OwnerPlayerId = -1;
            t.CubeCount     = 0;
            t.SetHasYurt(false);
            t.UpdateVisuals();
        }

        // Reset selection / phase bookkeeping so stale references don't leak in.
        ClearSelection();
        _placementTurn      = 0;
        _reinforceLeft      = 0;
        _fortifyUsed        = false;
        _pendingAttacker    = null;
        _pendingDefender    = null;
        _postConquestSource = null;
        _postConquestDest   = null;

        List<Tile> passable = new List<Tile>();
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.Terrain != TerrainType.Water && t.Terrain != TerrainType.Mountain)
                passable.Add(t);

        Shuffle(passable);

        for (int i = 0; i < passable.Count; i++)
        {
            Tile t = passable[i];
            t.OwnerPlayerId = i % PlayerCount;
            t.AddCubes(1);
            t.UpdateVisuals();
        }

        for (int pid = 0; pid < PlayerCount; pid++)
            _extraTroopsLeft[pid] = ExtraTroopsPerPlayer;

        AssignMissions();
        HexGridManager.Instance.RefreshAllBorders();
        UIManager.Instance?.ShowLeaderSelection();
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }

    private void AssignMissions()
    {
        HashSet<int> regionSet = new HashSet<int>();
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.ContinentId >= 0 && t.Terrain != TerrainType.Water && t.Terrain != TerrainType.Mountain)
                regionSet.Add(t.ContinentId);
        List<int> regions = new List<int>(regionSet);

        List<Mission> pool = new List<Mission>();
        pool.Add(new Mission { Type = MissionType.ControlTerritories, TargetCount = 24,
            Description = "Control 24 territories" });
        for (int pid = 0; pid < PlayerCount; pid++)
            pool.Add(new Mission { Type = MissionType.DestroyPlayer, TargetPlayer = pid,
                Description = $"Destroy {ClanNames[pid]}" });
        if (regions.Count >= 2)
            for (int i = 0; i < regions.Count - 1; i++)
                for (int j = i + 1; j < regions.Count; j++)
                    pool.Add(new Mission { Type = MissionType.ControlTwoRegions,
                        TargetRegions = new[] { regions[i], regions[j] },
                        Description = $"Control {SafeRegionName(regions[i])} & {SafeRegionName(regions[j])}" });

        Shuffle(pool);

        for (int pid = 0; pid < PlayerCount; pid++)
        {
            Mission m = pool[pid % pool.Count];
            if (m.Type == MissionType.DestroyPlayer && m.TargetPlayer == pid)
                m = new Mission { Type = MissionType.ControlTerritories, TargetCount = 24,
                    Description = "Control 24 territories" };
            _playerMissions[pid] = m;
        }
    }

    private static string SafeRegionName(int id) =>
        id >= 0 && id < RegionNames.Length ? RegionNames[id] : $"Region {id}";

    // -------------------------------------------------------------------------
    // INITIAL PLACEMENT
    // -------------------------------------------------------------------------

    private void BeginInitialPlacement()
    {
        CurrentState  = GameState.InitialPlacement;
        _placementTurn = 0;
        AdvancePlacement();
    }

    private void AdvancePlacement()
    {
        for (int tries = 0; tries < PlayerCount; tries++)
        {
            int pid = _placementTurn % PlayerCount;
            if (_extraTroopsLeft[pid] > 0)
            {
                CurrentPlayerId = pid;
                HighlightOwnTiles(pid);
                UIManager.Instance?.UpdateUI();
                UIManager.Instance?.ShowPhaseTransition("PLACE TROOPS", UIManager.GetClanColor(pid));
                UIManager.Instance?.ShowMessage(
                    $"PLACEMENT — {ClanNames[pid]}: click any of your territories. {_extraTroopsLeft[pid]} left.");
                return;
            }
            _placementTurn++;
        }

        ClearHighlights();
        CurrentPlayerId = 0;
        StartReinforce();
    }

    private void HandlePlacementClick(Tile clicked)
    {
        if (clicked.OwnerPlayerId != CurrentPlayerId) return;
        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

        clicked.AddCubes(1);
        _extraTroopsLeft[CurrentPlayerId]--;

        // Round-robin: pass to the next player after every single troop placed.
        _placementTurn = (CurrentPlayerId + 1) % PlayerCount;
        ClearHighlights();
        AdvancePlacement();
    }

    public void OnRandomizePlacement()
    {
        if (CurrentState != GameState.InitialPlacement) return;
        RandomizeForPlayer(CurrentPlayerId);
        _placementTurn = (CurrentPlayerId + 1) % PlayerCount;
        ClearHighlights();
        AdvancePlacement();
    }

    public void OnRandomizeAll()
    {
        if (CurrentState != GameState.InitialPlacement) return;
        for (int pid = 0; pid < PlayerCount; pid++)
            if (_extraTroopsLeft[pid] > 0) RandomizeForPlayer(pid);
        ClearHighlights();
        CurrentPlayerId = 0;
        StartReinforce();
    }

    private void RandomizeForPlayer(int pid)
    {
        List<Tile> own = new List<Tile>();
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.OwnerPlayerId == pid && t.Terrain != TerrainType.Water && t.Terrain != TerrainType.Mountain)
                own.Add(t);
        int n = _extraTroopsLeft[pid];
        for (int i = 0; i < n && own.Count > 0; i++)
            own[Random.Range(0, own.Count)].AddCubes(1);
        _extraTroopsLeft[pid] = 0;
    }

    // -------------------------------------------------------------------------
    // REINFORCE
    // -------------------------------------------------------------------------

    private void StartReinforce()
    {
        CurrentState   = GameState.Reinforce;
        _reinforceLeft = CalculateReinforcements(CurrentPlayerId);
        ClearSelection();
        HighlightOwnTiles(CurrentPlayerId);
        // no turn-start clip in new AudioManager
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowPhaseTransition("REINFORCE", UIManager.GetClanColor(CurrentPlayerId));
        UIManager.Instance?.ShowMessage($"REINFORCE: Place {_reinforceLeft} troop(s) on your territories.");
    }

    private int CalculateReinforcements(int playerId)
    {
        int owned = 0;
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.OwnerPlayerId == playerId) owned++;
        int bonus = 0;
        for (int r = 0; r < RegionBonuses.Length; r++)
            if (PlayerControlsRegion(playerId, r)) bonus += RegionBonuses[r];
        return Mathf.Max(3, owned / 3) + bonus;
    }

    private bool PlayerControlsRegion(int playerId, int regionId)
    {
        bool hasAny = false;
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
        {
            if (t.ContinentId != regionId) continue;
            if (t.Terrain == TerrainType.Water || t.Terrain == TerrainType.Mountain) continue;
            hasAny = true;
            if (t.OwnerPlayerId != playerId) return false;
        }
        return hasAny;
    }

    private void HandleReinforceClick(Tile clicked)
    {
        if (clicked.OwnerPlayerId != CurrentPlayerId) return;
        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;

        clicked.AddCubes(1);
        _reinforceLeft--;

        if (_reinforceLeft <= 0) { ClearHighlights(); StartAttack(); }
        else UIManager.Instance?.ShowMessage($"REINFORCE: {_reinforceLeft} troop(s) left to place.");
    }

    private void HighlightOwnTiles(int playerId)
    {
        ClearHighlights();
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
        {
            if (t.OwnerPlayerId != playerId) continue;
            if (t.Terrain == TerrainType.Water || t.Terrain == TerrainType.Mountain) continue;
            t.SetHighlighted(true, _reinforceHighlight);
            _highlightedTiles.Add(t);
        }
    }

    // -------------------------------------------------------------------------
    // ATTACK
    // -------------------------------------------------------------------------

    private void StartAttack()
    {
        CurrentState = GameState.Attack;
        ClearSelection();
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowPhaseTransition("ATTACK", UIManager.GetClanColor(CurrentPlayerId));
        UIManager.Instance?.ShowMessage("ATTACK: Select a territory with 2+ troops. Press End Attack when done.");
    }

    private void HandleAttackClick(Tile clicked)
    {
        if (_selectedTile == null)
        {
            if (clicked.OwnerPlayerId != CurrentPlayerId) return;
            if (clicked.CubeCount < 2) return;
            if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;
            _selectedTile = clicked; _selectedTile.SetSelected(true);
            HighlightAttackTargets();
            UIManager.Instance?.ShowMessage($"Selected: {clicked.CubeCount} troops. Click adjacent enemy territory.");
            return;
        }

        if (clicked == _selectedTile)
        {
            ClearSelection();
            UIManager.Instance?.ShowMessage("ATTACK: Select a territory with 2+ troops. Press End Attack when done.");
            return;
        }

        if (clicked.OwnerPlayerId == CurrentPlayerId)
        {
            ClearSelection();
            if (clicked.CubeCount >= 2 && clicked.Terrain != TerrainType.Water && clicked.Terrain != TerrainType.Mountain)
            {
                _selectedTile = clicked; _selectedTile.SetSelected(true);
                HighlightAttackTargets();
                UIManager.Instance?.ShowMessage($"Selected: {clicked.CubeCount} troops. Click adjacent enemy territory.");
            }
            return;
        }

        if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;
        if (!HexGridManager.Instance.GetNeighbors(_selectedTile.HexCoordinates).Contains(clicked))
        {
            UIManager.Instance?.ShowMessage("Target must be adjacent to the selected territory.");
            return;
        }

        BeginCombatSetup(_selectedTile, clicked);
    }

    private void HighlightAttackTargets()
    {
        ClearHighlights();
        foreach (Tile n in HexGridManager.Instance.GetNeighbors(_selectedTile.HexCoordinates))
        {
            if (n.OwnerPlayerId == CurrentPlayerId) continue;
            if (n.Terrain == TerrainType.Water || n.Terrain == TerrainType.Mountain) continue;
            n.SetHighlighted(true, _attackHighlight);
            _highlightedTiles.Add(n);
        }
    }

    private void BeginCombatSetup(Tile attacker, Tile defender)
    {
        _pendingAttacker = attacker;
        _pendingDefender = defender;
        int maxAtk = Mathf.Min(3, attacker.CubeCount - 1);
        int maxDef = Mathf.Max(1, Mathf.Min(2, defender.CubeCount));
        CurrentState = GameState.CombatSetup;
        ClearHighlights();
        string defName = defender.OwnerPlayerId >= 0 ? ClanNames[defender.OwnerPlayerId] : "Neutral";
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowCombatSetup(
            CurrentPlayerId, defender.OwnerPlayerId,
            maxAtk, maxDef,
            ClanNames[CurrentPlayerId], defName);
    }

    public void OnCombatSetupConfirm(int atkDice, int defDice)
    {
        if (CurrentState != GameState.CombatSetup || _pendingAttacker == null || _pendingDefender == null) return;

        Tile attacker = _pendingAttacker;
        Tile defender = _pendingDefender;
        int  prevOwner = defender.OwnerPlayerId;

        attacker.PlayAttackAnimation();
        defender.PlayAttackAnimation();

        bool conquered = CombatManager.Instance.ResolveCombat(attacker, defender, atkDice, defDice);

        if (conquered)
        {
            defender.OwnerPlayerId = CurrentPlayerId;
            defender.TriggerConquestPulse();
            AudioManager.Instance?.PlayConquerSound();
            HexGridManager.Instance.RefreshAllBorders();
        }
        else AudioManager.Instance?.PlayCombatSound();

        attacker.UpdateVisuals();
        defender.UpdateVisuals();

        UIManager.Instance?.HideCombatSetup();
        UIManager.Instance?.ShowCombatResult(
            CombatManager.Instance.LastResult,
            CurrentPlayerId, prevOwner,
            ClanNames[CurrentPlayerId],
            prevOwner >= 0 ? ClanNames[prevOwner] : "Neutral");
        UIManager.Instance?.UpdateUI();

        _pendingAttacker = null;
        _pendingDefender = null;

        StartCoroutine(HandlePostCombatRoutine(conquered, prevOwner, attacker, defender));
    }

    private System.Collections.IEnumerator HandlePostCombatRoutine(bool conquered, int prevOwner, Tile attacker, Tile defender)
    {
        // Wait for the dice animation to finish (approx 4.2 seconds)
        yield return new WaitForSeconds(4.2f);

        if (conquered)
        {
            bool win = CheckPlayerEliminated(prevOwner) || CheckWinCondition();
            if (win) yield break;
            _postConquestSource = attacker;
            _postConquestDest   = defender;
            BeginPostConquestMove();
        }
        else
        {
            ClearSelection();
            CurrentState = GameState.Attack;
            UIManager.Instance?.UpdateUI();
            UIManager.Instance?.ShowMessage("ATTACK: Select another territory or press End Attack.");
        }
    }

    public void OnCombatSetupCancel()
    {
        if (CurrentState != GameState.CombatSetup) return;
        _pendingAttacker = null;
        _pendingDefender = null;
        CurrentState = GameState.Attack;
        UIManager.Instance?.HideCombatSetup();
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowMessage("Combat cancelled. Select another territory or press End Attack.");
        ClearSelection();
    }

    private void BeginPostConquestMove()
    {
        int max = _postConquestSource.CubeCount - 1;
        if (max <= 0)
        {
            _postConquestSource = null; _postConquestDest = null;
            CurrentState = GameState.Attack;
            ClearSelection();
            UIManager.Instance?.UpdateUI();
            UIManager.Instance?.ShowMessage("ATTACK: Select another territory or press End Attack.");
            return;
        }
        CurrentState = GameState.PostConquest;
        ClearSelection();
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowMigrationCount(max, max, "Move extra troops (optional)");
        UIManager.Instance?.ShowMessage($"ADVANCE: Move extra troops to conquered territory (1–{max}), or Cancel to skip.");
    }

    public void OnMigrationCountConfirm(int count)
    {
        if (CurrentState == GameState.PostConquest)
        {
            UIManager.Instance?.HideMigrationCount();
            if (_postConquestSource != null && _postConquestDest != null)
                StartCoroutine(ExecutePostConquestAdvance(count));
            else
            {
                _postConquestSource = null; _postConquestDest = null;
                CurrentState = GameState.Attack;
                UIManager.Instance?.UpdateUI();
                UIManager.Instance?.ShowMessage("ATTACK: Select another territory or press End Attack.");
            }
            return;
        }

        if (CurrentState == GameState.Fortify && _fortifySource != null && _fortifyDest != null)
        {
            UIManager.Instance?.HideMigrationCount();
            StartCoroutine(ExecuteFortifyMove(count));
        }
    }

    private IEnumerator ExecutePostConquestAdvance(int count)
    {
        Tile source  = _postConquestSource;
        Tile dest    = _postConquestDest;
        int  advance = Mathf.Clamp(count, 1, source.CubeCount - 1);
        source.PlayWalkAnimation();
        yield return source.SpawnTraveler(source.transform.position, dest.transform.position, Tile.TravelDuration);
        source.RemoveCubes(advance);
        dest.AddCubes(advance);
        source.UpdateVisuals();
        dest.UpdateVisuals();
        dest.PlayWalkAnimation();
        _postConquestSource = null;
        _postConquestDest   = null;
        CurrentState = GameState.Attack;
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowMessage("ATTACK: Select another territory or press End Attack.");
    }

    private IEnumerator ExecuteFortifyMove(int count)
    {
        Tile source = _fortifySource;
        Tile dest   = _fortifyDest;
        int  actual = Mathf.Clamp(count, 1, source.CubeCount - 1);
        source.PlayWalkAnimation();
        yield return source.SpawnTraveler(source.transform.position, dest.transform.position, Tile.TravelDuration);
        source.RemoveCubes(actual);
        dest.AddCubes(actual);
        source.UpdateVisuals();
        dest.UpdateVisuals();
        dest.PlayWalkAnimation();
        _fortifyUsed     = true;
        _fortifySource   = null;
        _fortifyDest     = null;
        _waitingForCount = false;
        ClearSelection();
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowMessage("Fortify complete. Press End Turn.");
    }

    public void OnMigrationCountCancel()
    {
        if (CurrentState == GameState.PostConquest)
        {
            _postConquestSource = null; _postConquestDest = null;
            CurrentState = GameState.Attack;
            ClearSelection();
            UIManager.Instance?.HideMigrationCount();
            UIManager.Instance?.UpdateUI();
            UIManager.Instance?.ShowMessage("ATTACK: Select another territory or press End Attack.");
            return;
        }
        if (CurrentState == GameState.Fortify)
        {
            _fortifySource   = null;
            _fortifyDest     = null;
            _waitingForCount = false;
            ClearSelection();
            UIManager.Instance?.HideMigrationCount();
            UIManager.Instance?.UpdateUI();
            UIManager.Instance?.ShowMessage("FORTIFY: Select a territory to move troops. Press End Turn to skip.");
        }
    }

    // -------------------------------------------------------------------------
    // FORTIFY
    // -------------------------------------------------------------------------

    private void StartFortify()
    {
        CurrentState  = GameState.Fortify;
        _fortifyUsed  = false;
        _fortifySource = null;
        ClearSelection();
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowPhaseTransition("FORTIFY", UIManager.GetClanColor(CurrentPlayerId));
        UIManager.Instance?.ShowMessage("FORTIFY: Move troops between connected territories, or press End Turn.");
    }

    private void HandleFortifyClick(Tile clicked)
    {
        if (_fortifyUsed) return;

        if (_fortifySource == null)
        {
            if (clicked.OwnerPlayerId != CurrentPlayerId) return;
            if (clicked.CubeCount < 2) return;
            if (clicked.Terrain == TerrainType.Water || clicked.Terrain == TerrainType.Mountain) return;
            _fortifySource = clicked;
            _selectedTile  = clicked; _selectedTile.SetSelected(true);
            HighlightFortifyTargets();
            UIManager.Instance?.ShowMessage("FORTIFY: Click the destination territory.");
            return;
        }

        if (clicked == _fortifySource) { _fortifySource = null; ClearSelection(); UIManager.Instance?.ShowMessage("FORTIFY: Select a source territory. Press End Turn to skip."); return; }

        if (!_highlightedTiles.Contains(clicked)) { _fortifySource = null; ClearSelection(); return; }

        _fortifyDest     = clicked;
        _waitingForCount = true;
        int max = _fortifySource.CubeCount - 1;
        ClearHighlights();
        UIManager.Instance?.ShowMigrationCount(max, max, "Fortify");
        UIManager.Instance?.ShowMessage($"How many troops to move? (1–{max})");
    }

    private void HighlightFortifyTargets()
    {
        ClearHighlights();
        foreach (Tile t in GetConnectedOwn(_fortifySource))
        {
            t.SetHighlighted(true, _fortifyHighlight);
            _highlightedTiles.Add(t);
        }
    }

    private HashSet<Tile> GetConnectedOwn(Tile source)
    {
        HashSet<Tile> visited = new HashSet<Tile> { source };
        Queue<Tile>   queue   = new Queue<Tile>();
        queue.Enqueue(source);
        while (queue.Count > 0)
        {
            Tile cur = queue.Dequeue();
            foreach (Tile n in HexGridManager.Instance.GetNeighbors(cur.HexCoordinates))
            {
                if (visited.Contains(n)) continue;
                if (n.OwnerPlayerId != CurrentPlayerId) continue;
                if (n.Terrain == TerrainType.Water || n.Terrain == TerrainType.Mountain) continue;
                visited.Add(n); queue.Enqueue(n);
            }
        }
        visited.Remove(source);
        return visited;
    }

    // -------------------------------------------------------------------------
    // TURN / WIN
    // -------------------------------------------------------------------------

    public void OnEndTurnButtonClicked()
    {
        ResetIdleTimer();
        if (CurrentState == GameState.InitialPlacement) { OnRandomizePlacement(); return; }
        if (CurrentState == GameState.Attack)  { StartFortify(); return; }
        if (CurrentState == GameState.Fortify) { EndTurn(); return; }
    }

    private void EndTurn()
    {
        ClearSelection();
        int next = (CurrentPlayerId + 1) % PlayerCount;
        for (int i = 0; i < PlayerCount; i++)
        {
            if (HasAnyTile(next)) break;
            next = (next + 1) % PlayerCount;
        }
        CurrentPlayerId = next;
        StartReinforce();
    }

    private bool HasAnyTile(int pid)
    {
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.OwnerPlayerId == pid) return true;
        return false;
    }

    private bool CheckPlayerEliminated(int candidate)
    {
        if (candidate < 0 || candidate == CurrentPlayerId || HasAnyTile(candidate)) return false;
        UIManager.Instance?.ShowMessage($"{ClanNames[candidate]} has been eliminated!");
        for (int pid = 0; pid < PlayerCount; pid++)
        {
            if (_playerMissions[pid]?.Type == MissionType.DestroyPlayer &&
                _playerMissions[pid].TargetPlayer == candidate)
            { TriggerGameOver(pid); return true; }
        }
        return false;
    }

    private bool CheckWinCondition()
    {
        for (int pid = 0; pid < PlayerCount; pid++)
            if (MissionComplete(pid)) { TriggerGameOver(pid); return true; }
        return false;
    }

    private bool MissionComplete(int pid)
    {
        Mission m = _playerMissions[pid];
        if (m == null) return false;
        switch (m.Type)
        {
            case MissionType.ControlTerritories:
                int cnt = 0;
                foreach (Tile t in HexGridManager.Instance.Grid.Values)
                    if (t.OwnerPlayerId == pid) cnt++;
                return cnt >= m.TargetCount;
            case MissionType.DestroyPlayer:
                return !HasAnyTile(m.TargetPlayer);
            case MissionType.ControlTwoRegions:
                if (m.TargetRegions == null) return false;
                foreach (int r in m.TargetRegions)
                    if (!PlayerControlsRegion(pid, r)) return false;
                return true;
        }
        return false;
    }

    private void TriggerGameOver(int winnerId)
    {
        CurrentState = GameState.GameOver;
        ClearSelection();
        UIManager.Instance?.UpdateUI();
        UIManager.Instance?.ShowGameOver(winnerId, _playerMissions[winnerId]);
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    private void ClearHighlights()
    {
        foreach (Tile t in _highlightedTiles) if (t != null) t.ClearHighlight();
        _highlightedTiles.Clear();
    }

    private void ClearSelection()
    {
        if (_selectedTile != null) { _selectedTile.SetSelected(false); _selectedTile.UpdateVisuals(); _selectedTile = null; }
        _fortifySource   = null;
        _fortifyDest     = null;
        _waitingForCount = false;
        ClearHighlights();
    }

    // -------------------------------------------------------------------------
    // MENU / RESTART
    // -------------------------------------------------------------------------

    public void StartGame()
    {
        if (!_gridReady) return;
        InitializeGame();
    }

    public void BeginActualGame()
    {
        UIManager.Instance?.HideMenuScreens();
        BeginInitialPlacement();
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        CurrentState = GameState.MainMenu;
        ClearSelection();
        UIManager.Instance?.ShowMainMenu();
    }

    // -------------------------------------------------------------------------

    public string GetClanName(int pid) => pid >= 0 && pid < PlayerCount ? ClanNames[pid] : "Neutral";
    public int    GetExtraTroopsLeft(int pid) => _extraTroopsLeft[pid];
    public Mission GetPlayerMission(int pid) => pid >= 0 && pid < PlayerCount ? _playerMissions[pid] : null;
}
