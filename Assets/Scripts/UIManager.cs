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

    // Combat setup panel (per-attack dice selection)
    private GameObject          _combatSetupPanel;
    private Button[]            _atkDiceButtons;
    private Button[]            _defDiceButtons;
    private TextMeshProUGUI     _atkDiceLabel;
    private TextMeshProUGUI     _defDiceLabel;
    private int                 _selectedAtkDice;
    private int                 _selectedDefDice;
    private int                 _atkDiceMax;
    private int                 _defDiceMax;

    // Migration count panel
    private GameObject          _migrationCountPanel;
    private TextMeshProUGUI     _migrationCountTitle;
    private TextMeshProUGUI     _migrationCountText;
    private int                 _migrationCount;
    private int                 _migrationCountMax;

    // Migration choice panel (Soldier vs Yurta)
    private GameObject          _migrationChoicePanel;

    // Combat setup header refs (re-coloured per attack based on initiator)
    private TextMeshProUGUI     _atkHeader;
    private TextMeshProUGUI     _defHeader;

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
        BuildCombatSetupPanel();
        BuildMigrationCountPanel();
        BuildMigrationChoicePanel();
    }

    // Public helper so other systems (and the dice panel) can colour UI by initiator.
    public static Color GetClanColor(int playerId)
    {
        return playerId == 0 ? _clan0Color : _clan1Color;
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
                case GameState.Reinforce:       _phaseText.text = "Phase: Reinforce"; break;
                case GameState.Action:          _phaseText.text = "Phase: Action";    break;
                case GameState.CombatSetup:     _phaseText.text = "Phase: Combat";    break;
                case GameState.PostConquest:    _phaseText.text = "Phase: Advance";   break;
                case GameState.MigrationCheck:  _phaseText.text = "Phase: Migration"; break;
                case GameState.MigrationChoice: _phaseText.text = "Phase: Migration"; break;
                case GameState.MigrationCount:  _phaseText.text = "Phase: Migration"; break;
                case GameState.GameOver:        _phaseText.text = "Game Over";        break;
            }
        }

        if (_clan0PrestigeText != null)
            _clan0PrestigeText.text = $"Argyn Prestige: {GameManager.Instance.PlayerPrestige[0]}";

        if (_clan1PrestigeText != null)
            _clan1PrestigeText.text = $"Naiman Prestige: {GameManager.Instance.PlayerPrestige[1]}";

        if (_endTurnButton != null)
        {
            // End Turn is only valid during Action or post-migration; modals own their own flow.
            GameState s = GameManager.Instance.CurrentState;
            _endTurnButton.interactable = s == GameState.Action || s == GameState.MigrationCheck;
        }
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
    public void ShowCombatResult(CombatResult result,
                                 int attackerPlayerId, int defenderPlayerId,
                                 string attackerClanName, string defenderClanName)
    {
        if (_diceCombatPanel == null || result == null) return;

        // Stop any running hide coroutine and clear previous dynamic children
        if (_diceHideCoroutine != null)
            StopCoroutine(_diceHideCoroutine);

        foreach (Transform child in _diceCombatPanel.transform)
            Destroy(child.gameObject);

        PopulateDicePanel(result, attackerPlayerId, defenderPlayerId, attackerClanName, defenderClanName);

        _diceCombatPanel.SetActive(true);
        _diceHideCoroutine = StartCoroutine(HideDicePanelAfterDelay(3f));
    }

    // -------------------------------------------------------------------------
    // Dice panel population helpers
    // -------------------------------------------------------------------------

    private void PopulateDicePanel(CombatResult result,
                                   int attackerPlayerId, int defenderPlayerId,
                                   string attackerName, string defenderName)
    {
        // Panel is 560×220.  All anchoredPositions are relative to the panel centre.
        //   Header label  y = +85
        //   Column labels y = +58
        //   Dice row      y = +15
        //   Summary line  y = -50
        //   Conquered     y = -83  (conditional)

        // Initiator-aware colours: whoever is actually attacking gets their clan colour,
        // not the static "Blue=Attacker / Red=Defender" baseline.
        Color attackerColor = GetClanColor(attackerPlayerId);
        Color defenderColor = GetClanColor(defenderPlayerId);

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
        const float diceRowY = 5f;

        // Attacker dice — highest die closest to the centre gap (so pair-0 sits next to its
        // partner across the gap). Lower-ranked attacker dice extend leftward.
        int attackCount = result.AttackerRolls.Length;
        for (int i = 0; i < attackCount; i++)
        {
            float x = -(dieStep * 0.5f) - (dieStep * i);
            SpawnDie($"AtkDie{i}", _diceCombatPanel, result.AttackerRolls[i],
                new Vector2(x, diceRowY), attackerColor);
        }

        // Defender dice — LOWEST die closest to the centre gap. Reverse pairing: attacker's
        // highest meets defender's lowest, so they sit visually adjacent across the gap.
        int defendCount = result.DefenderRolls.Length;
        for (int i = 0; i < defendCount; i++)
        {
            float x = (dieStep * 0.5f) + (dieStep * (defendCount - 1 - i));
            SpawnDie($"DefDie{i}", _diceCombatPanel, result.DefenderRolls[i],
                new Vector2(x, diceRowY), defenderColor);
        }

        // Outcome badge above each attacker die that actually participated.
        // attackingDiceUsed excludes the discarded forest-bonus die.
        int atkUsed = result.AttackingDiceUsed > 0 ? result.AttackingDiceUsed : attackCount;
        int pairs = Mathf.Min(atkUsed, defendCount);
        for (int i = 0; i < pairs; i++)
        {
            int defIdx = pairs - 1 - i;
            int atkVal = result.AttackerRolls[i];
            int defVal = result.DefenderRolls[defIdx];

            bool attackerWins = atkVal > defVal;
            bool tied         = atkVal == defVal;

            // Defender wins ties, so a tie shows as an attacker loss visually.
            string symbol = tied ? "=" : attackerWins ? ">" : "<";
            Color  symCol = attackerWins
                ? new Color(0.20f, 0.85f, 0.20f)
                : new Color(0.90f, 0.20f, 0.20f);

            float atkX  = -(dieStep * 0.5f) - (dieStep * i);
            float badgeY = diceRowY + dieSize * 0.5f + 14f;

            TextMeshProUGUI ind = CreateTMPLabel($"Ind{i}", _diceCombatPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(atkX, badgeY), new Vector2(46f, 22f),
                $"{symbol} {defVal}", 14, FontStyles.Bold);
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

    // -------------------------------------------------------------------------
    // Combat setup panel — modal dice-count picker for both attacker and defender
    // -------------------------------------------------------------------------

    private void BuildCombatSetupPanel()
    {
        _combatSetupPanel = CreatePanel("CombatSetupPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 280f));
        _combatSetupPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.10f, 0.92f);

        CreateTMPLabel("Title", _combatSetupPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(540f, 32f),
            "Combat Setup", 22, FontStyles.Bold);

        _atkHeader = CreateTMPLabel("AtkHeader", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, 75f), new Vector2(220f, 24f),
            "Attacker dice", 14, FontStyles.Bold);

        _atkDiceButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            int n = i + 1;
            _atkDiceButtons[i] = CreateButton($"AtkDice{n}", _combatSetupPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-180f + i * 50f, 30f), new Vector2(46f, 46f),
                n.ToString(), _buttonColor, () => SetAtkDice(n));
        }

        _atkDiceLabel = CreateTMPLabel("AtkDiceLabel", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, -10f), new Vector2(220f, 22f),
            "Selected: 0", 13, FontStyles.Normal);

        _defHeader = CreateTMPLabel("DefHeader", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, 75f), new Vector2(220f, 24f),
            "Defender dice", 14, FontStyles.Bold);

        _defDiceButtons = new Button[2];
        for (int i = 0; i < 2; i++)
        {
            int n = i + 1;
            _defDiceButtons[i] = CreateButton($"DefDice{n}", _combatSetupPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(80f + i * 50f, 30f), new Vector2(46f, 46f),
                n.ToString(), _buttonColor, () => SetDefDice(n));
        }

        _defDiceLabel = CreateTMPLabel("DefDiceLabel", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, -10f), new Vector2(220f, 22f),
            "Selected: 0", 13, FontStyles.Normal);

        CreateButton("ResolveBtn", _combatSetupPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-80f, 30f), new Vector2(140f, 44f),
            "Resolve", new Color(0.55f, 0.10f, 0.10f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnCombatSetupConfirm(_selectedAtkDice, _selectedDefDice);
            });

        CreateButton("CancelBtn", _combatSetupPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(80f, 30f), new Vector2(140f, 44f),
            "Cancel", new Color(0.30f, 0.30f, 0.30f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnCombatSetupCancel();
            });

        _combatSetupPanel.SetActive(false);
    }

    public void ShowCombatSetup(int attackerPlayerId, int defenderPlayerId,
                                int maxAtk, int maxDef,
                                string atkClan, string defClan)
    {
        if (_combatSetupPanel == null) return;
        _atkDiceMax = maxAtk;
        _defDiceMax = maxDef;
        _selectedAtkDice = maxAtk;
        _selectedDefDice = maxDef;

        for (int i = 0; i < _atkDiceButtons.Length; i++)
            _atkDiceButtons[i].interactable = (i + 1) <= maxAtk;
        for (int i = 0; i < _defDiceButtons.Length; i++)
            _defDiceButtons[i].interactable = (i + 1) <= maxDef;

        // Headers re-coloured each time so the initiator (whoever is attacking) appears
        // in their own clan colour, not always blue/red.
        if (_atkHeader != null) _atkHeader.color = GetClanColor(attackerPlayerId);
        if (_defHeader != null) _defHeader.color = GetClanColor(defenderPlayerId);

        UpdateAtkDiceLabel(atkClan);
        UpdateDefDiceLabel(defClan);

        _combatSetupPanel.SetActive(true);
    }

    public void HideCombatSetup()
    {
        if (_combatSetupPanel != null) _combatSetupPanel.SetActive(false);
    }

    private void SetAtkDice(int n) { _selectedAtkDice = n; UpdateAtkDiceLabel(null); }
    private void SetDefDice(int n) { _selectedDefDice = n; UpdateDefDiceLabel(null); }

    private string _cachedAtkClan = "Attacker";
    private string _cachedDefClan = "Defender";

    private void UpdateAtkDiceLabel(string atkClan)
    {
        if (atkClan != null) _cachedAtkClan = atkClan;
        if (_atkDiceLabel != null)
            _atkDiceLabel.text = $"{_cachedAtkClan}: {_selectedAtkDice} / {_atkDiceMax}";
    }

    private void UpdateDefDiceLabel(string defClan)
    {
        if (defClan != null) _cachedDefClan = defClan;
        if (_defDiceLabel != null)
            _defDiceLabel.text = $"{_cachedDefClan}: {_selectedDefDice} / {_defDiceMax}";
    }

    // -------------------------------------------------------------------------
    // Migration count panel — pick how many warriors move
    // -------------------------------------------------------------------------

    private void BuildMigrationCountPanel()
    {
        _migrationCountPanel = CreatePanel("MigrationCountPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(420f, 220f));
        _migrationCountPanel.GetComponent<Image>().color = new Color(0.05f, 0.10f, 0.05f, 0.92f);

        _migrationCountTitle = CreateTMPLabel("Title", _migrationCountPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(400f, 28f),
            "Migrate warriors", 18, FontStyles.Bold);

        CreateButton("Minus", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-110f, 15f), new Vector2(54f, 54f),
            "-", _buttonColor, () => AdjustMigrationCount(-1));

        _migrationCountText = CreateTMPLabel("Count", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 15f), new Vector2(140f, 54f),
            "1", 26, FontStyles.Bold);

        CreateButton("Plus", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(110f, 15f), new Vector2(54f, 54f),
            "+", _buttonColor, () => AdjustMigrationCount(1));

        CreateButton("ConfirmMig", _migrationCountPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-70f, 30f), new Vector2(120f, 40f),
            "Confirm", new Color(0.15f, 0.55f, 0.15f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnMigrationCountConfirm(_migrationCount);
            });

        CreateButton("CancelMig", _migrationCountPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(70f, 30f), new Vector2(120f, 40f),
            "Cancel", new Color(0.30f, 0.30f, 0.30f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnMigrationCountCancel();
            });

        _migrationCountPanel.SetActive(false);
    }

    public void ShowMigrationCount(int max, int defaultCount, string title = "Migrate warriors")
    {
        if (_migrationCountPanel == null) return;
        _migrationCountMax = Mathf.Max(1, max);
        _migrationCount    = Mathf.Clamp(defaultCount, 1, _migrationCountMax);
        if (_migrationCountTitle != null) _migrationCountTitle.text = title;
        UpdateMigrationCountText();
        _migrationCountPanel.SetActive(true);
    }

    public void HideMigrationCount()
    {
        if (_migrationCountPanel != null) _migrationCountPanel.SetActive(false);
    }

    private void AdjustMigrationCount(int delta)
    {
        _migrationCount = Mathf.Clamp(_migrationCount + delta, 1, _migrationCountMax);
        UpdateMigrationCountText();
    }

    private void UpdateMigrationCountText()
    {
        if (_migrationCountText != null)
            _migrationCountText.text = $"{_migrationCount} / {_migrationCountMax}";
    }

    // -------------------------------------------------------------------------
    // Migration choice panel — pick whether to move soldiers or relocate the yurt
    // -------------------------------------------------------------------------

    private void BuildMigrationChoicePanel()
    {
        _migrationChoicePanel = CreatePanel("MigrationChoicePanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(440f, 200f));
        _migrationChoicePanel.GetComponent<Image>().color = new Color(0.05f, 0.10f, 0.05f, 0.92f);

        CreateTMPLabel("Title", _migrationChoicePanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(420f, 28f),
            "Move soldiers or relocate yurt?", 17, FontStyles.Bold);

        CreateButton("MoveSoldiers", _migrationChoicePanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-100f, 0f), new Vector2(170f, 60f),
            "Move Soldiers", new Color(0.15f, 0.55f, 0.15f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnMigrationChooseSoldiers();
            });

        CreateButton("MoveYurta", _migrationChoicePanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(100f, 0f), new Vector2(170f, 60f),
            "Move Yurta", new Color(0.85f, 0.65f, 0.10f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnMigrationChooseYurta();
            });

        CreateButton("CancelChoice", _migrationChoicePanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 25f), new Vector2(140f, 38f),
            "Cancel", new Color(0.30f, 0.30f, 0.30f), () =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnMigrationChoiceCancel();
            });

        _migrationChoicePanel.SetActive(false);
    }

    public void ShowMigrationChoice()
    {
        if (_migrationChoicePanel != null) _migrationChoicePanel.SetActive(true);
    }

    public void HideMigrationChoice()
    {
        if (_migrationChoicePanel != null) _migrationChoicePanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Button factory — used by the dice/migration panels
    // -------------------------------------------------------------------------

    private Button CreateButton(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        string label, Color bgColor, System.Action onClick)
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
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());
        ColorBlock cb = btn.colors;
        cb.disabledColor = new Color(0.30f, 0.30f, 0.30f, 0.6f);
        btn.colors = cb;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        RectTransform labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = _white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }
}
