# CLAUDE.md

Guidance for Claude Code when working in this repo.

> **⚡ IMPORTANT:** Read [`ARCHITECTURE.md`](ARCHITECTURE.md) before exploring any source files. It contains the full project structure, all file roles, state machine, game rules, data flows, and conventions. Do NOT re-read individual script files for orientation — everything you need is in that document.

## Project Overview

Unity 6 (URP) hex-grid strategy game inspired by Risk, themed around Kazakh nomadic clans. **4-player** local hot-seat (Argyn, Naiman, Kerey, Kongrat). Each player is assigned a **secret mission** — first to complete theirs wins.

- Engine: Unity 6 (`com.unity.render-pipelines.universal` 17.4.0)
- Input: New Input System (`UnityEngine.InputSystem`)
- UI: TextMesh Pro + uGUI (HUD built procedurally in code, no UI prefabs)
- Art: KayKit hex tiles + Meshy AI-generated rigged warrior animations
- Audio: MP3 sound effects loaded from `Assets/music/`
- Portraits: `Assets/portaits/` — Kazakh.png (Argyn), Timurid.png (Naiman), Rusya.png (Kerey), Ming.png (Kongrat). Assigned via `[SerializeField] Sprite[] _leaderPortraits` on the UIManager component (UI Canvas GameObject).
- Single scene: `Assets/Scenes/SampleScene.unity` (also `Assets/dd.unity`)
- Single bootstrap: `HexGridManager` builds the grid in `Start()`, then fires `OnGridReady` which `GameManager` and `CameraController` subscribe to. `AudioManager`, `AmbientEnvironment`, and `WaterAnimator` are auto-attached after grid generation.

## Game Flow (with Menu Screens)

The game now starts at **MainMenu** before any grid interaction:

```
MainMenu → [START GAME] → InitializeGame() → LeaderSelection → [BEGIN SETUP] → InitialPlacement → ...
```

`GameState` enum order: `MainMenu, InitialPlacement, Reinforce, Attack, CombatSetup, PostConquest, Fortify, GameOver`

- `GameManager.StartGame()` — called by START GAME button; triggers `InitializeGame()` (assigns tiles, missions), then shows LeaderSelection screen.
- `GameManager.BeginActualGame()` — called by BEGIN SETUP button; hides menu screens, starts `BeginInitialPlacement()`.
- `GameManager.RestartGame()` — reloads the scene (used by REPLAY and MAIN MENU buttons on Game Over).

## Core Game Loop

Game starts with an **Initial Placement** phase — each player places 15 extra troops in round-robin. Then turns cycle through 4 players:

1. **Reinforce** — gain `max(3, ownedTiles/3) + regionBonuses` troops. Place on any owned territory.
2. **Attack** — select an own tile with **2+ troops**, attack an adjacent enemy tile (Risk-style dice). Multiple attacks per turn allowed. Post-conquest troop advance is offered after each conquest.
3. **Fortify** — one move per turn through connected own territory (BFS connectivity).

End-turn flow: `Attack → Fortify → next alive player's Reinforce`.

The **End Phase button** is enabled during `InitialPlacement` (label: SKIP TURN → calls `OnRandomizePlacement`), `Attack` (END ATTACK), and `Fortify` (END TURN). Disabled during Reinforce.

### Combat ([CombatManager.cs](Assets/Scripts/CombatManager.cs))
- Standard Risk dice: attacker rolls up to 3 (must leave 1 behind), defender up to 2.
- Sorted descending, **reverse pairing** (attacker's highest vs defender's lowest). **Defender wins ties.**
- **Forest bonus**: attacker on a forested tile rolls one extra die and discards the lowest.
- On conquest, surviving attackers move into the tile. Post-conquest advance offered.

### Win Conditions (Missions)
Each player gets a secret mission at game start:
- **ControlTerritories** — control 24 territories
- **DestroyPlayer** — eliminate a specific clan
- **ControlTwoRegions** — control all passable tiles in 2 specific regions

### Regions & Bonuses
6 regions (angular sectors): Uly Zhuz (+5), Zhetysu (+5), Orta Zhuz (+4), Kishi Zhuz (+4), Dala (+3), Syr Darya (+3). Controlling all tiles in a region grants bonus reinforcements.

## Architecture

Singleton-based managers, all in [Assets/Scripts/](Assets/Scripts/):

