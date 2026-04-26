using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Colour palette
    // -------------------------------------------------------------------------
    private static readonly Color _clan0Color  = new Color(0.20f, 0.40f, 1.00f);
    private static readonly Color _clan1Color  = new Color(1.00f, 0.28f, 0.28f);
    private static readonly Color _clan2Color  = new Color(0.15f, 0.80f, 0.30f);
    private static readonly Color _clan3Color  = new Color(0.75f, 0.20f, 0.90f);
    private static readonly Color _panelColor  = new Color(0.04f, 0.07f, 0.13f, 0.88f);
    private static readonly Color _buttonColor = new Color(0.15f, 0.55f, 0.15f);
    private static readonly Color _white       = Color.white;
    private static readonly Color _goldColor   = new Color(0.95f, 0.78f, 0.28f);
    private static readonly Color _parchment   = new Color(0.97f, 0.92f, 0.78f);

    // -------------------------------------------------------------------------
    // UI element references (built in Awake)
    // -------------------------------------------------------------------------
    private TextMeshProUGUI _playerTurnText;
    private TextMeshProUGUI _phaseText;
    private TextMeshProUGUI _messageText;

    // Mission panel
    private GameObject      _missionPanel;
    private TextMeshProUGUI _missionLabel;
    private TextMeshProUGUI _missionProgress;

    // Placement panel
    private GameObject      _placementPanel;
    private TextMeshProUGUI _placementInfoText;

    // Combat result + dice animation
    private GameObject          _combatResultPanel;
    private TextMeshProUGUI     _combatResultText;
    private Coroutine           _combatHideCoroutine;
    private GameObject          _diceCombatPanel;
    private Coroutine           _diceAnimCoroutine;

    // Phase banner
    private GameObject          _phaseBannerPanel;
    private TextMeshProUGUI     _phaseBannerText;
    private Coroutine           _phaseBannerCoroutine;

    // End-turn / end-attack button
    private Button              _endTurnButton;
    private TextMeshProUGUI     _endTurnLabel;

    // Game-over overlay
    private GameObject          _gameOverPanel;
    private TextMeshProUGUI     _gameOverText;

    // Tooltip
    private GameObject          _tooltipPanel;
    private TextMeshProUGUI     _tooltipNameText;
    private TextMeshProUGUI     _tooltipTerrainText;
    private TextMeshProUGUI     _tooltipOwnerText;
    private TextMeshProUGUI     _tooltipWarriorsText;
    private TextMeshProUGUI     _tooltipRegionText;

    // Combat setup
    private GameObject          _combatSetupPanel;
    private Button[]            _atkDiceButtons;
    private Button[]            _defDiceButtons;
    private TextMeshProUGUI     _atkDiceLabel;
    private TextMeshProUGUI     _defDiceLabel;
    private int                 _selectedAtkDice;
    private int                 _selectedDefDice;
    private int                 _atkDiceMax;
    private int                 _defDiceMax;
    private TextMeshProUGUI     _atkHeader;
    private TextMeshProUGUI     _defHeader;
    private string              _cachedAtkClan = "Attacker";
    private string              _cachedDefClan = "Defender";

    // Troop count picker (post-conquest advance + fortify)
    private GameObject          _migrationCountPanel;
    private TextMeshProUGUI     _migrationCountTitle;
    private TextMeshProUGUI     _migrationCountText;
    private int                 _migrationCount;
    private int                 _migrationCountMax;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.one;

        EnsureCanvasComponents();
        EnsureEventSystem();
        BuildHUD();
    }

    private void EnsureCanvasComponents()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        canvas.sortingOrder = 10; // stay above 3D geometry
        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler s = gameObject.AddComponent<CanvasScaler>();
            s.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1280, 720);
            s.matchWidthOrHeight  = 0.5f;
        }
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    // Unity UI requires an EventSystem to process button clicks.
    // If none exists in the scene (common when everything is built in code),
    // create one and attach the correct input module for the project's input backend.
    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

        GameObject esGo = new GameObject("EventSystem");
        DontDestroyOnLoad(esGo);
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();

        // Prefer New Input System module; fall back to legacy Standalone if unavailable.
        System.Type nisType = System.Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (nisType != null)
            esGo.AddComponent(nisType);
        else
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    // -------------------------------------------------------------------------
    // HUD construction
    // -------------------------------------------------------------------------

    private void BuildHUD()
    {
        BuildTopBar();
        BuildMissionPanel();
        BuildPlacementPanel();
        BuildEndTurnButton();
        BuildCombatResultPanel();
        BuildDiceCombatPanel();
        BuildGameOverPanel();
        BuildTooltipPanel();
        BuildCombatSetupPanel();
        BuildMigrationCountPanel();
        BuildPhaseBanner();
    }

    public static Color GetClanColor(int playerId) => playerId switch
    {
        0 => _clan0Color,
        1 => _clan1Color,
        2 => _clan2Color,
        3 => _clan3Color,
        _ => new Color(0.55f, 0.55f, 0.55f),
    };

    private void BuildTopBar()
    {
        GameObject panel = CreatePanel("TopBar", gameObject,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -50f), new Vector2(0f, 100f));
        AddGoldTrim(panel, 2f);

        _playerTurnText = CreateTMPLabel("PlayerTurnText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 25f), new Vector2(700f, 40f),
            "Argyn's Turn", 22, FontStyles.Bold);

        _phaseText = CreateTMPLabel("PhaseText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -2f), new Vector2(600f, 30f),
            "Phase: Reinforce", 16, FontStyles.Normal);

        _messageText = CreateTMPLabel("MessageText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -26f), new Vector2(800f, 24f),
            "Initialising...", 14, FontStyles.Italic);
        _messageText.color = new Color(0.85f, 0.85f, 0.85f);
    }

    // Bottom-right: mission panel
    private void BuildMissionPanel()
    {
        _missionPanel = CreatePanel("MissionPanel", gameObject,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-155f, 110f), new Vector2(300f, 200f));
        AddGoldTrim(_missionPanel, 2f);

        TextMeshProUGUI title = CreateTMPLabel("MissionTitle", _missionPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -16f), new Vector2(280f, 24f),
            "SECRET MISSION", 13, FontStyles.Bold);
        title.color = _goldColor;

        _missionLabel = CreateTMPLabel("MissionLabel", _missionPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 30f), new Vector2(280f, 80f),
            "", 13, FontStyles.Normal);
        _missionLabel.alignment = TextAlignmentOptions.Center;
        _missionLabel.color     = _parchment;

        _missionProgress = CreateTMPLabel("MissionProgress", _missionPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 20f), new Vector2(280f, 40f),
            "", 12, FontStyles.Italic);
        _missionProgress.color = new Color(0.75f, 0.95f, 0.75f);
    }

    // Center-bottom: initial placement panel
    private void BuildPlacementPanel()
    {
        _placementPanel = CreatePanel("PlacementPanel", gameObject,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 130f), new Vector2(440f, 110f));
        _placementPanel.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.04f, 0.90f);
        AddGoldTrim(_placementPanel, 2f);

        _placementInfoText = CreateTMPLabel("PlacementInfo", _placementPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -18f), new Vector2(420f, 24f),
            "Place your starting troops", 14, FontStyles.Bold);

        CreateButton("RandomizeBtn", _placementPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-110f, 28f), new Vector2(180f, 44f),
            "Randomize for me", new Color(0.25f, 0.40f, 0.70f),
            () => GameManager.Instance?.OnRandomizePlacement());

        CreateButton("RandomizeAllBtn", _placementPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(110f, 28f), new Vector2(180f, 44f),
            "Auto-fill all", new Color(0.50f, 0.25f, 0.70f),
            () => GameManager.Instance?.OnRandomizeAll());

        _placementPanel.SetActive(false);
    }

    private void BuildEndTurnButton()
    {
        GameObject btnGo = new GameObject("EndTurnButton");
        btnGo.transform.SetParent(gameObject.transform, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 50f);
        rt.sizeDelta = new Vector2(220f, 60f);
        Image img = btnGo.AddComponent<Image>();
        img.color = _buttonColor;
        Button btn = btnGo.AddComponent<Button>();
        _endTurnButton = btn;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.25f, 0.70f, 0.25f);
        cb.pressedColor     = new Color(0.10f, 0.40f, 0.10f);
        cb.disabledColor    = new Color(0.35f, 0.35f, 0.35f);
        btn.colors = cb;
        btn.onClick.AddListener(() => GameManager.Instance?.OnEndTurnButtonClicked());

        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(btnGo.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
        _endTurnLabel = textGo.AddComponent<TextMeshProUGUI>();
        _endTurnLabel.text = "End Turn"; _endTurnLabel.fontSize = 20;
        _endTurnLabel.fontStyle = FontStyles.Bold; _endTurnLabel.color = _white;
        _endTurnLabel.alignment = TextAlignmentOptions.Center;
    }

    private void BuildCombatResultPanel()
    {
        _combatResultPanel = CreatePanel("CombatResultPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500f, 160f));
        _combatResultText = CreateTMPLabel("CombatText", _combatResultPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(460f, 140f), "", 18, FontStyles.Bold);
        _combatResultText.alignment = TextAlignmentOptions.Center;
        _combatResultPanel.SetActive(false);
    }

    private void BuildDiceCombatPanel()
    {
        _diceCombatPanel = CreatePanel("DiceCombatPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 220f));
        AddGoldTrim(_diceCombatPanel, 2.5f);
        _diceCombatPanel.SetActive(false);
    }

    private void BuildGameOverPanel()
    {
        _gameOverPanel = CreatePanel("GameOverPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(620f, 340f));
        _gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);
        AddGoldTrim(_gameOverPanel, 3f);

        _gameOverText = CreateTMPLabel("GameOverText", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 60f), new Vector2(580f, 140f), "Game Over", 30, FontStyles.Bold);
        _gameOverText.alignment = TextAlignmentOptions.Center;

        CreateButton("RestartButton", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -90f), new Vector2(200f, 55f),
            "Play Again", new Color(0.60f, 0.10f, 0.10f),
            () => UnityEngine.SceneManagement.SceneManager.LoadScene(0));

        _gameOverPanel.SetActive(false);
    }

    private void BuildTooltipPanel()
    {
        _tooltipPanel = CreatePanel("TooltipPanel", gameObject,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(130f, 90f), new Vector2(260f, 150f));
        AddGoldTrim(_tooltipPanel, 1.5f);

        _tooltipNameText = CreateTMPLabel("TooltipName", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(240f, 22f), "", 14, FontStyles.Bold);
        _tooltipNameText.color = _goldColor;
        _tooltipNameText.alignment = TextAlignmentOptions.Left;

        _tooltipRegionText = CreateTMPLabel("TooltipRegion", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -33f), new Vector2(240f, 18f), "", 11, FontStyles.Italic);
        _tooltipRegionText.color = _parchment;
        _tooltipRegionText.alignment = TextAlignmentOptions.Left;

        _tooltipTerrainText = CreateTMPLabel("TooltipTerrain", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -54f), new Vector2(240f, 22f), "", 13, FontStyles.Bold);
        _tooltipTerrainText.alignment = TextAlignmentOptions.Left;

        _tooltipOwnerText = CreateTMPLabel("TooltipOwner", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -76f), new Vector2(240f, 20f), "", 12, FontStyles.Normal);
        _tooltipOwnerText.alignment = TextAlignmentOptions.Left;

        _tooltipWarriorsText = CreateTMPLabel("TooltipWarriors", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -98f), new Vector2(240f, 20f), "", 12, FontStyles.Normal);
        _tooltipWarriorsText.alignment = TextAlignmentOptions.Left;

        _tooltipPanel.SetActive(false);
    }

    private void BuildPhaseBanner()
    {
        _phaseBannerPanel = CreatePanel("PhaseBanner", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(480f, 80f));
        _phaseBannerPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        _phaseBannerPanel.GetComponent<Image>().raycastTarget = false; // purely visual — must not block modal clicks

        _phaseBannerText = CreateTMPLabel("BannerText", _phaseBannerPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(460f, 70f), "", 38, FontStyles.Bold);
        _phaseBannerText.alignment = TextAlignmentOptions.Center;
        _phaseBannerPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        int   pid   = GameManager.Instance.CurrentPlayerId;
        Color color = GetClanColor(pid);

        if (_playerTurnText != null)
        {
            _playerTurnText.text  = $"{GameManager.Instance.GetClanName(pid)}'s Turn";
            _playerTurnText.color = color;
        }

        if (_phaseText != null)
        {
            _phaseText.text = GameManager.Instance.CurrentState switch
            {
                GameState.InitialPlacement => "Phase: Placement",
                GameState.Reinforce        => "Phase: Reinforce",
                GameState.Attack           => "Phase: Attack",
                GameState.CombatSetup      => "Phase: Combat",
                GameState.PostConquest     => "Phase: Advance",
                GameState.Fortify          => "Phase: Fortify",
                GameState.GameOver         => "Game Over",
                _                          => "",
            };
        }

        // End-turn button
        if (_endTurnButton != null)
        {
            GameState s = GameManager.Instance.CurrentState;
            _endTurnButton.interactable = s == GameState.Attack || s == GameState.Fortify;
            if (_endTurnLabel != null)
                _endTurnLabel.text = s == GameState.Attack ? "End Attack" : "End Turn";
        }

        // Placement panel
        if (_placementPanel != null)
        {
            bool placing = GameManager.Instance.CurrentState == GameState.InitialPlacement;
            _placementPanel.SetActive(placing);
            if (placing && _placementInfoText != null)
                _placementInfoText.text = $"{GameManager.Instance.GetClanName(pid)}: place troops — {GameManager.Instance.GetExtraTroopsLeft(pid)} left";
        }

        // Mission panel — always show current player's mission
        if (_missionPanel != null)
        {
            Mission m = GameManager.Instance.GetPlayerMission(pid);
            _missionPanel.SetActive(m != null && GameManager.Instance.CurrentState != GameState.GameOver);
            if (m != null && _missionLabel != null)
            {
                _missionLabel.text = m.Description;
                if (_missionProgress != null)
                    _missionProgress.text = GetMissionProgress(pid, m);
            }
        }
    }

    private string GetMissionProgress(int pid, Mission m)
    {
        if (HexGridManager.Instance == null) return "";
        switch (m.Type)
        {
            case MissionType.ControlTerritories:
                int cnt = 0;
                foreach (Tile t in HexGridManager.Instance.Grid.Values)
                    if (t.OwnerPlayerId == pid) cnt++;
                return $"Progress: {cnt} / {m.TargetCount} territories";
            case MissionType.DestroyPlayer:
                bool alive = false;
                foreach (Tile t in HexGridManager.Instance.Grid.Values)
                    if (t.OwnerPlayerId == m.TargetPlayer) { alive = true; break; }
                return alive ? "Target: still active" : "Target: eliminated!";
            case MissionType.ControlTwoRegions:
                if (m.TargetRegions == null) return "";
                int controlled = 0;
                foreach (int rid in m.TargetRegions)
                    if (PlayerControlsRegion(pid, rid)) controlled++;
                return $"Regions: {controlled} / {m.TargetRegions.Length} controlled";
        }
        return "";
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

    public void ShowMessage(string message)
    {
        if (_messageText != null) _messageText.text = message;
    }

    public void ShowPhaseTransition(string label, Color color)
    {
        if (_phaseBannerPanel == null) return;
        if (_phaseBannerCoroutine != null) StopCoroutine(_phaseBannerCoroutine);
        _phaseBannerCoroutine = StartCoroutine(PhaseBannerCoroutine(label, color));
    }

    private void DismissPhaseBanner()
    {
        if (_phaseBannerCoroutine != null) { StopCoroutine(_phaseBannerCoroutine); _phaseBannerCoroutine = null; }
        if (_phaseBannerPanel != null) _phaseBannerPanel.SetActive(false);
    }

    private IEnumerator PhaseBannerCoroutine(string label, Color color)
    {
        _phaseBannerText.text  = label;
        _phaseBannerText.color = color;
        _phaseBannerPanel.SetActive(true);
        float t = 0f;
        while (t < 0.18f) { t += Time.deltaTime; _phaseBannerText.alpha = t / 0.18f; yield return null; }
        _phaseBannerText.alpha = 1f;
        yield return new WaitForSeconds(0.75f);
        t = 0f;
        while (t < 0.25f) { t += Time.deltaTime; _phaseBannerText.alpha = 1f - t / 0.25f; yield return null; }
        _phaseBannerPanel.SetActive(false);
    }

    // Tooltip
    public void ShowTooltip(Tile tile)
    {
        if (_tooltipPanel == null || tile == null) return;

        bool hasName = !string.IsNullOrEmpty(tile.TerritoryName);
        _tooltipNameText.text = hasName ? tile.TerritoryName : (tile.Terrain.ToString());
        _tooltipNameText.color = hasName ? _goldColor : _parchment;

        string regionName = tile.ContinentId >= 0 && tile.ContinentId < GameManager.RegionNames.Length
            ? GameManager.RegionNames[tile.ContinentId] : "";
        _tooltipRegionText.gameObject.SetActive(!string.IsNullOrEmpty(regionName));
        _tooltipRegionText.text = regionName;

        string terrain = tile.Terrain switch
        {
            TerrainType.Plain    => "Plain",
            TerrainType.Mountain => "Mountain",
            TerrainType.Water    => "Water",
            _                    => tile.Terrain.ToString(),
        };
        if (tile.HasForest) terrain += " (Forest)";
        _tooltipTerrainText.text  = terrain;
        _tooltipTerrainText.color = _white;

        int pid = tile.OwnerPlayerId;
        if (pid >= 0 && pid < GameManager.PlayerCount)
        {
            _tooltipOwnerText.text  = $"Owner: {GameManager.Instance?.GetClanName(pid) ?? $"Player {pid}"}";
            _tooltipOwnerText.color = GetClanColor(pid);
        }
        else
        {
            _tooltipOwnerText.text  = "Owner: Neutral";
            _tooltipOwnerText.color = new Color(0.65f, 0.65f, 0.65f);
        }

        bool showTroops = tile.Terrain != TerrainType.Water && tile.CubeCount > 0;
        _tooltipWarriorsText.text  = showTroops ? $"Troops: {tile.CubeCount}" : "Troops: —";
        _tooltipWarriorsText.color = _white;

        _tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (_tooltipPanel != null) _tooltipPanel.SetActive(false);
    }

    // Combat result (plain text — legacy)
    public void ShowCombatResult(string message)
    {
        if (_combatResultPanel == null) return;
        _combatResultText.text = message;
        _combatResultPanel.SetActive(true);
        if (_combatHideCoroutine != null) StopCoroutine(_combatHideCoroutine);
        _combatHideCoroutine = StartCoroutine(HidePanelAfter(_combatResultPanel, 2f));
    }

    // Combat result with dice animation
    public void ShowCombatResult(CombatResult result,
                                 int attackerPlayerId, int defenderPlayerId,
                                 string attackerName, string defenderName)
    {
        if (_diceCombatPanel == null || result == null) return;
        if (_diceAnimCoroutine != null) StopCoroutine(_diceAnimCoroutine);
        foreach (Transform child in _diceCombatPanel.transform) Destroy(child.gameObject);
        _diceCombatPanel.SetActive(true);
        _diceAnimCoroutine = StartCoroutine(AnimateDicePanel(result, attackerPlayerId, defenderPlayerId, attackerName, defenderName));
    }

    private IEnumerator AnimateDicePanel(CombatResult result,
                                         int atkPid, int defPid,
                                         string atkName, string defName)
    {
        Color atkColor = GetClanColor(atkPid);
        Color defColor = GetClanColor(defPid);

        CreateTMPLabel("Header", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 85f), new Vector2(540f, 30f),
            $"{atkName} attacks {defName}!", 17, FontStyles.Bold);

        TextMeshProUGUI aLabel = CreateTMPLabel("AtkLabel", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, 58f), new Vector2(200f, 24f), atkName, 13, FontStyles.Bold);
        aLabel.color = atkColor;

        TextMeshProUGUI dLabel = CreateTMPLabel("DefLabel", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, 58f), new Vector2(200f, 24f), defName, 13, FontStyles.Bold);
        dLabel.color = defColor;

        const float dieSize = 52f, dieStep = 60f, diceRowY = 5f;
        int atkCount = result.AttackerRolls.Length, defCount = result.DefenderRolls.Length;

        TextMeshProUGUI[] atkLabels = new TextMeshProUGUI[atkCount];
        TextMeshProUGUI[] defLabels = new TextMeshProUGUI[defCount];

        for (int i = 0; i < atkCount; i++)
            atkLabels[i] = SpawnDie($"AtkDie{i}", _diceCombatPanel, -1, new Vector2(-(dieStep * 0.5f) - dieStep * i, diceRowY), atkColor);
        for (int i = 0; i < defCount; i++)
            defLabels[i] = SpawnDie($"DefDie{i}", _diceCombatPanel, -1, new Vector2((dieStep * 0.5f) + dieStep * (defCount - 1 - i), diceRowY), defColor);

        for (int c = 0; c < 10; c++)
        {
            foreach (var l in atkLabels) if (l) l.text = Random.Range(1, 7).ToString();
            foreach (var l in defLabels) if (l) l.text = Random.Range(1, 7).ToString();
            yield return new WaitForSeconds(0.07f);
        }

        for (int i = 0; i < atkCount; i++) if (atkLabels[i]) atkLabels[i].text = result.AttackerRolls[i].ToString();
        for (int i = 0; i < defCount; i++) if (defLabels[i]) defLabels[i].text = result.DefenderRolls[i].ToString();

        yield return new WaitForSeconds(0.15f);

        int atkUsed = result.AttackingDiceUsed > 0 ? result.AttackingDiceUsed : atkCount;
        int pairs   = Mathf.Min(atkUsed, defCount);
        for (int i = 0; i < pairs; i++)
        {
            int di = pairs - 1 - i;
            int av = result.AttackerRolls[i], dv = result.DefenderRolls[di];
            bool win = av > dv;
            string sym = av == dv ? "=" : win ? ">" : "<";
            Color  sc  = win ? new Color(0.2f, 0.85f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
            TextMeshProUGUI ind = CreateTMPLabel($"Ind{i}", _diceCombatPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-(dieStep * 0.5f) - dieStep * i, diceRowY + dieSize * 0.5f + 14f), new Vector2(46f, 22f),
                $"{sym} {dv}", 14, FontStyles.Bold);
            ind.color = sc;
        }

        TextMeshProUGUI summary = CreateTMPLabel("Summary", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -50f), new Vector2(520f, 28f),
            $"Attacker lost {result.AttackerLosses}  |  Defender lost {result.DefenderLosses}", 15, FontStyles.Normal);
        summary.color = new Color(0.9f, 0.9f, 0.9f);

        if (result.Conquered)
        {
            TextMeshProUGUI conq = CreateTMPLabel("Conquered", _diceCombatPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -83f), new Vector2(520f, 28f), "CONQUERED!", 18, FontStyles.Bold);
            conq.color = _goldColor;
        }

        yield return new WaitForSeconds(3f);
        if (_diceCombatPanel != null) _diceCombatPanel.SetActive(false);
    }

    private TextMeshProUGUI SpawnDie(string name, GameObject parent, int value, Vector2 position, Color clanColor)
    {
        const float dieSize = 52f;
        GameObject dieGo = new GameObject(name);
        dieGo.transform.SetParent(parent.transform, false);
        RectTransform rt = dieGo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position; rt.sizeDelta = new Vector2(dieSize, dieSize);
        Color bg = Color.Lerp(clanColor, Color.white, 0.35f); bg.a = 0.9f;
        dieGo.AddComponent<Image>().color = bg;

        GameObject lGo = new GameObject("Value"); lGo.transform.SetParent(dieGo.transform, false);
        RectTransform lrt = lGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text = value < 1 ? "?" : value.ToString(); tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold; tmp.color = _white; tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    private IEnumerator HidePanelAfter(GameObject panel, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (panel != null) panel.SetActive(false);
    }

    // Game over
    public void ShowGameOver(int winningPlayerId, Mission mission)
    {
        if (_gameOverPanel == null) return;
        string name = GameManager.Instance?.GetClanName(winningPlayerId) ?? $"Player {winningPlayerId}";
        string missionDesc = mission != null ? $"\nMission: {mission.Description}" : "";
        _gameOverText.text  = $"{name} is victorious!{missionDesc}\nThe steppes bow to your Batyr.";
        _gameOverText.color = GetClanColor(winningPlayerId);
        _gameOverPanel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Combat setup panel
    // -------------------------------------------------------------------------

    private void BuildCombatSetupPanel()
    {
        _combatSetupPanel = CreatePanel("CombatSetupPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 280f));
        _combatSetupPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.10f, 0.94f);
        AddGoldTrim(_combatSetupPanel, 2.5f);

        CreateTMPLabel("Title", _combatSetupPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(540f, 32f), "Combat Setup", 22, FontStyles.Bold);

        _atkHeader = CreateTMPLabel("AtkHeader", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, 75f), new Vector2(220f, 24f), "Attacker dice", 14, FontStyles.Bold);

        _atkDiceButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            int n = i + 1;
            _atkDiceButtons[i] = CreateButton($"AtkDice{n}", _combatSetupPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-180f + i * 50f, 30f), new Vector2(46f, 46f),
                n.ToString(), _buttonColor, () => SetAtkDice(n));
        }

        _atkDiceLabel = CreateTMPLabel("AtkLabel", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, -10f), new Vector2(220f, 22f), "Selected: 0", 13, FontStyles.Normal);

        _defHeader = CreateTMPLabel("DefHeader", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, 75f), new Vector2(220f, 24f), "Defender dice", 14, FontStyles.Bold);

        _defDiceButtons = new Button[2];
        for (int i = 0; i < 2; i++)
        {
            int n = i + 1;
            _defDiceButtons[i] = CreateButton($"DefDice{n}", _combatSetupPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(80f + i * 50f, 30f), new Vector2(46f, 46f),
                n.ToString(), _buttonColor, () => SetDefDice(n));
        }

        _defDiceLabel = CreateTMPLabel("DefLabel", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, -10f), new Vector2(220f, 22f), "Selected: 0", 13, FontStyles.Normal);

        CreateButton("ResolveBtn", _combatSetupPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-80f, 30f), new Vector2(140f, 44f),
            "Resolve", new Color(0.55f, 0.10f, 0.10f),
            () => GameManager.Instance?.OnCombatSetupConfirm(_selectedAtkDice, _selectedDefDice));

        CreateButton("CancelBtn", _combatSetupPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(80f, 30f), new Vector2(140f, 44f),
            "Cancel", new Color(0.30f, 0.30f, 0.30f),
            () => GameManager.Instance?.OnCombatSetupCancel());

        _combatSetupPanel.SetActive(false);
    }

    public void ShowCombatSetup(int atkPid, int defPid, int maxAtk, int maxDef, string atkClan, string defClan)
    {
        if (_combatSetupPanel == null) return;
        DismissPhaseBanner();
        _atkDiceMax = maxAtk; _defDiceMax = maxDef;
        _selectedAtkDice = maxAtk; _selectedDefDice = maxDef;
        for (int i = 0; i < _atkDiceButtons.Length; i++) _atkDiceButtons[i].interactable = (i + 1) <= maxAtk;
        for (int i = 0; i < _defDiceButtons.Length; i++) _defDiceButtons[i].interactable = (i + 1) <= maxDef;
        if (_atkHeader) _atkHeader.color = GetClanColor(atkPid);
        if (_defHeader) _defHeader.color = GetClanColor(defPid);
        _cachedAtkClan = atkClan; _cachedDefClan = defClan;
        RefreshCombatLabels();
        _combatSetupPanel.SetActive(true);
    }

    public void HideCombatSetup()
    {
        if (_combatSetupPanel != null) _combatSetupPanel.SetActive(false);
    }

    private void SetAtkDice(int n) { _selectedAtkDice = n; RefreshCombatLabels(); }
    private void SetDefDice(int n) { _selectedDefDice = n; RefreshCombatLabels(); }

    private void RefreshCombatLabels()
    {
        if (_atkDiceLabel) _atkDiceLabel.text = $"{_cachedAtkClan}: {_selectedAtkDice} / {_atkDiceMax}";
        if (_defDiceLabel) _defDiceLabel.text = $"{_cachedDefClan}: {_selectedDefDice} / {_defDiceMax}";
    }

    // -------------------------------------------------------------------------
    // Troop count picker
    // -------------------------------------------------------------------------

    private void BuildMigrationCountPanel()
    {
        _migrationCountPanel = CreatePanel("MigrationCountPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(420f, 220f));
        _migrationCountPanel.GetComponent<Image>().color = new Color(0.05f, 0.10f, 0.05f, 0.94f);
        AddGoldTrim(_migrationCountPanel, 2.5f);

        _migrationCountTitle = CreateTMPLabel("Title", _migrationCountPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(400f, 28f), "Move troops", 18, FontStyles.Bold);

        CreateButton("Minus", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-110f, 15f), new Vector2(54f, 54f),
            "-", _buttonColor, () => AdjustCount(-1));

        _migrationCountText = CreateTMPLabel("Count", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 15f), new Vector2(140f, 54f), "1", 26, FontStyles.Bold);

        CreateButton("Plus", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(110f, 15f), new Vector2(54f, 54f),
            "+", _buttonColor, () => AdjustCount(1));

        CreateButton("Confirm", _migrationCountPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-70f, 30f), new Vector2(120f, 40f),
            "Confirm", new Color(0.15f, 0.55f, 0.15f),
            () => GameManager.Instance?.OnMigrationCountConfirm(_migrationCount));

        CreateButton("Cancel", _migrationCountPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(70f, 30f), new Vector2(120f, 40f),
            "Cancel", new Color(0.30f, 0.30f, 0.30f),
            () => GameManager.Instance?.OnMigrationCountCancel());

        _migrationCountPanel.SetActive(false);
    }

    public void ShowMigrationCount(int max, int defaultCount, string title = "Move troops")
    {
        if (_migrationCountPanel == null) return;
        _migrationCountMax = Mathf.Max(1, max);
        _migrationCount    = Mathf.Clamp(defaultCount, 1, _migrationCountMax);
        if (_migrationCountTitle != null) _migrationCountTitle.text = title;
        UpdateCountText();
        DismissPhaseBanner();
        _migrationCountPanel.SetActive(true);
    }

    public void HideMigrationCount()
    {
        if (_migrationCountPanel != null) _migrationCountPanel.SetActive(false);
    }

    private void AdjustCount(int delta)
    {
        _migrationCount = Mathf.Clamp(_migrationCount + delta, 1, _migrationCountMax);
        UpdateCountText();
    }

    private void UpdateCountText()
    {
        if (_migrationCountText != null)
            _migrationCountText.text = $"{_migrationCount} / {_migrationCountMax}";
    }

    // -------------------------------------------------------------------------
    // Factory helpers
    // -------------------------------------------------------------------------

    private void AddGoldTrim(GameObject parent, float thickness)
    {
        AddTrimStrip(parent, "TrimTop",    new Vector2(0f,1f), new Vector2(1f,1f), new Vector2(0f, thickness));
        AddTrimStrip(parent, "TrimBottom", new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(0f, thickness));
        AddTrimStrip(parent, "TrimLeft",   new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(thickness, 0f));
        AddTrimStrip(parent, "TrimRight",  new Vector2(1f,0f), new Vector2(1f,1f), new Vector2(thickness, 0f));
    }

    private void AddTrimStrip(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = sizeDelta;
        Image img = go.AddComponent<Image>(); img.color = _goldColor; img.raycastTarget = false;
    }

    private GameObject CreatePanel(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        go.AddComponent<Image>().color = _panelColor;
        return go;
    }

    private TextMeshProUGUI CreateTMPLabel(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        string text, float fontSize, FontStyles style)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
        tmp.color = _white; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false; // text never needs to capture clicks
        return tmp;
    }

    private Button CreateButton(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        string label, Color bgColor, System.Action onClick)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        go.AddComponent<Image>().color = bgColor;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());
        ColorBlock cb = btn.colors; cb.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f); btn.colors = cb;

        GameObject lGo = new GameObject("Label"); lGo.transform.SetParent(go.transform, false);
        RectTransform lrt = lGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18; tmp.fontStyle = FontStyles.Bold;
        tmp.color = _white; tmp.alignment = TextAlignmentOptions.Center;
        return btn;
    }
}
