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

    private GameObject          _gameOverPanel;
    private TextMeshProUGUI     _gameOverText;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    [SerializeField] private bool hudEnabled = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        EnsureCanvasComponents();
        if (hudEnabled) BuildHUD();
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
        BuildGameOverPanel();
    }

    // Top bar — turn, phase, and instruction message
    private void BuildTopBar()
    {
        GameObject panel = CreatePanel("TopBar", gameObject,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, -50f), new Vector2(Screen.width, 100f));

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
            "Clan 1 Prestige: 0", 15, FontStyles.Bold);
        _clan0PrestigeText.color = _clan0Color;

        _clan1PrestigeText = CreateTMPLabel("Clan1Prestige", panel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(200f, 30f),
            "Clan 2 Prestige: 0", 15, FontStyles.Bold);
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
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.25f, 0.70f, 0.25f);
        cb.pressedColor     = new Color(0.10f, 0.40f, 0.10f);
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
            _playerTurnText.text  = pid == 0 ? "Clan 1's Turn" : "Clan 2's Turn";
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
            _clan0PrestigeText.text = $"Clan 1 Prestige: {GameManager.Instance.PlayerPrestige[0]}";

        if (_clan1PrestigeText != null)
            _clan1PrestigeText.text = $"Clan 2 Prestige: {GameManager.Instance.PlayerPrestige[1]}";
    }

    // Updates the instruction text that guides the current player.
    public void ShowMessage(string message)
    {
        if (_messageText != null)
            _messageText.text = message;
    }

    // Displays a combat result banner for 2 seconds then hides it.
    public void ShowCombatResult(string message)
    {
        if (_combatResultPanel == null) return;

        _combatResultText.text = message;
        _combatResultPanel.SetActive(true);

        if (_combatHideCoroutine != null)
            StopCoroutine(_combatHideCoroutine);

        _combatHideCoroutine = StartCoroutine(HideCombatPanelAfterDelay(2f));
    }

    private IEnumerator HideCombatPanelAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_combatResultPanel != null)
            _combatResultPanel.SetActive(false);
    }

    // Shows the game-over panel with a clan-specific victory message.
    public void ShowGameOver(int winningPlayerId)
    {
        if (_gameOverPanel == null) return;

        string clanName = winningPlayerId == 0 ? "Clan 1" : "Clan 2";
        _gameOverText.text  = $"{clanName} is victorious!\nThe steppes bow to your Batyr.";
        _gameOverText.color = winningPlayerId == 0 ? _clan0Color : _clan1Color;
        _gameOverPanel.SetActive(true);
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