| Script | Role |
|---|---|
| [GameManager.cs](Assets/Scripts/GameManager.cs) | Turn/phase state machine, click + hover handling, missions, idle hint timer, selection/highlight bookkeeping, win condition. New public methods: `StartGame`, `BeginActualGame`, `RestartGame`, `ReturnToMenu` |
| [HexGridManager.cs](Assets/Scripts/HexGridManager.cs) | Procedural axial-coord hex map (radius 4), terrain assignment, decoration placement, territory naming, hex borders, background plane |
| [Tile.cs](Assets/Scripts/Tile.cs) | Per-tile state and visuals. Troop count shown as a **world-space Canvas badge** (dark pill, clan-tinted bg, brass border, billboard via LateUpdate). Tile tint lerp: selected=0.62, highlighted=0.52 for strong visual feedback |
| [CombatManager.cs](Assets/Scripts/CombatManager.cs) | Pure dice resolution — returns `CombatResult` for the UI to render |
| [UIManager.cs](Assets/Scripts/UIManager.cs) | **Builds the entire HUD programmatically at runtime** — no UI prefabs. Screens: MainMenu, LeaderSelection, in-game HUD, GameOver. Key panels: top bar, mission panel, combat setup (600×300, clan-zoned), dice roll (600×260, clan-zoned), phase banner (dark bg + white text, 2.5s), idle hint badge, tooltip |
| [CameraController.cs](Assets/Scripts/CameraController.cs) | Civ-style tilt camera (40°→58° as you zoom in), WASD/arrow pan, scroll zoom, auto-fits grid on `OnGridReady` and on aspect change |
| [AudioManager.cs](Assets/Scripts/AudioManager.cs) | **MP3-based SFX** — plays combat, conquer, dice, and button sounds from `Assets/music/` |
| [AmbientEnvironment.cs](Assets/Scripts/AmbientEnvironment.cs) | Dust particles, breathing sun light (Perlin noise), per-tile sway |
| [WaterAnimator.cs](Assets/Scripts/WaterAnimator.cs) | Per-water-tile color lerp + Y bob via `MaterialPropertyBlock` |

### UI Screens (all procedural, no prefabs)

| Screen | Trigger | Key elements |
|---|---|---|
| **MainMenu** | On `OnGridReady` | Title, START GAME / QUIT buttons, clan colour dots |
| **LeaderSelection** | After `StartGame()` | 4 clan cards (168×290): 158×158 square portrait, clan name, leader name, secret mission text |
| **GameOver** | On win condition | Winner portrait (200×200), clan/leader name, mission line, REPLAY / MAIN MENU buttons |

### Tile Highlighting
- **Selected tile** (own, attacking from): yellow-gold tint, lerp 0.62
- **Attack targets** (enemy, adjacent): orange tint, lerp 0.52
- **Reinforce targets**: yellow-green tint, lerp 0.52
- All highlights go through `MaterialPropertyBlock` — never modify shared materials

### Combat UI
- **Combat Setup panel** (600×300): left zone = attacker (clan-tinted), right zone = defender (clan-tinted), VS divider, gold-bordered 54×54 die buttons, RESOLVE/CANCEL in bottom bar
- **Dice Roll panel** (600×260): attacker dice strictly in left half (centred at x=−148), defender in right half (x=+148) — no overlap; ▲/▼ win/loss indicators above attacker dice

### Idle Hint System
`GameManager.TickIdleHint()` runs every frame. After **12 seconds** of no player action during an actionable phase, `UIManager.ShowIdleHint(message)` displays a 520×48 pill badge at bottom-center with a phase-specific prompt. Resets on any tile click or End Phase button press.

### Hex Coordinates
Axial `(q, r)` stored as `Vector2Int`. Neighbor offsets in `HexGridManager.GetNeighbors()`. World position is **pointy-top with 6% spread** between tiles (see `CalculateHexPosition`). Distance to origin uses cube-coord max formula in `HexDistanceFromOrigin`.

### Terrain Generation
`HexGridManager.DetermineTerrain` keeps inner rings (dist ≤ 2) as Plain. Distance 3 has mixed terrain (72% Plain, 16% Mountain, 12% Water). Outer ring is always Mountain (natural border). Forest is a **post-pass tag** set during `PlaceDecorations` — any Plain tile that gets tree decorations becomes a forest (`Tile.HasForest = true`, +1 attack die).

### Tile Colouring
Tile surfaces are tinted by **continent/region** (Risk board style — gold, orange, blue, green, tan, rose). Warriors and borders are coloured by **owner** (green, red, blue, gold — matching the UIManager hi-fi palette).

