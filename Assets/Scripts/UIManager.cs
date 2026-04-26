using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Design system — matches hi-fi prototype (hifi/styles.css)
    // Player 0 (Argyn)  → Kazakh green   #3a8a3e
    // Player 1 (Naiman) → Timurid red    #c8402e
    // Player 2 (Kerey)  → Slavic blue    #2a6aaa
    // Player 3 (Kongrat)→ Ming gold      #d4b044
    // -------------------------------------------------------------------------
    private static readonly Color _clan0Color   = new Color(0.227f, 0.541f, 0.243f); // kazakh green
    private static readonly Color _clan1Color   = new Color(0.784f, 0.251f, 0.180f); // timurid red
    private static readonly Color _clan2Color   = new Color(0.165f, 0.416f, 0.667f); // slavic blue
    private static readonly Color _clan3Color   = new Color(0.831f, 0.690f, 0.267f); // ming gold
    private static readonly Color _panelColor   = new Color(0.047f, 0.035f, 0.031f, 0.92f); // #0c0908 dark wood
    private static readonly Color _panelLeather = new Color(0.180f, 0.133f, 0.094f, 0.94f); // #2e2218 leather
    private static readonly Color _buttonColor  = new Color(0.115f, 0.090f, 0.063f, 0.90f); // dark felt
    private static readonly Color _white        = Color.white;
    private static readonly Color _goldColor    = new Color(0.784f, 0.659f, 0.306f); // #c8a84e
    private static readonly Color _goldBright   = new Color(0.910f, 0.800f, 0.369f); // #e8cc5e
    private static readonly Color _goldDim      = new Color(0.541f, 0.455f, 0.204f); // #8a7434
    private static readonly Color _brass        = new Color(0.690f, 0.533f, 0.188f); // #b08830
    private static readonly Color _brassAlpha   = new Color(0.690f, 0.533f, 0.188f, 0.45f);
    private static readonly Color _cream        = new Color(0.925f, 0.878f, 0.769f); // #ece0c4
    private static readonly Color _creamDim     = new Color(0.722f, 0.659f, 0.510f); // #b8a882
    private static readonly Color _parchment    = new Color(0.784f, 0.722f, 0.533f); // #c8b888
    private static readonly Color _parchmentBg  = new Color(0.690f, 0.627f, 0.459f); // parchment panel bg

    // -------------------------------------------------------------------------
    // UI element references (built in Awake)
    // -------------------------------------------------------------------------

    // Screen-level panels (for show/hide)
    private GameObject           _topBarGo;
    private GameObject           _rightPanelGo;
    private GameObject           _hintPanelGo;
    private GameObject           _mainMenuPanel;
    private GameObject           _leaderSelectionPanel;
    private TextMeshProUGUI[]    _leaderMissionTexts;

    // Sprite overrides — assign via the Inspector to plug in Hi-Fi art assets
    [SerializeField] private Sprite   _menuBackground;
    [SerializeField] private Sprite   _buttonSkin;
    [SerializeField] private Sprite   _panelSkin;
    [SerializeField] private Sprite[] _leaderPortraits; // order: Argyn, Naiman, Kerey, Kongrat
    [SerializeField] private Sprite   _victoryBackground;

    // Top bar
    private Image            _leaderPanelBg;
    private Image            _leaderDotImage;
    private TextMeshProUGUI  _leaderNameText;
    private TextMeshProUGUI  _leaderEmpireText;
    private TextMeshProUGUI  _phaseHintText;
    private Image[]          _phasePillBg    = new Image[4];
    private TextMeshProUGUI[] _phasePillLabel = new TextMeshProUGUI[4];
    private Button           _endTurnButton;
    private TextMeshProUGUI  _endTurnLabel;
    private Image            _endTurnBg;

    // Legacy text refs (kept for ShowMessage)
    private TextMeshProUGUI _playerTurnText;
    private TextMeshProUGUI _phaseText;
    private TextMeshProUGUI _messageText;

    // Idle hint badge
    private GameObject      _idleHintPanel;
    private TextMeshProUGUI _idleHintText;
    private Coroutine       _idleHintCoroutine;

    // Right panel (full-height sidebar)
    private Image[]           _rosterSlotBg  = new Image[4];
    private TextMeshProUGUI[] _rosterNames   = new TextMeshProUGUI[4];
    private TextMeshProUGUI[] _rosterTerr    = new TextMeshProUGUI[4];
    private TextMeshProUGUI[] _rosterWarr    = new TextMeshProUGUI[4];
    private TextMeshProUGUI   _turnNumText;
    private TextMeshProUGUI   _totalTerrText;

    // Mission (inside right panel)
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

    // Game-over overlay
    private GameObject          _gameOverPanel;
    private TextMeshProUGUI     _gameOverText;

    // Tooltip
    private GameObject          _tooltipPanel;
    private Image               _tooltipPanelBg;
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
    private Image               _atkZoneImage;
    private Image               _defZoneImage;
    private string              _cachedAtkClan = "Attacker";
    private string              _cachedDefClan = "Defender";

    // Troop count picker
    private GameObject          _migrationCountPanel;
    private TextMeshProUGUI     _migrationCountTitle;
    private TextMeshProUGUI     _migrationCountText;
    private int                 _migrationCount;
    private int                 _migrationCountMax;

    // Phase label data
    private static readonly string[] _phaseIds    = { "SETUP", "REINFORCE", "ATTACK", "FORTIFY" };
    private static readonly string[] _phaseIcons  = { "*", "+", "x", ">>" };

    // Cinzel fonts — loaded in Awake via AssetDatabase
    private TMP_FontAsset _fontRegular;
    private TMP_FontAsset _fontBold;

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
        LoadFonts();
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
        canvas.sortingOrder = 10;
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

    private static void EnsureEventSystem()
    {
        var existing = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        GameObject esGo;
        if (existing != null)
        {
            esGo = existing.gameObject;
        }
        else
        {
            esGo = new GameObject("EventSystem");
            DontDestroyOnLoad(esGo);
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        var standalone = esGo.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (standalone != null) DestroyImmediate(standalone);

        var isModule = esGo.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (isModule != null)
        {
            // If the module exists but has a broken asset reference, it won't work.
            // Recreating it forces it to generate default implicit actions.
            DestroyImmediate(isModule);
        }
        esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        if (esGo.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
    }

    private void LoadFonts()
    {
        // Fonts live in Assets/Resources/fonts/ so they ship with built players.
        // AssetDatabase is editor-only and would leave the build font-less.
        _fontRegular = Resources.Load<TMP_FontAsset>("fonts/Cinzel-Regular SDF");
        _fontBold    = Resources.Load<TMP_FontAsset>("fonts/Cinzel-Bold SDF");
        if (_fontRegular == null || _fontBold == null)
            Debug.LogWarning("[UIManager] Cinzel SDF fonts not found in Resources — falling back to TMP default.");
    }

    // -------------------------------------------------------------------------
    // HUD construction
    // -------------------------------------------------------------------------

    private void BuildHUD()
    {
        BuildTopBar();
        BuildRightPanel();
        BuildCombatResultPanel();
        BuildDiceCombatPanel();
        BuildGameOverPanel();
        BuildTooltipPanel();
        BuildCombatSetupPanel();
        BuildMigrationCountPanel();
        BuildPhaseBanner();
        BuildIdleHintPanel();
        BuildMainMenu();
        BuildLeaderSelection();

        // Game starts at MainMenu — hide gameplay HUD until the player starts
        if (_topBarGo != null)    _topBarGo.SetActive(false);
        if (_rightPanelGo != null) _rightPanelGo.SetActive(false);
        if (_hintPanelGo != null)  _hintPanelGo.SetActive(false);
    }

    public static Color GetClanColor(int playerId) => playerId switch
    {
        0 => _clan0Color,
        1 => _clan1Color,
        2 => _clan2Color,
        3 => _clan3Color,
        _ => new Color(0.55f, 0.55f, 0.55f),
    };

    // Top bar: leader panel | dividers | phase pills | hint text | END PHASE button
    private void BuildTopBar()
    {
        const float barH = 56f;
        const float rightPanelW = 220f;

        // Bar background
        GameObject bar = new GameObject("TopBar");
        bar.transform.SetParent(gameObject.transform, false);
        _topBarGo = bar;
        RectTransform brt = bar.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f); brt.anchorMax = new Vector2(1f, 1f);
        brt.pivot = new Vector2(0f, 1f);
        brt.offsetMin = new Vector2(0f, -barH);
        brt.offsetMax = new Vector2(-rightPanelW, 0f);
        Image barImg = bar.AddComponent<Image>();
        barImg.color = new Color(0.082f, 0.059f, 0.047f, 0.96f); // dark wood
        barImg.raycastTarget = false;
        // Bottom gold border
        AddGoldBorder(bar, bottom: true);

        // ── Leader panel (left side) ─────────────────────────────────────
        const float leaderW = 200f;
        GameObject leaderPanel = new GameObject("LeaderPanel");
        leaderPanel.transform.SetParent(bar.transform, false);
        RectTransform lp = leaderPanel.AddComponent<RectTransform>();
        lp.anchorMin = new Vector2(0f, 0f); lp.anchorMax = new Vector2(0f, 1f);
        lp.pivot = new Vector2(0f, 0.5f);
        lp.anchoredPosition = new Vector2(4f, 0f);
        lp.sizeDelta = new Vector2(leaderW, -8f);
        _leaderPanelBg = leaderPanel.AddComponent<Image>();
        _leaderPanelBg.color = new Color(0f, 0f, 0f, 0f);
        _leaderPanelBg.raycastTarget = false;
        AddGoldBorder(leaderPanel);

        // Colour swatch dot
        GameObject dot = new GameObject("Dot");
        dot.transform.SetParent(leaderPanel.transform, false);
        RectTransform drt = dot.AddComponent<RectTransform>();
        drt.anchorMin = drt.anchorMax = drt.pivot = new Vector2(0f, 0.5f);
        drt.anchoredPosition = new Vector2(10f, 0f);
        drt.sizeDelta = new Vector2(10f, 10f);
        Image dotImg = dot.AddComponent<Image>();
        dotImg.color = _clan0Color;
        dotImg.raycastTarget = false;
        _leaderDotImage = dotImg;

        _leaderNameText = CreateTMPLabel("LeaderName", leaderPanel,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(26f, 7f), new Vector2(-30f, 20f),
            "Kazakh's Turn", 13, FontStyles.Bold);
        _leaderNameText.color = _cream;
        _leaderNameText.alignment = TextAlignmentOptions.Left;

        _leaderEmpireText = CreateTMPLabel("LeaderEmpire", leaderPanel,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(26f, -8f), new Vector2(-30f, 16f),
            "REINFORCE", 9, FontStyles.Normal);
        _leaderEmpireText.color = _goldDim;
        _leaderEmpireText.alignment = TextAlignmentOptions.Left;

        // Vertical divider
        AddVerticalDivider(bar, leaderW + 10f);

        // ── Phase pills (center) ─────────────────────────────────────────
        const float pillW = 82f, pillH = 32f, pillGap = 6f;
        float pillsStart = leaderW + 20f;
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            GameObject pill = new GameObject($"PhasePill_{_phaseIds[i]}");
            pill.transform.SetParent(bar.transform, false);
            RectTransform prt = pill.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 0.5f); prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.anchoredPosition = new Vector2(pillsStart + i * (pillW + pillGap), 0f);
            prt.sizeDelta = new Vector2(pillW, pillH);
            Image pillImg = pill.AddComponent<Image>();
            pillImg.color = new Color(_brassAlpha.r, _brassAlpha.g, _brassAlpha.b, 0.08f);
            pillImg.raycastTarget = false;
            _phasePillBg[i] = pillImg;
            AddGoldBorder(pill);

            // Icon + label side by side
            TextMeshProUGUI pillTmp = CreateTMPLabel($"PillLabel_{i}", pill,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                $"{_phaseIcons[i]} {_phaseIds[i]}", 9, FontStyles.Bold);
            pillTmp.color = _goldDim;
            _phasePillLabel[i] = pillTmp;
        }

        AddVerticalDivider(bar, pillsStart + 4 * (pillW + pillGap) + 4f);

        // ── Hint panel — floating below the top bar, anchored upper-left under leader panel ──
        GameObject hintPanel = new GameObject("HintPanel");
        _hintPanelGo = hintPanel;
        hintPanel.transform.SetParent(gameObject.transform, false);
        RectTransform hpr = hintPanel.AddComponent<RectTransform>();
        hpr.anchorMin = new Vector2(0f, 1f); hpr.anchorMax = new Vector2(0f, 1f);
        hpr.pivot = new Vector2(0f, 1f);
        hpr.anchoredPosition = new Vector2(8f, -(barH + 6f));
        hpr.sizeDelta = new Vector2(260f, 36f);
        Image hintBg = hintPanel.AddComponent<Image>();
        hintBg.color = new Color(0.082f, 0.059f, 0.047f, 0.88f);
        hintBg.raycastTarget = false;
        AddGoldBorder(hintPanel);

        _phaseHintText = CreateTMPLabel("PhaseHint", hintPanel,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(-12f, -6f),
            "Place your initial warriors", 9, FontStyles.Italic);
        _phaseHintText.color = _cream;
        _phaseHintText.alignment = TextAlignmentOptions.Center;
        _phaseHintText.textWrappingMode = TextWrappingModes.Normal;

        // Keep a hidden legacy text for any ShowMessage calls
        _messageText = CreateTMPLabel("MessageText_Legacy", bar,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(0f, 0f), "", 10, FontStyles.Normal);
        _messageText.color = new Color(0,0,0,0); // invisible
        _playerTurnText = _leaderNameText;
        _phaseText      = _leaderEmpireText;

        // ── END PHASE circular button (right of bar) ─────────────────────
        GameObject endBtn = new GameObject("EndPhaseBtn");
        endBtn.transform.SetParent(bar.transform, false);
        RectTransform ert = endBtn.AddComponent<RectTransform>();
        ert.anchorMin = ert.anchorMax = new Vector2(1f, 0.5f);
        ert.pivot = new Vector2(1f, 0.5f);
        ert.anchoredPosition = new Vector2(-16f, 0f);
        ert.sizeDelta = new Vector2(82f, 32f);
        _endTurnBg = endBtn.AddComponent<Image>();
        _endTurnBg.color = new Color(0.082f, 0.059f, 0.047f, 0.96f);

        // Gold ring
        AddGoldBorder(endBtn);

        Button btn = endBtn.AddComponent<Button>();
        _endTurnButton = btn;
        btn.targetGraphic = _endTurnBg;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.25f, 0.20f, 0.10f, 1f);
        cb.pressedColor     = new Color(0.10f, 0.08f, 0.04f, 1f);
        cb.disabledColor    = new Color(0.15f, 0.12f, 0.09f, 0.6f);
        btn.colors = cb;
        btn.onClick.AddListener(() => GameManager.Instance?.OnEndTurnButtonClicked());

        _endTurnLabel = CreateTMPLabel("EndLabel", endBtn,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            "END\nPHASE", 9, FontStyles.Bold);
        _endTurnLabel.color = _goldColor;
    }

    // Full-height right sidebar: mission parchment + ornament + empire roster + turn counter
    private void BuildRightPanel()
    {
        const float panelW = 220f;
        const float topBarH = 56f;

        // Sidebar background
        GameObject panel = new GameObject("RightPanel");
        panel.transform.SetParent(gameObject.transform, false);
        _rightPanelGo = panel;
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(panelW, 0f);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = _panelLeather;
        panelImg.raycastTarget = false;
        AddGoldBorder(panel, left: true);

        float y = -topBarH; // start below top bar

        // ── Secret mission parchment ──────────────────────────────────────
        const float missionH = 90f;
        _missionPanel = new GameObject("MissionPanel");
        _missionPanel.transform.SetParent(panel.transform, false);
        RectTransform mrt = _missionPanel.AddComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0f, 1f); mrt.anchorMax = new Vector2(1f, 1f);
        mrt.pivot = new Vector2(0.5f, 1f);
        mrt.anchoredPosition = new Vector2(0f, y);
        mrt.sizeDelta = new Vector2(-16f, missionH);
        Image mImg = _missionPanel.AddComponent<Image>();
        mImg.color = _parchmentBg;
        mImg.raycastTarget = false;
        AddGoldBorder(_missionPanel, dashed: true);

        TextMeshProUGUI mTitle = CreateTMPLabel("MissionTitle", _missionPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -6f), new Vector2(0f, 16f),
            "SECRET MISSION", 9, FontStyles.Bold);
        mTitle.color = new Color(0.25f, 0.16f, 0.04f); // dark brown — high contrast on parchment
        mTitle.alignment = TextAlignmentOptions.Center;

        _missionLabel = CreateTMPLabel("MissionLabel", _missionPanel,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 4f), new Vector2(-10f, 52f), "", 11, FontStyles.Bold);
        _missionLabel.color = new Color(0.08f, 0.05f, 0.02f); // near-black, maximum contrast
        _missionLabel.alignment = TextAlignmentOptions.Center;

        _missionProgress = CreateTMPLabel("MissionProgress", _missionPanel,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 6f), new Vector2(-12f, 16f), "", 8, FontStyles.Normal);
        _missionProgress.color = new Color(0.2f, 0.4f, 0.2f);
        _missionProgress.alignment = TextAlignmentOptions.Center;

        y -= missionH + 6f;

        // ── Gold ornament divider ─────────────────────────────────────────
        AddOrnament(panel, y, panelW);
        y -= 10f;

        // ── "Empire Roster" label ─────────────────────────────────────────
        TextMeshProUGUI rosterTitle = CreateTMPLabel("RosterTitle", panel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, y), new Vector2(0f, 18f),
            "EMPIRE ROSTER", 8, FontStyles.Bold);
        rosterTitle.color = _goldDim;
        rosterTitle.alignment = TextAlignmentOptions.Center;
        y -= 20f;

        // ── 4 empire slots ────────────────────────────────────────────────
        const float slotH = 46f, slotGap = 6f;
        string[] defaultNames = { "Kazakh", "Timurid", "Rusya", "Ming" };
        for (int i = 0; i < 4; i++)
        {
            Color fc = GetClanColor(i);

            GameObject slot = new GameObject($"EmpireSlot_{i}");
            slot.transform.SetParent(panel.transform, false);
            RectTransform srt = slot.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 1f); srt.anchorMax = new Vector2(1f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0f, y);
            srt.sizeDelta = new Vector2(-12f, slotH);
            Image slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(fc.r, fc.g, fc.b, 0.12f);
            slotImg.raycastTarget = false;
            _rosterSlotBg[i] = slotImg;
            AddGoldBorder(slot);

            // Faction colour swatch
            GameObject swatch = new GameObject("Swatch");
            swatch.transform.SetParent(slot.transform, false);
            RectTransform swrt = swatch.AddComponent<RectTransform>();
            swrt.anchorMin = swrt.anchorMax = new Vector2(0f, 0.5f);
            swrt.pivot = new Vector2(0f, 0.5f);
            swrt.anchoredPosition = new Vector2(6f, 0f);
            swrt.sizeDelta = new Vector2(6f, 22f);
            Image swatchImg = swatch.AddComponent<Image>();
            swatchImg.color = fc;
            swatchImg.raycastTarget = false;

            // Clan name
            _rosterNames[i] = CreateTMPLabel($"RosterName_{i}", slot,
                new Vector2(0f, 0.5f), new Vector2(0.6f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(18f, 6f), new Vector2(-18f, 18f),
                defaultNames[i], 11, FontStyles.Bold);
            _rosterNames[i].color = _cream;
            _rosterNames[i].alignment = TextAlignmentOptions.Left;

            // Territory count
            _rosterTerr[i] = CreateTMPLabel($"RosterTerr_{i}", slot,
                new Vector2(0f, 0.5f), new Vector2(0.6f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(18f, -8f), new Vector2(-18f, 14f),
                "T: —", 9, FontStyles.Normal);
            _rosterTerr[i].color = _goldDim;
            _rosterTerr[i].alignment = TextAlignmentOptions.Left;

            // Warrior count (right side)
            _rosterWarr[i] = CreateTMPLabel($"RosterWarr_{i}", slot,
                new Vector2(0.6f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                new Vector2(-6f, 0f), new Vector2(0f, 0f),
                "W: —", 10, FontStyles.Bold);
            _rosterWarr[i].color = _cream;
            _rosterWarr[i].alignment = TextAlignmentOptions.Right;

            y -= slotH + slotGap;
        }

        y -= 4f;
        AddOrnament(panel, y, panelW);
        y -= 12f;

        // ── Turn counter ──────────────────────────────────────────────────
        _turnNumText = CreateTMPLabel("TurnNum", panel,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, y), new Vector2(-10f, 36f),
            "TURN\n1", 10, FontStyles.Bold);
        _turnNumText.color = _cream;
        _turnNumText.alignment = TextAlignmentOptions.Left;

        _totalTerrText = CreateTMPLabel("TotalTerr", panel,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-10f, y), new Vector2(-10f, 36f),
            "TILES\n—", 10, FontStyles.Bold);
        _totalTerrText.color = _cream;
        _totalTerrText.alignment = TextAlignmentOptions.Right;
        y -= 40f;
    }

    private void BuildCombatResultPanel()
    {
        _combatResultPanel = CreatePanel("CombatResultPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500f, 160f));
        _combatResultPanel.GetComponent<Image>().color = _panelColor;
        AddGoldBorder(_combatResultPanel);
        _combatResultText = CreateTMPLabel("CombatText", _combatResultPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(460f, 140f), "", 18, FontStyles.Bold);
        _combatResultText.alignment = TextAlignmentOptions.Center;
        _combatResultText.color = _cream;
        _combatResultPanel.SetActive(false);
    }

    private void BuildDiceCombatPanel()
    {
        _diceCombatPanel = CreatePanel("DiceCombatPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(600f, 260f));
        _diceCombatPanel.GetComponent<Image>().color = new Color(0.055f, 0.039f, 0.027f, 0.97f);
        AddGoldBorder(_diceCombatPanel);
        _diceCombatPanel.SetActive(false);
    }

    // ── Game Over panel fields (portrait swapped at runtime)
    private Image _gameOverPortrait;

    private void BuildGameOverPanel()
    {
        // Wider panel to accommodate portrait + text side by side
        _gameOverPanel = CreatePanel("GameOverPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(680f, 360f));
        _gameOverPanel.GetComponent<Image>().color = new Color(0.025f, 0.016f, 0.010f, 0.96f);
        AddGoldBorder(_gameOverPanel);

        // ── Portrait (left half, 200×200 square) ─────────────────────────
        GameObject portraitGo = new GameObject("WinnerPortrait");
        portraitGo.transform.SetParent(_gameOverPanel.transform, false);
        RectTransform prt = portraitGo.AddComponent<RectTransform>();
        prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
        prt.anchoredPosition = new Vector2(-200f, 20f);
        prt.sizeDelta = new Vector2(200f, 200f);
        _gameOverPortrait = portraitGo.AddComponent<Image>();
        _gameOverPortrait.color = new Color(0.18f, 0.14f, 0.09f, 1f);
        _gameOverPortrait.preserveAspect = true;
        _gameOverPortrait.raycastTarget = false;
        AddGoldBorder(portraitGo);

        // ── Right: title + result text ────────────────────────────────────
        var victoryTitle = CreateTMPLabel("VictoryTitle", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(100f, 100f), new Vector2(320f, 40f), "VICTORY!", 28, FontStyles.Bold);
        victoryTitle.color = _goldColor;
        victoryTitle.alignment = TextAlignmentOptions.Center;

        _gameOverText = CreateTMPLabel("GameOverText", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(100f, 20f), new Vector2(320f, 130f), "", 15, FontStyles.Normal);
        _gameOverText.alignment = TextAlignmentOptions.Center;
        _gameOverText.color = _cream;

        // Gold divider
        GameObject div = new GameObject("Divider");
        div.transform.SetParent(_gameOverPanel.transform, false);
        RectTransform divrt = div.AddComponent<RectTransform>();
        divrt.anchorMin = divrt.anchorMax = divrt.pivot = new Vector2(0.5f, 0.5f);
        divrt.anchoredPosition = new Vector2(100f, -65f);
        divrt.sizeDelta = new Vector2(300f, 1f);
        div.AddComponent<Image>().color = _brassAlpha;

        var replayBtn = CreateButton("ReplayButton", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-20f, -115f), new Vector2(170f, 50f),
            "REPLAY", new Color(0.490f, 0.082f, 0.082f),
            () => GameManager.Instance?.RestartGame());
        AddGoldBorder(replayBtn.gameObject);

        CreateButton("MainMenuButton", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, -115f), new Vector2(170f, 50f),
            "MAIN MENU", new Color(0.165f, 0.122f, 0.063f, 0.96f),
            () => GameManager.Instance?.RestartGame());

        _gameOverPanel.SetActive(false);
    }

    private void BuildTooltipPanel()
    {
        // Bottom-left, faction-colored panel
        _tooltipPanel = CreatePanel("TooltipPanel", gameObject,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(10f, 10f), new Vector2(260f, 148f));
        _tooltipPanelBg = _tooltipPanel.GetComponent<Image>();
        _tooltipPanelBg.color = _panelLeather;
        _tooltipPanelBg.raycastTarget = false;
        AddGoldBorder(_tooltipPanel);

        // Territory name — large, gold
        _tooltipNameText = CreateTMPLabel("TooltipName", _tooltipPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(8f, -10f), new Vector2(-16f, 26f), "", 16, FontStyles.Bold);
        _tooltipNameText.color = _goldColor;
        _tooltipNameText.alignment = TextAlignmentOptions.Left;

        // Region label
        _tooltipRegionText = CreateTMPLabel("TooltipRegion", _tooltipPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(8f, -38f), new Vector2(-16f, 16f), "", 9, FontStyles.Italic);
        _tooltipRegionText.color = _creamDim;
        _tooltipRegionText.alignment = TextAlignmentOptions.Left;

        // Gold ornament separator
        AddOrnament(_tooltipPanel, -56f, 260f);

        // Terrain type
        _tooltipTerrainText = CreateTMPLabel("TooltipTerrain", _tooltipPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(8f, -70f), new Vector2(-16f, 18f), "", 11, FontStyles.Normal);
        _tooltipTerrainText.color = _creamDim;
        _tooltipTerrainText.alignment = TextAlignmentOptions.Left;

        // Owner
        _tooltipOwnerText = CreateTMPLabel("TooltipOwner", _tooltipPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(8f, -90f), new Vector2(-16f, 18f), "", 11, FontStyles.Normal);
        _tooltipOwnerText.alignment = TextAlignmentOptions.Left;

        // Warrior count — right-aligned with sword icon
        _tooltipWarriorsText = CreateTMPLabel("TooltipWarriors", _tooltipPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-8f, -90f), new Vector2(-16f, 18f), "", 13, FontStyles.Bold);
        _tooltipWarriorsText.color = _goldBright;
        _tooltipWarriorsText.alignment = TextAlignmentOptions.Right;

        _tooltipPanel.SetActive(false);
    }

    private void BuildPhaseBanner()
    {
        // Anchor top-center, sit just under the top bar — small, unobtrusive
        _phaseBannerPanel = CreatePanel("PhaseBanner", gameObject,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -86f), new Vector2(220f, 42f));
        // Dark semi-transparent background — slightly larger than the text
        Image bannerBg = _phaseBannerPanel.GetComponent<Image>();
        bannerBg.color = new Color(0.04f, 0.03f, 0.02f, 0.82f);
        bannerBg.raycastTarget = false;
        AddGoldBorder(_phaseBannerPanel);

        _phaseBannerText = CreateTMPLabel("BannerText", _phaseBannerPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(200f, 32f), "", 18, FontStyles.Bold);
        _phaseBannerText.alignment  = TextAlignmentOptions.Center;
        _phaseBannerText.color      = Color.white;
        _phaseBannerPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        int   pid   = GameManager.Instance.CurrentPlayerId;
        Color fc    = GetClanColor(pid);

        // ── Leader panel ──────────────────────────────────────────────────
        // Background color is transparent so no need to update it here.

        if (_leaderDotImage != null) _leaderDotImage.color = fc;

        if (_leaderNameText != null)
        {
            _leaderNameText.text  = $"{GameManager.Instance.GetClanName(pid)}'s Turn";
            _leaderNameText.color = _cream;
        }

        if (_leaderEmpireText != null)
        {
            _leaderEmpireText.text = GameManager.Instance.CurrentState switch
            {
                GameState.MainMenu         => "MENU",
                GameState.InitialPlacement => "SETUP",
                GameState.Reinforce        => "REINFORCE",
                GameState.Attack           => "ATTACK",
                GameState.CombatSetup      => "COMBAT",
                GameState.PostConquest     => "ADVANCE",
                GameState.Fortify          => "FORTIFY",
                GameState.GameOver         => "GAME OVER",
                _                          => "—",
            };
            _leaderEmpireText.color = fc;
        }

        // ── Phase pills ───────────────────────────────────────────────────
        RefreshPhasePills();

        // ── Hint text ─────────────────────────────────────────────────────
        if (_phaseHintText != null)
        {
            _phaseHintText.text = GameManager.Instance.CurrentState switch
            {
                GameState.MainMenu         => "",
                GameState.InitialPlacement => $"Place {GameManager.Instance.GetExtraTroopsLeft(pid)} warriors on your territories",
                GameState.Reinforce        => $"+{Mathf.Max(3, (CountTiles(pid) / 3))} warriors to place",
                GameState.Attack           => "Click an enemy territory to attack",
                GameState.CombatSetup      => "Choose dice — attacker rolls up to 3",
                GameState.PostConquest     => "Choose how many warriors to advance",
                GameState.Fortify          => "Move warriors between connected territories",
                GameState.GameOver         => "The steppes have found their ruler",
                _                          => "",
            };
        }

        // ── End-turn button ───────────────────────────────────────────────
        if (_endTurnButton != null)
        {
            GameState s = GameManager.Instance.CurrentState;
            bool active = s == GameState.InitialPlacement || s == GameState.Attack || s == GameState.Fortify;
            _endTurnButton.interactable = active;
            if (_endTurnBg != null)
                _endTurnBg.color = active
                    ? new Color(0.165f, 0.122f, 0.063f, 0.96f)
                    : new Color(0.082f, 0.059f, 0.047f, 0.96f);
            if (_endTurnLabel != null)
                _endTurnLabel.text = s switch
                {
                    GameState.InitialPlacement => "SKIP\nTURN",
                    GameState.Attack           => "END\nATTACK",
                    GameState.Fortify          => "END\nTURN",
                    _                          => "END\nPHASE",
                };
        }

        // ── Mission panel ─────────────────────────────────────────────────
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

        // ── Empire roster ─────────────────────────────────────────────────
        RefreshEmpireRoster(pid);
    }

    private void RefreshPhasePills()
    {
        if (_phasePillBg == null) return;

        GameState state = GameManager.Instance.CurrentState;
        if (state == GameState.MainMenu) return;
        bool[] active = {
            state == GameState.InitialPlacement,
            state == GameState.Reinforce,
            state == GameState.Attack || state == GameState.CombatSetup || state == GameState.PostConquest,
            state == GameState.Fortify,
        };

        for (int i = 0; i < 4; i++)
        {
            if (_phasePillBg[i] == null) continue;
            _phasePillBg[i].color = active[i]
                ? new Color(_goldColor.r, _goldColor.g, _goldColor.b, 0.20f)
                : new Color(_brassAlpha.r, _brassAlpha.g, _brassAlpha.b, 0.06f);
            if (_phasePillLabel[i] != null)
                _phasePillLabel[i].color = active[i] ? _goldBright : _goldDim;
        }
    }

    private void RefreshEmpireRoster(int activePid)
    {
        if (HexGridManager.Instance == null) return;

        int totalTiles = 0;
        int[] terrCounts = new int[4];
        int[] warrCounts = new int[4];

        foreach (Tile t in HexGridManager.Instance.Grid.Values)
        {
            if (t.Terrain == TerrainType.Water || t.Terrain == TerrainType.Mountain) continue;
            int pid = t.OwnerPlayerId;
            if (pid >= 0 && pid < 4)
            {
                terrCounts[pid]++;
                warrCounts[pid] += t.CubeCount;
                totalTiles++;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            Color fc = GetClanColor(i);
            bool isActive = i == activePid;

            if (_rosterSlotBg[i] != null)
                _rosterSlotBg[i].color = new Color(fc.r, fc.g, fc.b, isActive ? 0.28f : 0.10f);
            if (_rosterNames[i] != null)
            {
                _rosterNames[i].text  = GameManager.Instance?.GetClanName(i) ?? $"Player {i}";
                _rosterNames[i].color = isActive ? _cream : _creamDim;
            }
            if (_rosterTerr[i] != null)
                _rosterTerr[i].text = $"T: {terrCounts[i]}";
            if (_rosterWarr[i] != null)
                _rosterWarr[i].text = $"W: {warrCounts[i]}";
        }

        if (_totalTerrText != null)
            _totalTerrText.text = $"TILES\n{totalTiles}";
    }

    private int CountTiles(int pid)
    {
        if (HexGridManager.Instance == null) return 0;
        int n = 0;
        foreach (Tile t in HexGridManager.Instance.Grid.Values)
            if (t.OwnerPlayerId == pid && t.Terrain != TerrainType.Water && t.Terrain != TerrainType.Mountain)
                n++;
        return n;
    }

    private string GetMissionProgress(int pid, Mission m)
    {
        if (HexGridManager.Instance == null) return "";
        switch (m.Type)
        {
            case MissionType.ControlTerritories:
                return $"Progress: {CountTiles(pid)} / {m.TargetCount} territories";
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
        // Route to phase hint in hi-fi layout
        if (_phaseHintText != null) _phaseHintText.text = message;
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
        _phaseBannerText.color = Color.white;  // always white — dark bg provides contrast
        Image bannerBg = _phaseBannerPanel.GetComponent<Image>();
        _phaseBannerPanel.SetActive(true);

        // Fade in (0.2 s)
        float t = 0f;
        while (t < 0.20f)
        {
            t += Time.deltaTime;
            float a = t / 0.20f;
            _phaseBannerText.alpha = a;
            if (bannerBg) bannerBg.color = new Color(0.04f, 0.03f, 0.02f, 0.82f * a);
            yield return null;
        }
        _phaseBannerText.alpha = 1f;
        if (bannerBg) bannerBg.color = new Color(0.04f, 0.03f, 0.02f, 0.82f);

        // Hold (2.5 s)
        yield return new WaitForSeconds(2.5f);

        // Fade out (0.35 s)
        t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            float a = 1f - t / 0.35f;
            _phaseBannerText.alpha = a;
            if (bannerBg) bannerBg.color = new Color(0.04f, 0.03f, 0.02f, 0.82f * a);
            yield return null;
        }
        _phaseBannerPanel.SetActive(false);
    }

    // Tooltip
    public void ShowTooltip(Tile tile)
    {
        if (_tooltipPanel == null || tile == null) return;

        // Tint tooltip panel by owner
        int ownerId = tile.OwnerPlayerId;
        if (_tooltipPanelBg != null)
        {
            if (ownerId >= 0 && ownerId < 4)
            {
                Color fc = GetClanColor(ownerId);
                _tooltipPanelBg.color = new Color(
                    Mathf.Lerp(_panelLeather.r, fc.r, 0.18f),
                    Mathf.Lerp(_panelLeather.g, fc.g, 0.18f),
                    Mathf.Lerp(_panelLeather.b, fc.b, 0.18f),
                    0.94f);
            }
            else
                _tooltipPanelBg.color = _panelLeather;
        }

        bool hasName = !string.IsNullOrEmpty(tile.TerritoryName);
        _tooltipNameText.text  = hasName ? tile.TerritoryName : tile.Terrain.ToString();
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
        if (tile.HasForest) terrain += "  (Forest +1 die)";
        _tooltipTerrainText.text  = terrain;
        _tooltipTerrainText.color = _creamDim;

        if (ownerId >= 0 && ownerId < GameManager.PlayerCount)
        {
            string clanName = GameManager.Instance?.GetClanName(ownerId) ?? $"Player {ownerId}";
            _tooltipOwnerText.text  = clanName;
            _tooltipOwnerText.color = GetClanColor(ownerId);
        }
        else
        {
            _tooltipOwnerText.text  = "Neutral";
            _tooltipOwnerText.color = _creamDim;
        }

        bool showTroops = tile.Terrain != TerrainType.Water && tile.CubeCount > 0;
        _tooltipWarriorsText.text  = showTroops ? $"{tile.CubeCount} troops" : "";
        _tooltipWarriorsText.color = _goldBright;

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

        // ── Coloured left/right zone strips ──────────────────────────────
        {
            GameObject az = new GameObject("AtkZone");
            az.transform.SetParent(_diceCombatPanel.transform, false);
            RectTransform azrt = az.AddComponent<RectTransform>();
            azrt.anchorMin = Vector2.zero; azrt.anchorMax = new Vector2(0.5f, 1f);
            azrt.offsetMin = azrt.offsetMax = Vector2.zero;
            az.AddComponent<Image>().color = new Color(atkColor.r * 0.12f, atkColor.g * 0.12f, atkColor.b * 0.12f, 0.6f);

            GameObject dz = new GameObject("DefZone");
            dz.transform.SetParent(_diceCombatPanel.transform, false);
            RectTransform dzrt = dz.AddComponent<RectTransform>();
            dzrt.anchorMin = new Vector2(0.5f, 0f); dzrt.anchorMax = Vector2.one;
            dzrt.offsetMin = dzrt.offsetMax = Vector2.zero;
            dz.AddComponent<Image>().color = new Color(defColor.r * 0.12f, defColor.g * 0.12f, defColor.b * 0.12f, 0.6f);
        }

        // ── Header ─────────────────────────────────────────────────────
        var header = CreateTMPLabel("Header", _diceCombatPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -14f), new Vector2(560f, 30f),
            $"{atkName}  ⚔  {defName}", 17, FontStyles.Bold);
        header.color = _goldColor;

        TextMeshProUGUI aLabel = CreateTMPLabel("AtkLabel", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-145f, 72f), new Vector2(200f, 24f), atkName.ToUpper(), 12, FontStyles.Bold);
        aLabel.color = atkColor;
        aLabel.alignment = TextAlignmentOptions.Center;

        // VS label
        var vsLbl = CreateTMPLabel("VS", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 72f), new Vector2(40f, 24f), "VS", 13, FontStyles.Bold);
        vsLbl.color = _goldDim;

        TextMeshProUGUI dLabel = CreateTMPLabel("DefLabel", _diceCombatPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(145f, 72f), new Vector2(200f, 24f), defName.ToUpper(), 12, FontStyles.Bold);
        dLabel.color = defColor;
        dLabel.alignment = TextAlignmentOptions.Center;

        const float dieSize = 58f, dieStep = 68f, diceRowY = 10f;
        int atkCount = result.AttackerRolls.Length, defCount = result.DefenderRolls.Length;

        TextMeshProUGUI[] atkLabels = new TextMeshProUGUI[atkCount];
        TextMeshProUGUI[] defLabels = new TextMeshProUGUI[defCount];

        // Keep each side strictly in its half of the 600px panel (±300).
        // Attacker group centred at x=-148, defender at x=+148.
        const float atkCX = -148f, defCX = 148f;
        for (int i = 0; i < atkCount; i++)
        {
            float x = atkCX + (i - (atkCount - 1) * 0.5f) * dieStep;
            atkLabels[i] = SpawnDie($"AtkDie{i}", _diceCombatPanel, -1, new Vector2(x, diceRowY), atkColor);
        }
        for (int i = 0; i < defCount; i++)
        {
            float x = defCX + (i - (defCount - 1) * 0.5f) * dieStep;
            defLabels[i] = SpawnDie($"DefDie{i}", _diceCombatPanel, -1, new Vector2(x, diceRowY), defColor);
        }

        AudioManager.Instance?.PlayDiceSound();

        RectTransform[] allRects = new RectTransform[atkCount + defCount];
        int rIdx = 0;
        foreach (var l in atkLabels) allRects[rIdx++] = l != null ? l.transform.parent as RectTransform : null;
        foreach (var l in defLabels) allRects[rIdx++] = l != null ? l.transform.parent as RectTransform : null;

        // Phase 1: Shake — fast at first, progressively slower
        float[] shakeIntervals = { 0.035f, 0.035f, 0.04f, 0.04f, 0.05f, 0.06f, 0.075f, 0.095f, 0.12f, 0.16f };
        foreach (float interval in shakeIntervals)
        {
            foreach (var l in atkLabels) if (l) l.text = Random.Range(1, 7).ToString();
            foreach (var l in defLabels) if (l) l.text = Random.Range(1, 7).ToString();
            foreach (var rt in allRects)
            {
                if (rt == null) continue;
                rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-28f, 28f));
                rt.localScale    = Vector3.one * Random.Range(0.88f, 1.12f);
            }
            yield return new WaitForSeconds(interval);
        }

        // Snap to final values
        for (int i = 0; i < atkCount; i++) if (atkLabels[i]) atkLabels[i].text = result.AttackerRolls[i].ToString();
        for (int i = 0; i < defCount; i++) if (defLabels[i]) defLabels[i].text = result.DefenderRolls[i].ToString();

        // Phase 2: Pop reveal
        const float popDuration = 0.32f;
        float popT = 0f;
        while (popT < popDuration)
        {
            popT += Time.deltaTime;
            float u = Mathf.Clamp01(popT / popDuration);
            float scale = u < 0.5f
                ? Mathf.Lerp(1.05f, 1.55f, u * 2f)
                : Mathf.Lerp(1.55f, 1f,    (u - 0.5f) * 2f);
            foreach (var rt in allRects)
            {
                if (rt == null) continue;
                rt.localRotation = Quaternion.Lerp(rt.localRotation, Quaternion.identity, 0.35f);
                rt.localScale    = Vector3.one * scale;
            }
            yield return null;
        }
        foreach (var rt in allRects) { if (rt != null) { rt.localRotation = Quaternion.identity; rt.localScale = Vector3.one; } }

        yield return new WaitForSeconds(0.1f);

        int atkUsed = result.AttackingDiceUsed > 0 ? result.AttackingDiceUsed : atkCount;
        int pairs   = Mathf.Min(atkUsed, defCount);

        // Win/loss indicators shown above each attacker die (sorted high→low, pairs matched)
        for (int i = 0; i < pairs; i++)
        {
            int di  = pairs - 1 - i;
            int av  = result.AttackerRolls[i], dv = result.DefenderRolls[di];
            bool win = av > dv;
            string sym = win ? "▲" : "▼";
            Color  sc  = win ? new Color(0.2f, 0.85f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
            float atkDieX = atkCX + (i - (atkCount - 1) * 0.5f) * dieStep;
            var ind = CreateTMPLabel($"Ind{i}", _diceCombatPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(atkDieX, diceRowY + dieSize * 0.5f + 14f), new Vector2(46f, 22f),
                sym, 16, FontStyles.Bold);
            ind.color = sc;
        }

        TextMeshProUGUI summary = CreateTMPLabel("Summary", _diceCombatPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 42f), new Vector2(560f, 28f),
            $"Attacker lost {result.AttackerLosses}  •  Defender lost {result.DefenderLosses}", 14, FontStyles.Normal);
        summary.color = _cream;
        summary.alignment = TextAlignmentOptions.Center;

        if (result.Conquered)
        {
            TextMeshProUGUI conq = CreateTMPLabel("Conquered", _diceCombatPanel,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 15f), new Vector2(560f, 28f), "TERRITORY CONQUERED!", 18, FontStyles.Bold);
            conq.color = _goldColor;
            conq.alignment = TextAlignmentOptions.Center;
        }

        yield return new WaitForSeconds(3f);
        if (_diceCombatPanel != null) _diceCombatPanel.SetActive(false);
    }

    private TextMeshProUGUI SpawnDie(string name, GameObject parent, int value, Vector2 position, Color clanColor)
    {
        const float dieSize = 58f;
        GameObject dieGo = new GameObject(name);
        dieGo.transform.SetParent(parent.transform, false);
        RectTransform rt = dieGo.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position; rt.sizeDelta = new Vector2(dieSize, dieSize);
        Color bg = new Color(
            Mathf.Lerp(_panelColor.r, clanColor.r, 0.25f),
            Mathf.Lerp(_panelColor.g, clanColor.g, 0.25f),
            Mathf.Lerp(_panelColor.b, clanColor.b, 0.25f), 0.94f);
        Image dieImg = dieGo.AddComponent<Image>();
        dieImg.color = bg;
        dieImg.raycastTarget = false;
        AddGoldBorder(dieGo);

        GameObject lGo = new GameObject("Value"); lGo.transform.SetParent(dieGo.transform, false);
        RectTransform lrt = lGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text = value < 1 ? "?" : value.ToString(); tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold; tmp.color = _cream; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private IEnumerator HidePanelAfter(GameObject panel, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (panel != null) panel.SetActive(false);
    }

    // Leader names mirroring the portrait asset order (must match _leaderPortraits index)
    private static readonly string[] LeaderTitles =
        { "Zhanibek Khan", "Tamerlane", "Slavic Knight", "Yongle" };

    public void ShowGameOver(int winningPlayerId, Mission mission)
    {
        if (_gameOverPanel == null) return;
        Color fc   = GetClanColor(winningPlayerId);
        string clan = GameManager.Instance?.GetClanName(winningPlayerId) ?? $"Player {winningPlayerId}";
        string leader = winningPlayerId >= 0 && winningPlayerId < LeaderTitles.Length
            ? LeaderTitles[winningPlayerId] : clan;
        string missionLine = mission != null ? $"Mission complete:\n{mission.Description}" : "";

        if (_gameOverText != null)
        {
            _gameOverText.text  = $"{clan}\n{leader}\n\n{missionLine}";
            _gameOverText.color = fc;
        }

        // Swap portrait to the winner's image
        if (_gameOverPortrait != null)
        {
            bool hasPortrait = _leaderPortraits != null
                && winningPlayerId < _leaderPortraits.Length
                && _leaderPortraits[winningPlayerId] != null;
            if (hasPortrait)
            {
                _gameOverPortrait.sprite = _leaderPortraits[winningPlayerId];
                _gameOverPortrait.color  = Color.white;
            }
            else
            {
                _gameOverPortrait.sprite = null;
                _gameOverPortrait.color  = new Color(fc.r * 0.35f, fc.g * 0.35f, fc.b * 0.35f, 1f);
            }
        }

        _gameOverPanel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Combat setup panel
    // -------------------------------------------------------------------------

    private void BuildCombatSetupPanel()
    {
        // Panel: 600 × 300, dark wood background
        _combatSetupPanel = CreatePanel("CombatSetupPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(600f, 300f));
        _combatSetupPanel.GetComponent<Image>().color = new Color(0.055f, 0.039f, 0.027f, 0.97f);
        AddGoldBorder(_combatSetupPanel);

        // ── Title bar ────────────────────────────────────────────────────
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(_combatSetupPanel.transform, false);
        RectTransform trt = titleBar.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = Vector2.zero; trt.sizeDelta = new Vector2(0f, 46f);
        titleBar.AddComponent<Image>().color = new Color(0.082f, 0.059f, 0.039f, 1f);
        AddGoldBorder(titleBar, bottom: true);

        var title = CreateTMPLabel("Title", titleBar,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(560f, 38f), "COMBAT SETUP", 22, FontStyles.Bold);
        title.color = _goldColor;

        // ── Left (attacker) zone ─────────────────────────────────────────
        GameObject atkZone = new GameObject("AtkZone");
        atkZone.transform.SetParent(_combatSetupPanel.transform, false);
        RectTransform artz = atkZone.AddComponent<RectTransform>();
        artz.anchorMin = new Vector2(0f, 0.18f); artz.anchorMax = new Vector2(0.5f, 1f);
        artz.offsetMin = new Vector2(0f, 0f); artz.offsetMax = new Vector2(-1f, -46f);
        Image atkZoneImg = atkZone.AddComponent<Image>();
        atkZoneImg.color = new Color(0.18f, 0.05f, 0.04f, 0.35f);
        atkZoneImg.raycastTarget = false;
        _atkZoneImage = atkZoneImg;

        _atkHeader = CreateTMPLabel("AtkHeader", atkZone,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -14f), new Vector2(240f, 24f), "ATTACKER DICE", 13, FontStyles.Bold);
        _atkHeader.color = _clan1Color;
        _atkHeader.alignment = TextAlignmentOptions.Center;

        _atkDiceButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            int n = i + 1;
            var btn = CreateButton($"AtkDice{n}", atkZone,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-74f + i * 62f, 10f), new Vector2(54f, 54f),
                n.ToString(), new Color(0.30f, 0.08f, 0.06f), () => SetAtkDice(n));
            AddGoldBorder(btn.gameObject);
            _atkDiceButtons[i] = btn;
        }

        _atkDiceLabel = CreateTMPLabel("AtkLabel", atkZone,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 20f), new Vector2(240f, 20f), "", 11, FontStyles.Normal);
        _atkDiceLabel.color = _creamDim;
        _atkDiceLabel.alignment = TextAlignmentOptions.Center;

        // ── Center VS divider ─────────────────────────────────────────────
        GameObject vsDivider = new GameObject("VsDivider");
        vsDivider.transform.SetParent(_combatSetupPanel.transform, false);
        RectTransform vrt = vsDivider.AddComponent<RectTransform>();
        vrt.anchorMin = new Vector2(0.5f, 0.18f); vrt.anchorMax = new Vector2(0.5f, 1f);
        vrt.pivot = new Vector2(0.5f, 0.5f);
        vrt.anchoredPosition = new Vector2(0f, -23f); vrt.sizeDelta = new Vector2(1.5f, -46f);
        vsDivider.AddComponent<Image>().color = _brassAlpha;

        var vsLabel = CreateTMPLabel("VS", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 8f), new Vector2(42f, 28f), "VS", 15, FontStyles.Bold);
        vsLabel.color = _goldDim;

        // ── Right (defender) zone ─────────────────────────────────────────
        GameObject defZone = new GameObject("DefZone");
        defZone.transform.SetParent(_combatSetupPanel.transform, false);
        RectTransform defzt = defZone.AddComponent<RectTransform>();
        defzt.anchorMin = new Vector2(0.5f, 0.18f); defzt.anchorMax = new Vector2(1f, 1f);
        defzt.offsetMin = new Vector2(1f, 0f); defzt.offsetMax = new Vector2(0f, -46f);
        Image defZoneImg = defZone.AddComponent<Image>();
        defZoneImg.color = new Color(0.04f, 0.08f, 0.18f, 0.35f);
        defZoneImg.raycastTarget = false;
        _defZoneImage = defZoneImg;

        _defHeader = CreateTMPLabel("DefHeader", defZone,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -14f), new Vector2(240f, 24f), "DEFENDER DICE", 13, FontStyles.Bold);
        _defHeader.color = _clan2Color;
        _defHeader.alignment = TextAlignmentOptions.Center;

        _defDiceButtons = new Button[2];
        for (int i = 0; i < 2; i++)
        {
            int n = i + 1;
            var btn = CreateButton($"DefDice{n}", defZone,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-31f + i * 62f, 10f), new Vector2(54f, 54f),
                n.ToString(), new Color(0.06f, 0.12f, 0.28f), () => SetDefDice(n));
            AddGoldBorder(btn.gameObject);
            _defDiceButtons[i] = btn;
        }

        _defDiceLabel = CreateTMPLabel("DefLabel", defZone,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 20f), new Vector2(240f, 20f), "", 11, FontStyles.Normal);
        _defDiceLabel.color = _creamDim;
        _defDiceLabel.alignment = TextAlignmentOptions.Center;

        // ── Bottom action bar ─────────────────────────────────────────────
        GameObject actionBar = new GameObject("ActionBar");
        actionBar.transform.SetParent(_combatSetupPanel.transform, false);
        RectTransform abart = actionBar.AddComponent<RectTransform>();
        abart.anchorMin = new Vector2(0f, 0f); abart.anchorMax = new Vector2(1f, 0.18f);
        abart.offsetMin = abart.offsetMax = Vector2.zero;
        actionBar.AddComponent<Image>().color = new Color(0.06f, 0.044f, 0.030f, 1f);
        AddGoldBorder(actionBar, top: true);

        var resolveBtn = CreateButton("ResolveBtn", actionBar,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-90f, 0f), new Vector2(160f, 38f),
            "RESOLVE", new Color(0.490f, 0.082f, 0.082f),
            () => GameManager.Instance?.OnCombatSetupConfirm(_selectedAtkDice, _selectedDefDice));
        AddGoldBorder(resolveBtn.gameObject);

        CreateButton("CancelBtn", actionBar,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(90f, 0f), new Vector2(160f, 38f),
            "CANCEL", new Color(0.20f, 0.16f, 0.11f),
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
        Color ac = GetClanColor(atkPid);
        Color dc = GetClanColor(defPid);
        if (_atkHeader)    _atkHeader.color = ac;
        if (_defHeader)    _defHeader.color = dc;
        if (_atkZoneImage) _atkZoneImage.color = new Color(ac.r * 0.22f, ac.g * 0.22f, ac.b * 0.22f, 0.45f);
        if (_defZoneImage) _defZoneImage.color = new Color(dc.r * 0.22f, dc.g * 0.22f, dc.b * 0.22f, 0.45f);
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
        _migrationCountPanel.GetComponent<Image>().color = _panelColor;
        AddGoldBorder(_migrationCountPanel);

        _migrationCountTitle = CreateTMPLabel("Title", _migrationCountPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(400f, 28f), "Move troops", 18, FontStyles.Bold);
        _migrationCountTitle.color = _goldColor;

        CreateButton("Minus", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-110f, 15f), new Vector2(54f, 54f),
            "-", new Color(0.35f, 0.10f, 0.10f), () => AdjustCount(-1));

        _migrationCountText = CreateTMPLabel("Count", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 15f), new Vector2(140f, 54f), "1", 26, FontStyles.Bold);
        _migrationCountText.color = _cream;

        CreateButton("Plus", _migrationCountPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(110f, 15f), new Vector2(54f, 54f),
            "+", new Color(0.10f, 0.28f, 0.10f), () => AdjustCount(1));

        CreateButton("Confirm", _migrationCountPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-70f, 30f), new Vector2(120f, 40f),
            "Confirm", new Color(0.118f, 0.431f, 0.118f),
            () => GameManager.Instance?.OnMigrationCountConfirm(_migrationCount));

        CreateButton("Cancel", _migrationCountPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(70f, 30f), new Vector2(120f, 40f),
            "Cancel", new Color(0.20f, 0.16f, 0.11f),
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
    // Idle hint badge
    // -------------------------------------------------------------------------

    private void BuildIdleHintPanel()
    {
        _idleHintPanel = CreatePanel("IdleHintPanel", gameObject,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 90f), new Vector2(520f, 48f));
        Image bg = _idleHintPanel.GetComponent<Image>();
        bg.color = new Color(0.04f, 0.03f, 0.02f, 0.85f);
        bg.raycastTarget = false;
        AddGoldBorder(_idleHintPanel);

        _idleHintText = CreateTMPLabel("IdleHintText", _idleHintPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500f, 40f), "", 13, FontStyles.Normal);
        _idleHintText.color     = _cream;
        _idleHintText.alignment = TextAlignmentOptions.Center;

        _idleHintPanel.SetActive(false);
    }

    public void ShowIdleHint(string message)
    {
        if (_idleHintPanel == null) return;
        if (_idleHintCoroutine != null) StopCoroutine(_idleHintCoroutine);
        _idleHintCoroutine = StartCoroutine(IdleHintCoroutine(message));
    }

    public void HideIdleHint()
    {
        if (_idleHintCoroutine != null) { StopCoroutine(_idleHintCoroutine); _idleHintCoroutine = null; }
        if (_idleHintPanel != null) _idleHintPanel.SetActive(false);
    }

    private IEnumerator IdleHintCoroutine(string message)
    {
        if (_idleHintText != null) _idleHintText.text = message;
        _idleHintPanel.SetActive(true);
        Image bg = _idleHintPanel.GetComponent<Image>();

        float t = 0f;
        while (t < 0.25f) { t += Time.deltaTime; float a = t / 0.25f; _idleHintText.alpha = a; if (bg) bg.color = new Color(0.04f, 0.03f, 0.02f, 0.85f * a); yield return null; }
        _idleHintText.alpha = 1f;

        yield return new WaitForSeconds(4f);

        t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; float a = 1f - t / 0.4f; _idleHintText.alpha = a; if (bg) bg.color = new Color(0.04f, 0.03f, 0.02f, 0.85f * a); yield return null; }
        _idleHintPanel.SetActive(false);
        _idleHintCoroutine = null;
    }

    // -------------------------------------------------------------------------
    // Main Menu screen
    // -------------------------------------------------------------------------

    private void BuildMainMenu()
    {
        _mainMenuPanel = CreatePanel("MainMenuPanel", gameObject,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        Image bg = _mainMenuPanel.GetComponent<Image>();
        bg.color = _menuBackground != null ? Color.white : new Color(0.025f, 0.016f, 0.010f, 0.97f);
        if (_menuBackground != null) { bg.sprite = _menuBackground; bg.type = Image.Type.Sliced; }
        bg.raycastTarget = true;

        // Title
        var title = CreateTMPLabel("Title", _mainMenuPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 110f), new Vector2(720f, 90f),
            "RISE OF NOMADS", 54, FontStyles.Bold);
        title.color = _goldColor;
        title.alignment = TextAlignmentOptions.Center;

        var sub = CreateTMPLabel("Subtitle", _mainMenuPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 56f), new Vector2(520f, 30f),
            "Lords of the Kazakh Steppe", 18, FontStyles.Italic);
        sub.color = _creamDim;
        sub.alignment = TextAlignmentOptions.Center;

        // Decorative gold separator
        GameObject line = new GameObject("GoldLine");
        line.transform.SetParent(_mainMenuPanel.transform, false);
        RectTransform lrt = line.AddComponent<RectTransform>();
        lrt.anchorMin = lrt.anchorMax = lrt.pivot = new Vector2(0.5f, 0.5f);
        lrt.anchoredPosition = new Vector2(0f, 26f);
        lrt.sizeDelta = new Vector2(400f, 1.5f);
        line.AddComponent<Image>().color = _brassAlpha;

        // Clan colour dots
        float[] dotX = { -90f, -30f, 30f, 90f };
        for (int i = 0; i < 4; i++)
        {
            GameObject dot = new GameObject($"ClanDot_{i}");
            dot.transform.SetParent(_mainMenuPanel.transform, false);
            RectTransform drt = dot.AddComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = drt.pivot = new Vector2(0.5f, 0.5f);
            drt.anchoredPosition = new Vector2(dotX[i], 10f);
            drt.sizeDelta = new Vector2(8f, 8f);
            dot.AddComponent<Image>().color = GetClanColor(i);
        }

        var startBtn = CreateButton("StartBtn", _mainMenuPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -30f), new Vector2(250f, 58f),
            "START GAME", new Color(0.165f, 0.122f, 0.063f, 0.96f),
            () => GameManager.Instance?.StartGame());
        AddGoldBorder(startBtn.gameObject);

        var quitBtn = CreateButton("QuitBtn", _mainMenuPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -102f), new Vector2(250f, 50f),
            "QUIT", new Color(0.20f, 0.16f, 0.11f, 0.96f),
            () => Application.Quit());

        var ver = CreateTMPLabel("Version", _mainMenuPanel,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-14f, 12f), new Vector2(220f, 18f),
            "Rise of Nomads  v1.0", 9, FontStyles.Normal);
        ver.color = new Color(_goldDim.r, _goldDim.g, _goldDim.b, 0.45f);
        ver.alignment = TextAlignmentOptions.Right;

        _mainMenuPanel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Leader Selection screen
    // -------------------------------------------------------------------------

    private void BuildLeaderSelection()
    {
        _leaderSelectionPanel = CreatePanel("LeaderSelectionPanel", gameObject,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        _leaderSelectionPanel.GetComponent<Image>().color = new Color(0.025f, 0.016f, 0.010f, 0.97f);
        _leaderSelectionPanel.GetComponent<Image>().raycastTarget = true;

        var title = CreateTMPLabel("Title", _leaderSelectionPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -38f), new Vector2(600f, 52f),
            "CLAN LEADERS", 38, FontStyles.Bold);
        title.color = _goldColor;
        title.alignment = TextAlignmentOptions.Center;

        var sub = CreateTMPLabel("Subtitle", _leaderSelectionPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -76f), new Vector2(520f, 24f),
            "Your secret mission has been assigned — keep it hidden!", 13, FontStyles.Italic);
        sub.color = _creamDim;
        sub.alignment = TextAlignmentOptions.Center;

        // Leader names matching the provided portrait assets (in clan-color order)
        // 0=Argyn(green)→Zhanibek Khan  1=Naiman(red)→Tamerlane
        // 2=Kerey(blue)→Slavic Knight   3=Kongrat(gold)→Yongle
        string[] clanNames   = { "Argyn",         "Naiman",    "Kerey",         "Kongrat" };
        string[] leaderNames = { "Zhanibek Khan", "Tamerlane", "Slavic Knight", "Yongle" };

        // Card centres: 168px wide, 10px gap → total span 702px at 1280 ref
        float[] xPos = { -285f, -95f, 95f, 285f };
        _leaderMissionTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            Color fc = GetClanColor(i);

            // ── Card frame: 168 × 290 ────────────────────────────────────────
            // Layout (y from frame centre, frame half-height = 145):
            //   +145  top edge
            //   +143  clan colour swatch (4px)
            //   +139  portrait top
            //    +65  portrait centre  (158×158 square)
            //     -9  portrait bottom
            //    -20  name strip centre (22px)
            //    -32  name bottom
            //    -88  mission block centre (height 88)
            //   -132  mission bottom
            //   -145  bottom edge  (13px margin)
            GameObject frame = new GameObject($"LeaderFrame_{i}");
            frame.transform.SetParent(_leaderSelectionPanel.transform, false);
            RectTransform frt = frame.AddComponent<RectTransform>();
            frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0.5f, 0.5f);
            frt.anchoredPosition = new Vector2(xPos[i], 15f);
            frt.sizeDelta = new Vector2(168f, 290f);
            Image fImg = frame.AddComponent<Image>();
            fImg.color = new Color(fc.r * 0.10f, fc.g * 0.10f, fc.b * 0.10f, 0.97f);
            if (_panelSkin != null) { fImg.sprite = _panelSkin; fImg.type = Image.Type.Sliced; }
            fImg.raycastTarget = false;
            AddGoldBorder(frame);

            // Top clan colour swatch (full width stripe)
            GameObject swatch = new GameObject("Swatch");
            swatch.transform.SetParent(frame.transform, false);
            RectTransform swrt = swatch.AddComponent<RectTransform>();
            swrt.anchorMin = swrt.anchorMax = swrt.pivot = new Vector2(0.5f, 0.5f);
            swrt.anchoredPosition = new Vector2(0f, 143f);
            swrt.sizeDelta = new Vector2(168f, 4f);
            swatch.AddComponent<Image>().color = fc;

            // ── Portrait (158×158 square — matches 1:1 source images) ───────
            GameObject portrait = new GameObject("Portrait");
            portrait.transform.SetParent(frame.transform, false);
            RectTransform prt = portrait.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = new Vector2(0f, 65f);
            prt.sizeDelta = new Vector2(158f, 158f);
            Image pImg = portrait.AddComponent<Image>();
            pImg.raycastTarget = false;
            pImg.preserveAspect = true;  // never squish the 1:1 portraits

            if (_leaderPortraits != null && i < _leaderPortraits.Length && _leaderPortraits[i] != null)
            {
                pImg.sprite = _leaderPortraits[i];
                pImg.color  = Color.white;
            }
            else
            {
                // Placeholder: clan-tinted box with first letter
                pImg.color = new Color(fc.r * 0.35f, fc.g * 0.35f, fc.b * 0.35f, 1f);
                var initLabel = CreateTMPLabel("Initial", portrait,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(158f, 158f),
                    clanNames[i][0].ToString(), 60, FontStyles.Bold);
                initLabel.color = new Color(fc.r, fc.g, fc.b, 0.30f);
            }

            // Thin clan-colour border around portrait
            AddGoldBorder(portrait);

            // ── Clan + leader name strip ─────────────────────────────────────
            var clanLabel = CreateTMPLabel($"Clan_{i}", frame,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f), new Vector2(156f, 20f),
                clanNames[i].ToUpper(), 13, FontStyles.Bold);
            clanLabel.color = fc;
            clanLabel.alignment = TextAlignmentOptions.Center;

            var leaderLabel = CreateTMPLabel($"Leader_{i}", frame,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -36f), new Vector2(156f, 16f),
                leaderNames[i], 9, FontStyles.Italic);
            leaderLabel.color = _creamDim;
            leaderLabel.alignment = TextAlignmentOptions.Center;

            // Ornament divider
            GameObject div = new GameObject("Div");
            div.transform.SetParent(frame.transform, false);
            RectTransform divrt = div.AddComponent<RectTransform>();
            divrt.anchorMin = divrt.anchorMax = divrt.pivot = new Vector2(0.5f, 0.5f);
            divrt.anchoredPosition = new Vector2(0f, -47f);
            divrt.sizeDelta = new Vector2(140f, 1f);
            div.AddComponent<Image>().color = _brassAlpha;

            // ── Secret mission block ─────────────────────────────────────────
            _leaderMissionTexts[i] = CreateTMPLabel($"Mission_{i}", frame,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -90f), new Vector2(152f, 82f),
                "SECRET MISSION:\n???", 8, FontStyles.Italic);
            _leaderMissionTexts[i].color = _creamDim;
            _leaderMissionTexts[i].alignment = TextAlignmentOptions.Center;
        }

        var beginBtn = CreateButton("BeginSetupBtn", _leaderSelectionPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 36f), new Vector2(260f, 58f),
            "BEGIN SETUP", new Color(0.165f, 0.122f, 0.063f, 0.96f),
            () => GameManager.Instance?.BeginActualGame());
        AddGoldBorder(beginBtn.gameObject);

        _leaderSelectionPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Screen navigation
    // -------------------------------------------------------------------------

    public void ShowMainMenu()
    {
        if (_mainMenuPanel != null)       _mainMenuPanel.SetActive(true);
        if (_leaderSelectionPanel != null) _leaderSelectionPanel.SetActive(false);
        if (_topBarGo != null)            _topBarGo.SetActive(false);
        if (_rightPanelGo != null)        _rightPanelGo.SetActive(false);
        if (_hintPanelGo != null)         _hintPanelGo.SetActive(false);
        if (_gameOverPanel != null)       _gameOverPanel.SetActive(false);
        HideTooltip();
        DismissPhaseBanner();
    }

    public void ShowLeaderSelection()
    {
        if (_mainMenuPanel != null)       _mainMenuPanel.SetActive(false);
        if (_leaderSelectionPanel != null)
        {
            RefreshLeaderSelection();
            _leaderSelectionPanel.SetActive(true);
        }
        if (_topBarGo != null)   _topBarGo.SetActive(false);
        if (_rightPanelGo != null) _rightPanelGo.SetActive(false);
        if (_hintPanelGo != null)  _hintPanelGo.SetActive(false);
        HideTooltip();
    }

    public void HideMenuScreens()
    {
        if (_mainMenuPanel != null)       _mainMenuPanel.SetActive(false);
        if (_leaderSelectionPanel != null) _leaderSelectionPanel.SetActive(false);
        if (_topBarGo != null)            _topBarGo.SetActive(true);
        if (_rightPanelGo != null)        _rightPanelGo.SetActive(true);
        if (_hintPanelGo != null)         _hintPanelGo.SetActive(true);
    }

    private void RefreshLeaderSelection()
    {
        if (_leaderMissionTexts == null || GameManager.Instance == null) return;
        for (int i = 0; i < _leaderMissionTexts.Length; i++)
        {
            if (_leaderMissionTexts[i] == null) continue;
            Mission m = GameManager.Instance.GetPlayerMission(i);
            _leaderMissionTexts[i].text = m != null
                ? $"SECRET MISSION:\n{m.Description}"
                : "SECRET MISSION:\n???";
        }
    }

    // -------------------------------------------------------------------------
    // Factory helpers
    // -------------------------------------------------------------------------

    // Gold border strips on all four sides (or specified sides)
    private void AddGoldBorder(GameObject parent,
        bool top = false, bool bottom = false, bool left = false, bool right = false,
        bool dashed = false)
    {
        // When called with no side flags (all false), draws all four sides.
        bool full = !top && !bottom && !left && !right;
        if (full || top)    AddTrimStrip(parent, "TrimTop",    new Vector2(0f,1f), new Vector2(1f,1f), new Vector2(0f, 1.5f));
        if (full || bottom) AddTrimStrip(parent, "TrimBottom", new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(0f, 1.5f));
        if (full || left)   AddTrimStrip(parent, "TrimLeft",   new Vector2(0f,0f), new Vector2(0f,1f), new Vector2(1.5f, 0f));
        if (full || right)  AddTrimStrip(parent, "TrimRight",  new Vector2(1f,0f), new Vector2(1f,1f), new Vector2(1.5f, 0f));
    }

    private void AddTrimStrip(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; rt.sizeDelta = sizeDelta;
        Image img = go.AddComponent<Image>(); img.color = _brass; img.raycastTarget = false;
    }

    private void AddVerticalDivider(GameObject parent, float x)
    {
        GameObject go = new GameObject("Divider"); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.1f); rt.anchorMax = new Vector2(0f, 0.9f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(x, 0f); rt.sizeDelta = new Vector2(1f, 0f);
        Image img = go.AddComponent<Image>(); img.color = _brassAlpha; img.raycastTarget = false;
    }

    private void AddOrnament(GameObject parent, float anchoredY, float width)
    {
        GameObject go = new GameObject("Ornament"); go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, anchoredY); rt.sizeDelta = new Vector2(-16f, 4f);
        Image img = go.AddComponent<Image>(); img.color = _brassAlpha; img.raycastTarget = false;
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
        tmp.raycastTarget = false;
        TMP_FontAsset chosen = (style == FontStyles.Bold && _fontBold != null) ? _fontBold
                             : (_fontRegular != null) ? _fontRegular
                             : null;
        if (chosen != null) tmp.font = chosen;
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
        Image img = go.AddComponent<Image>();
        img.color = bgColor;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonSound(); onClick?.Invoke(); });
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.12f, 1f), Mathf.Min(bgColor.g + 0.12f, 1f), Mathf.Min(bgColor.b + 0.06f, 1f));
        cb.pressedColor     = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f);
        cb.disabledColor    = new Color(0.20f, 0.16f, 0.12f, 0.6f);
        btn.colors = cb;

        GameObject lGo = new GameObject("Label"); lGo.transform.SetParent(go.transform, false);
        RectTransform lrt = lGo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = lGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 16; tmp.fontStyle = FontStyles.Bold;
        tmp.color = _cream; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return btn;
    }
}
