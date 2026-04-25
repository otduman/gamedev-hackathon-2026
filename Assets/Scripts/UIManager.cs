using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UIManager builds the entire HUD programmatically at runtime.
// Attach this script to a Canvas GameObject (HackathonAssetSetup does this automatically).
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Colour palette
    // -------------------------------------------------------------------------
    private static readonly Color _clan0Color  = new Color(0.20f, 0.40f, 1.00f);
    private static readonly Color _clan1Color  = new Color(1.00f, 0.30f, 0.20f);
    private static readonly Color _panelColor  = new Color(0.00f, 0.00f, 0.00f, 0.70f);
    private static readonly Color _buttonColor = new Color(0.15f, 0.55f, 0.15f);
    private static readonly Color _white       = Color.white;
    private static readonly Color _goldColor   = new Color(1.00f, 0.84f, 0.00f);

    // -------------------------------------------------------------------------
    // UI element references (built in Awake)
    // -------------------------------------------------------------------------
    private TextMeshProUGUI _playerTurnText;
    private TextMeshProUGUI _phaseText;
    private TextMeshProUGUI _messageText;
    private TextMeshProUGUI _clan0PrestigeText;
    private TextMeshProUGUI _clan1PrestigeText;

    private GameObject          _combatResultPanel;
    private TextMeshProUGUI     _combatResultText;
    private Coroutine           _combatHideCoroutine;

    // Visual dice panel
    private GameObject          _diceCombatPanel;
    private Coroutine           _diceHideCoroutine;

    private Button               _endTurnButton;

    private GameObject          _gameOverPanel;
    private TextMeshProUGUI     _gameOverText;

    private GameObject          _tooltipPanel;
    private TextMeshProUGUI     _tooltipTerrainText;
    private TextMeshProUGUI     _tooltipOwnerText;
    private TextMeshProUGUI     _tooltipWarriorsText;
    private TextMeshProUGUI     _tooltipYurtText;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Unity sometimes serialises the Canvas RectTransform with scale (0,0,0).
        // Force it to 1 here so the HUD is always visible regardless of scene state.
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.one;

        EnsureCanvasComponents();
        BuildHUD();
    }

    // Make sure the GameObject this is on has the required Canvas plumbing.
    private void EnsureCanvasComponents()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    // -------------------------------------------------------------------------
    // HUD construction
    // -------------------------------------------------------------------------

    private void BuildHUD()
    {
        BuildTopBar();
        BuildPrestigePanel();
        BuildEndTurnButton();
        BuildCombatResultPanel();
        BuildDiceCombatPanel();
        BuildGameOverPanel();
        BuildTooltipPanel();
    }

    // Top bar — turn, phase, and instruction message
    private void BuildTopBar()
    {
        GameObject panel = CreatePanel("TopBar", gameObject,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -50f), new Vector2(0f, 100f));

        _playerTurnText = CreateTMPLabel("PlayerTurnText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 25f), new Vector2(600f, 40f),
            "Player 1's Turn", 22, FontStyles.Bold);

        _phaseText = CreateTMPLabel("PhaseText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -2f), new Vector2(600f, 30f),
            "Phase: Reinforce", 16, FontStyles.Normal);

        _messageText = CreateTMPLabel("MessageText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -26f), new Vector2(700f, 24f),
            "Initialising...", 14, FontStyles.Italic);
        _messageText.color = new Color(0.85f, 0.85f, 0.85f);
    }

    // Bottom-right prestige panel
    private void BuildPrestigePanel()
    {
        GameObject panel = CreatePanel("PrestigePanel", gameObject,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-110f, 80f), new Vector2(220f, 120f));

        _clan0PrestigeText = CreateTMPLabel("Clan0Prestige", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 30f), new Vector2(200f, 30f),
            "Argyn Prestige: 0", 15, FontStyles.Bold);
        _clan0PrestigeText.color = _clan0Color;

        _clan1PrestigeText = CreateTMPLabel("Clan1Prestige", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(200f, 30f),
            "Naiman Prestige: 0", 15, FontStyles.Bold);
        _clan1PrestigeText.color = _clan1Color;

        CreateTMPLabel("TargetText", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -28f), new Vector2(200f, 24f),
            "First to 15 wins", 12, FontStyles.Normal);
    }

    // End Turn button — bottom center
    private void BuildEndTurnButton()
    {
        GameObject btnGo = new GameObject("EndTurnButton");
        btnGo.transform.SetParent(gameObject.transform, false);

        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 50f);
        rt.sizeDelta = new Vector2(200f, 60f);

        Image img = btnGo.AddComponent<Image>();
        img.color = _buttonColor;

        Button btn = btnGo.AddComponent<Button>();
        _endTurnButton = btn;
        ColorBlock cb = btn.colors;
        cb.highlightedColor  = new Color(0.25f, 0.70f, 0.25f);
        cb.pressedColor      = new Color(0.10f, 0.40f, 0.10f);
        cb.disabledColor     = new Color(0.35f, 0.35f, 0.35f);
        btn.colors = cb;

        // Wire click to GameManager — safe because GameManager.Instance is set in Awake.
        btn.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnEndTurnButtonClicked();
        });

        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(btnGo.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textGo.AddComponent<TextMeshProUGUI>();
        label.text      = "End Turn";
        label.fontSize  = 20;
        label.fontStyle = FontStyles.Bold;
        label.color     = _white;
        label.alignment = TextAlignmentOptions.Center;
    }

    // Combat result overlay — center screen, hidden until needed
    private void BuildCombatResultPanel()
    {
        _combatResultPanel = CreatePanel("CombatResultPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500f, 160f));

        _combatResultText = CreateTMPLabel("CombatText", _combatResultPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(460f, 140f),
            "", 18, FontStyles.Bold);
        _combatResultText.alignment = TextAlignmentOptions.Center;

        _combatResultPanel.SetActive(false);
    }

    // Visual dice combat panel — 560×220, built entirely in code, hidden until combat resolves
    private void BuildDiceCombatPanel()
    {
        _diceCombatPanel = CreatePanel("DiceCombatPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 220f));

        _diceCombatPanel.SetActive(false);
    }

    // Game-over overlay — center screen, hidden until a player wins
    private void BuildGameOverPanel()
    {
        _gameOverPanel = CreatePanel("GameOverPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(600f, 300f));
        (_gameOverPanel.GetComponent<Image>()).color = new Color(0f, 0f, 0f, 0.85f);

        _gameOverText = CreateTMPLabel("GameOverText", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 50f), new Vector2(560f, 100f),
            "Game Over", 32, FontStyles.Bold);
        _gameOverText.alignment = TextAlignmentOptions.Center;

        // Restart button inside the game-over panel
        GameObject restartGo = new GameObject("RestartButton");
        restartGo.transform.SetParent(_gameOverPanel.transform, false);

        RectTransform rrt = restartGo.AddComponent<RectTransform>();
        rrt.anchorMin = rrt.anchorMax = rrt.pivot = new Vector2(0.5f, 0.5f);
        rrt.anchoredPosition = new Vector2(0f, -70f);
        rrt.sizeDelta = new Vector2(200f, 55f);

        Image rImg = restartGo.AddComponent<Image>();
        rImg.color = new Color(0.60f, 0.10f, 0.10f);

        Button rBtn = restartGo.AddComponent<Button>();
        rBtn.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(0));

        GameObject rLabel = new GameObject("Label");
        rLabel.transform.SetParent(restartGo.transform, false);
        RectTransform rLabelRt = rLabel.AddComponent<RectTransform>();
        rLabelRt.anchorMin = Vector2.zero;
        rLabelRt.anchorMax = Vector2.one;
        rLabelRt.offsetMin = rLabelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI rText = rLabel.AddComponent<TextMeshProUGUI>();
        rText.text      = "Play Again";
        rText.fontSize  = 18;
        rText.fontStyle = FontStyles.Bold;
        rText.color     = _white;
        rText.alignment = TextAlignmentOptions.Center;

        _gameOverPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public API called by GameManager
    // -------------------------------------------------------------------------

    // Refreshes every HUD element from current GameManager state.
    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        int pid = GameManager.Instance.CurrentPlayerId;

        if (_playerTurnText != null)
        {
            _playerTurnText.text  = pid == 0 ? "Argyn's Turn" : "Naiman's Turn";
            _playerTurnText.color = pid == 0 ? _clan0Color : _clan1Color;
        }

        if (_phaseText != null)
        {
            switch (GameManager.Instance.CurrentState)
            {
                case GameState.Reinforce:      _phaseText.text = "Phase: Reinforce";  break;
                case GameState.Action:         _phaseText.text = "Phase: Action";     break;
                case GameState.MigrationCheck: _phaseText.text = "Phase: Migration";  break;
                case GameState.GameOver:       _phaseText.text = "Game Over";         break;
            }
        }

        if (_clan0PrestigeText != null)
            _clan0PrestigeText.text = $"Argyn Prestige: {GameManager.Instance.PlayerPrestige[0]}";

        if (_clan1PrestigeText != null)
            _clan1PrestigeText.text = $"Naiman Prestige: {GameManager.Instance.PlayerPrestige[1]}";

        if (_endTurnButton != null)
            _endTurnButton.interactable = GameManager.Instance.CurrentState != GameState.Reinforce;
    }

    // Updates the instruction text that guides the current player.
    public void ShowMessage(string message)
    {
        if (_messageText != null)
            _messageText.text = message;
    }

    // Plain-text fallback — keeps old call sites compiling.
    public void ShowCombatResult(string message)
    {
        if (_combatResultPanel == null) return;

        _combatResultText.text = message;
        _combatResultPanel.SetActive(true);

        if (_combatHideCoroutine != null)
            StopCoroutine(_combatHideCoroutine);

        _combatHideCoroutine = StartCoroutine(HideCombatPanelAfterDelay(2f));
    }

    // Visual dice overload — primary path when CombatResult is available.
    public void ShowCombatResult(CombatResult result, string attackerClanName, string defenderClanName)
    {
        if (_diceCombatPanel == null || result == null) return;

        // Stop any running hide coroutine and clear previous dynamic children
        if (_diceHideCoroutine != null)
            StopCoroutine(_diceHideCoroutine);

        foreach (Transform child in _diceCombatPanel.transform)
            Destroy(child.gameObject);

        PopulateDicePanel(result, attackerClanName, defenderClanName);

        _diceCombatPanel.SetActive(true);
        _diceHideCoroutine = StartCoroutine(HideDicePanelAfterDelay(3f));
    }

    // -------------------------------------------------------------------------
    // Dice panel population helpers
    // -------------------------------------------------------------------------

    private void PopulateDicePanel(CombatResult result, string attackerName, string defenderName)
    {
        // Panel is 560×220.  All anchoredPositions are relative to the panel centre.
        //   Header label  y = +85
        //   Column labels y = +58
        //   Dice row      y = +15
        //   Summary line  y = -50
        //   Conquered     y = -83  (conditional)

        Color attackerColor = new Color(0.20f, 0.40f, 1.00f);
        Color defenderColor = new Color(1.00f, 0.30f, 0.20f);

        // Header
        CreateTMPLabel("Header", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 85f), new Vector2(540f, 30f),
            $"{attackerName} attacks {defenderName}!", 17, FontStyles.Bold);

        // Column labels
        TextMeshProUGUI attackLabel = CreateTMPLabel("AttackerLabel", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, 58f), new Vector2(200f, 24f),
            attackerName, 13, FontStyles.Bold);
        attackLabel.color = attackerColor;

        TextMeshProUGUI defendLabel = CreateTMPLabel("DefenderLabel", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, 58f), new Vector2(200f, 24f),
            defenderName, 13, FontStyles.Bold);
        defendLabel.color = defenderColor;

        // Dice geometry constants
        const float dieSize  = 52f;
        const float dieGap   = 8f;
        const float dieStep  = dieSize + dieGap;
        const float diceRowY = 15f;

        // Attacker dice — stack rightward toward centre (rightmost die is closest to centre gap)
        int attackCount = result.AttackerRolls.Length;
        for (int i = 0; i < attackCount; i++)
        {
            float x = -(dieStep * 0.5f) - (dieStep * (attackCount - 1 - i));
            SpawnDie($"AtkDie{i}", _diceCombatPanel, result.AttackerRolls[i],
                new Vector2(x, diceRowY), attackerColor);
        }

        // Defender dice — stack leftward from centre gap
        int defendCount = result.DefenderRolls.Length;
        for (int i = 0; i < defendCount; i++)
        {
            float x = (dieStep * 0.5f) + (dieStep * i);
            SpawnDie($"DefDie{i}", _diceCombatPanel, result.DefenderRolls[i],
                new Vector2(x, diceRowY), defenderColor);
        }

        // Comparison indicators between matched pairs
        int pairs = Mathf.Min(attackCount, defendCount);
        for (int i = 0; i < pairs; i++)
        {
            float attackerX   = -(dieStep * 0.5f) - (dieStep * (attackCount - 1 - i));
            float defenderX   = (dieStep * 0.5f) + (dieStep * i);
            float indicatorX  = (attackerX + defenderX) * 0.5f;

            bool attackerWins = result.AttackerRolls[i] > result.DefenderRolls[i];
            bool tied         = result.AttackerRolls[i] == result.DefenderRolls[i];

            string symbol = tied ? "=" : attackerWins ? ">" : "<";
            Color  symCol = attackerWins
                ? new Color(0.20f, 0.85f, 0.20f)   // green — attacker wins pair
                : new Color(0.90f, 0.20f, 0.20f);   // red   — attacker loses pair

            TextMeshProUGUI ind = CreateTMPLabel($"Ind{i}", _diceCombatPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(indicatorX, diceRowY), new Vector2(20f, dieSize),
                symbol, 16, FontStyles.Bold);
            ind.color = symCol;
        }

        // Summary line
        TextMeshProUGUI summary = CreateTMPLabel("Summary", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -50f), new Vector2(520f, 28f),
            $"Attacker lost {result.AttackerLosses}  |  Defender lost {result.DefenderLosses}",
            15, FontStyles.Normal);
        summary.color = new Color(0.90f, 0.90f, 0.90f);

        // Conquered banner
        if (result.Conquered)
        {
            TextMeshProUGUI conquered = CreateTMPLabel("Conquered", _diceCombatPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -83f), new Vector2(520f, 28f),
                "CONQUERED!", 18, FontStyles.Bold);
            conquered.color = _goldColor;
        }
    }

    // Spawns a single die: a coloured background Image with a bold number label on top.
    private void SpawnDie(string name, GameObject parent, int value, Vector2 position, Color clanColor)
    {
        const float dieSize = 52f;

        GameObject dieGo = new GameObject(name);
        dieGo.transform.SetParent(parent.transform, false);

        RectTransform rt = dieGo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(dieSize, dieSize);

        // Slightly lighter clan colour at 0.9 alpha
        Color bgColor = Color.Lerp(clanColor, Color.white, 0.35f);
        bgColor.a = 0.9f;
        Image img = dieGo.AddComponent<Image>();
        img.color = bgColor;

        // Number label
        GameObject labelGo = new GameObject("Value");
        labelGo.transform.SetParent(dieGo.transform, false);

        RectTransform labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = value.ToString();
        tmp.fontSize  = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = _white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private IEnumerator HideCombatPanelAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_combatResultPanel != null)
            _combatResultPanel.SetActive(false);
    }

    private IEnumerator HideDicePanelAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_diceCombatPanel != null)
            _diceCombatPanel.SetActive(false);
    }

    // Shows the game-over panel with a clan-specific victory message.
    public void ShowGameOver(int winningPlayerId)
    {
        if (_gameOverPanel == null) return;

        string clanName = winningPlayerId == 0 ? "Argyn" : "Naiman";
        _gameOverText.text  = $"{clanName} is victorious!\nThe steppes bow to your Batyr.";
        _gameOverText.color = winningPlayerId == 0 ? _clan0Color : _clan1Color;
        _gameOverPanel.SetActive(true);
    }

    // Tooltip panel — bottom-left corner, hidden until a tile is hovered
    private void BuildTooltipPanel()
    {
        // Anchor to bottom-left corner.
        _tooltipPanel = CreatePanel("TooltipPanel", gameObject,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(130f, 70f), new Vector2(240f, 120f));

        // Terrain name — bold, white, top of panel.
        _tooltipTerrainText = CreateTMPLabel("TooltipTerrain", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(220f, 26f),
            "", 15, FontStyles.Bold);
        _tooltipTerrainText.alignment = TextAlignmentOptions.Left;

        // Owner line — coloured by clan.
        _tooltipOwnerText = CreateTMPLabel("TooltipOwner", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -36f), new Vector2(220f, 22f),
            "", 13, FontStyles.Normal);
        _tooltipOwnerText.alignment = TextAlignmentOptions.Left;

        // Warriors count.
        _tooltipWarriorsText = CreateTMPLabel("TooltipWarriors", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -58f), new Vector2(220f, 22f),
            "", 13, FontStyles.Normal);
        _tooltipWarriorsText.alignment = TextAlignmentOptions.Left;

        // Yurt bonus — only shown when tile.HasYurt is true.
        _tooltipYurtText = CreateTMPLabel("TooltipYurt", _tooltipPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -80f), new Vector2(220f, 22f),
            "+2 Prestige Bonus", 13, FontStyles.Italic);
        _tooltipYurtText.alignment = TextAlignmentOptions.Left;
        _tooltipYurtText.color = new Color(1.00f, 0.85f, 0.20f);

        _tooltipPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public tooltip API called by GameManager
    // -------------------------------------------------------------------------

    public void ShowTooltip(Tile tile)
    {
        if (_tooltipPanel == null || tile == null) return;

        // Terrain name with forest/movement cost annotation.
        string terrainLabel = tile.Terrain switch
        {
            TerrainType.Plain    => "Plain",
            TerrainType.Mountain => "Mountain",
            TerrainType.Water    => "Water",
            TerrainType.Pasture  => "Pasture",
            _                    => tile.Terrain.ToString()
        };
        if (tile.HasForest) terrainLabel += " (Forest · 2MP)";
        _tooltipTerrainText.text  = terrainLabel;
        _tooltipTerrainText.color = _white;

        // Owner line — show contested status if applicable.
        if (tile.OwnerPlayerId == 0)
        {
            _tooltipOwnerText.text  = tile.IsContested ? "Owner: Argyn (Contested)" : "Owner: Argyn Clan";
            _tooltipOwnerText.color = _clan0Color;
        }
        else if (tile.OwnerPlayerId == 1)
        {
            _tooltipOwnerText.text  = tile.IsContested ? "Owner: Naiman (Contested)" : "Owner: Naiman Clan";
            _tooltipOwnerText.color = _clan1Color;
        }
        else
        {
            _tooltipOwnerText.text  = "Owner: Neutral";
            _tooltipOwnerText.color = new Color(0.65f, 0.65f, 0.65f);
        }

        // Warriors count — dash for water or unoccupied neutral.
        bool showWarriors = tile.Terrain != TerrainType.Water &&
                            !(tile.OwnerPlayerId == -1 && tile.CubeCount == 0);
        _tooltipWarriorsText.text  = showWarriors ? $"Warriors: {tile.CubeCount}" : "Warriors: —";
        _tooltipWarriorsText.color = _white;

        // Yurt bonus.
        _tooltipYurtText.gameObject.SetActive(tile.HasYurt);

        _tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (_tooltipPanel != null)
            _tooltipPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Private factory helpers (avoid repeating RectTransform setup boilerplate)
    // -------------------------------------------------------------------------

    private GameObject CreatePanel(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;

        Image img = go.AddComponent<Image>();
        img.color = _panelColor;

        return go;
    }

    private TextMeshProUGUI CreateTMPLabel(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        string text, float fontSize, FontStyles style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.color     = _white;
        tmp.alignment = TextAlignmentOptions.Center;

        return tmp;
    }
}