## Conventions

- **Singletons**: every manager exposes `public static Instance { get; private set; }` set in `Awake`. Always null-check before use (`if (UIManager.Instance != null) ...`).
- **Visual updates**: tile tinting goes through `MaterialPropertyBlock` (no per-tile material instances). Call `Tile.UpdateVisuals()` after any state change.
- **Highlighting**: `GameManager` tracks `_highlightedTiles` for the current selection — always `ClearHighlights()` before re-highlighting, `ClearSelection()` when phase changes.
- **No UI prefabs**: HUD elements are built in `UIManager.Awake` via `CreatePanel`/`CreateTMPLabel`/`CreateButton` factories. Add new HUD widgets there, not in the scene.
- **Screen switching**: `UIManager.ShowMainMenu()`, `ShowLeaderSelection()`, `HideMenuScreens()` toggle `_topBarGo`, `_rightPanelGo`, `_mainMenuPanel`, `_leaderSelectionPanel`. The HUD starts hidden; shown only after `BeginActualGame()`.
- **Procedural assets where practical**: hex borders (LineRenderer), background plane (loads `Assets/background-image.jpg` at runtime).
- **Animations**: warrior clips found by keyword search ("Attack", "Walk"). `Tile.PlayAttackAnimation()` / `PlayWalkAnimation()` plays the clip then returns to idle.
- **Turn order**: eliminated players (no tiles) are automatically skipped.
- **Idle timer**: `GameManager._idleTimer` resets on `ResetIdleTimer()` — call this whenever the player takes any action.

## Adding Mechanics — Where Things Go

| Change | File |
|---|---|
| New phase or rule | `GameState` enum + handler in [GameManager.cs](Assets/Scripts/GameManager.cs) |
| New terrain type | `TerrainType` enum in [Tile.cs](Assets/Scripts/Tile.cs); add prefab array + case in `HexGridManager.GetPrefabForTerrain` and `DetermineTerrain` |
| Combat tweak (dice count, modifiers) | [CombatManager.cs](Assets/Scripts/CombatManager.cs) — keep it pure, return data via `CombatResult` |
| New tile property (e.g. fortress) | Field + setter on [Tile.cs](Assets/Scripts/Tile.cs); read it in `GameManager` rule checks; render in `UpdateVisuals` |
| HUD element | Add a `Build*` method in [UIManager.cs](Assets/Scripts/UIManager.cs), call from `BuildHUD()` |
| New menu screen | Add `Build*`, `Show*` methods in [UIManager.cs](Assets/Scripts/UIManager.cs); add toggle logic to `ShowMainMenu`/`HideMenuScreens` |
| Hex math (range, pathfinding) | Extend [HexGridManager.cs](Assets/Scripts/HexGridManager.cs) — there's already `GetNeighbors`; add BFS helpers next to `GetConnectedOwn` pattern in `GameManager` |
| New mission type | Add to `MissionType` enum in [GameManager.cs](Assets/Scripts/GameManager.cs), add cases in `AssignMissions()` and `MissionComplete()` |
| New sound effect | Add MP3 to `Assets/music/`, add `SerializeField` + `Play*` method in [AudioManager.cs](Assets/Scripts/AudioManager.cs) |
| New idle hint | Add entry to `IdleHints[]` array in [GameManager.cs](Assets/Scripts/GameManager.cs) matching the `GameState` enum index |
| New leader portrait | Add PNG to `Assets/portaits/`, import as **Sprite (2D and UI)**, assign to `_leaderPortraits` array on the UI Canvas GameObject in Inspector |

## Running

Open the project in Unity 6, press Play in `SampleScene`. Everything bootstraps from `HexGridManager` on the scene's grid GameObject. Game starts at **MainMenu** — click START GAME to proceed to leader selection, then BEGIN SETUP to start placement. No build steps; no test suite present.

## Specialized Agents Available

This repo ships with custom agents in [.claude/agents/](.claude/agents/):
- **unity-architect** — ScriptableObject-first, decoupled systems, anti-singleton refactors
- **unity-editor-tool-developer** — custom Inspectors, EditorWindows, property drawers
- **unity-multiplayer-engineer** — Netcode for GameObjects, lockstep, authority
- **unity-shader-graph-artist** — URP Shader Graph, VFX

Invoke them with the Agent tool when the task matches.
