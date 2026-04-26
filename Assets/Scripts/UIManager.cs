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

    // Top bar
    private Image            _leaderPanelBg;
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
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

        GameObject esGo = new GameObject("EventSystem");
        DontDestroyOnLoad(esGo);
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();

        System.Type nisType = System.Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (nisType != null)
            esGo.AddComponent(nisType);
        else
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private void LoadFonts()
    {
#if UNITY_EDITOR
        _fontRegular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Cinzel/Cinzel-Regular SDF.asset");
        _fontBold    = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Cinzel/Cinzel-Bold SDF.asset");
#endif
        if (_fontRegular == null || _fontBold == null)
            Debug.LogWarning("[UIManager] Cinzel SDF fonts not found — falling back to TMP default.");
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
        RectTransform brt = bar.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f); brt.anchorMax = new Vector2(1f, 1f);
        brt.pivot = new Vector2(0.5f, 1f);
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta = new Vector2(-rightPanelW, barH); // leave right panel gap
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
        _leaderPanelBg.color = new Color(_clan0Color.r, _clan0Color.g, _clan0Color.b, 0.18f);
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

        _leaderNameText = CreateTMPLabel("LeaderName", leaderPanel,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(26f, 7f), new Vector2(-30f, 20f),
            "Argyn's Turn", 13, FontStyles.Bold);
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

        // ── Hint text (flex) ──────────────────────────────────────────────
        float hintLeft = pillsStart + 4 * (pillW + pillGap) + 14f;
        const float endBtnW = 56f;
        _phaseHintText = CreateTMPLabel("PhaseHint", bar,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f),
            new Vector2(hintLeft, 0f), new Vector2(-(hintLeft + endBtnW + rightPanelW + 12f), 0f),
            "Place your initial warriors", 11, FontStyles.Italic);
        _phaseHintText.color = _goldDim;
        _phaseHintText.alignment = TextAlignmentOptions.Left;

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
        ert.anchoredPosition = new Vector2(-(rightPanelW + 8f), 0f);
        ert.sizeDelta = new Vector2(endBtnW, endBtnW);
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
            new Vector2(0f, -8f), new Vector2(0f, 16f),
            "SECRET MISSION", 8, FontStyles.Bold);
        mTitle.color = _goldDim;
        mTitle.alignment = TextAlignmentOptions.Center;

        _missionLabel = CreateTMPLabel("MissionLabel", _missionPanel,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 4f), new Vector2(-12f, 48f), "", 10, FontStyles.Italic);
        _missionLabel.color = new Color(0.12f, 0.09f, 0.06f);
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
        string[] defaultNames = { "Argyn", "Naiman", "Kerey", "Kongrat" };
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
            Vector2.zero, new Vector2(560f, 220f));
        _diceCombatPanel.GetComponent<Image>().color = _panelColor;
        AddGoldBorder(_diceCombatPanel);
        _diceCombatPanel.SetActive(false);
    }

    private void BuildGameOverPanel()
    {
        _gameOverPanel = CreatePanel("GameOverPanel", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(620f, 340f));
        _gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);
        AddGoldBorder(_gameOverPanel);

        _gameOverText = CreateTMPLabel("GameOverText", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 60f), new Vector2(580f, 140f), "Game Over", 30, FontStyles.Bold);
        _gameOverText.alignment = TextAlignmentOptions.Center;
        _gameOverText.color = _goldColor;

        CreateButton("RestartButton", _gameOverPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -90f), new Vector2(200f, 55f),
            "Play Again", new Color(0.490f, 0.082f, 0.082f),
            () => UnityEngine.SceneManagement.SceneManager.LoadScene(0));

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
        _phaseBannerPanel = CreatePanel("PhaseBanner", gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(480f, 80f));
        _phaseBannerPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        _phaseBannerPanel.GetComponent<Image>().raycastTarget = false;

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
        Color fc    = GetClanColor(pid);

        // ── Leader panel ──────────────────────────────────────────────────
        if (_leaderPanelBg != null)
            _leaderPanelBg.color = new Color(fc.r, fc.g, fc.b, 0.20f);

        if (_leaderNameText != null)
        {
            _leaderNameText.text  = $"{GameManager.Instance.GetClanName(pid)}'s Turn";
            _leaderNameText.color = _cream;
        }

        if (_leaderEmpireText != null)
        {
            _leaderEmpireText.text = GameManager.Instance.CurrentState switch
            {
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
            _endTurnButton.interactable = s == GameState.Attack || s == GameState.Fortify;
            if (_endTurnBg != null)
            {
                bool active = s == GameState.Attack || s == GameState.Fortify;
                _endTurnBg.color = active
                    ? new Color(0.165f, 0.122f, 0.063f, 0.96f)  // warm gold tint
                    : new Color(0.082f, 0.059f, 0.047f, 0.96f);
            }
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
        summary.color = _cream;

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
        _combatSetupPanel.GetComponent<Image>().color = _panelColor;
        AddGoldBorder(_combatSetupPanel);

        var title = CreateTMPLabel("Title", _combatSetupPanel,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -22f), new Vector2(540f, 32f), "COMBAT SETUP", 22, FontStyles.Bold);
        title.color = _goldColor;

        _atkHeader = CreateTMPLabel("AtkHeader", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, 75f), new Vector2(220f, 24f), "Attacker dice", 14, FontStyles.Bold);
        _atkHeader.color = _clan1Color;

        _atkDiceButtons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            int n = i + 1;
            _atkDiceButtons[i] = CreateButton($"AtkDice{n}", _combatSetupPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-180f + i * 50f, 30f), new Vector2(46f, 46f),
                n.ToString(), new Color(0.25f, 0.09f, 0.07f), () => SetAtkDice(n));
        }

        _atkDiceLabel = CreateTMPLabel("AtkLabel", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-130f, -10f), new Vector2(220f, 22f), "Selected: 0", 13, FontStyles.Normal);
        _atkDiceLabel.color = _creamDim;

        _defHeader = CreateTMPLabel("DefHeader", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, 75f), new Vector2(220f, 24f), "Defender dice", 14, FontStyles.Bold);
        _defHeader.color = _clan2Color;

        _defDiceButtons = new Button[2];
        for (int i = 0; i < 2; i++)
        {
            int n = i + 1;
            _defDiceButtons[i] = CreateButton($"DefDice{n}", _combatSetupPanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(80f + i * 50f, 30f), new Vector2(46f, 46f),
                n.ToString(), new Color(0.07f, 0.14f, 0.24f), () => SetDefDice(n));
        }

        _defDiceLabel = CreateTMPLabel("DefLabel", _combatSetupPanel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, -10f), new Vector2(220f, 22f), "Selected: 0", 13, FontStyles.Normal);
        _defDiceLabel.color = _creamDim;

        CreateButton("ResolveBtn", _combatSetupPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(-80f, 30f), new Vector2(140f, 44f),
            "Resolve", new Color(0.490f, 0.082f, 0.082f),
            () => GameManager.Instance?.OnCombatSetupConfirm(_selectedAtkDice, _selectedDefDice));

        CreateButton("CancelBtn", _combatSetupPanel,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(80f, 30f), new Vector2(140f, 44f),
            "Cancel", new Color(0.20f, 0.16f, 0.11f),
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
        _migrationCountPanel.GetComponent<Image>().color = new Color(0.047f, 0.082f, 0.047f, 0.94f);
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
