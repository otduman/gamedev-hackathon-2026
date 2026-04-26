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
- Single scene: `Assets/Scenes/SampleScene.unity` (also `Assets/dd.unity`)
- Single bootstrap: `HexGridManager` builds the grid in `Start()`, then fires `OnGridReady` which `GameManager` and `CameraController` subscribe to. `AudioManager`, `AmbientEnvironment`, and `WaterAnimator` are auto-attached after grid generation.

## Core Game Loop

Game starts with an **Initial Placement** phase — each player places 15 extra troops in round-robin. Then turns cycle through 4 players:

1. **Reinforce** — gain `max(3, ownedTiles/3) + regionBonuses` troops. Place on any owned territory.
2. **Attack** — select an own tile with **2+ troops**, attack an adjacent enemy tile (Risk-style dice). Multiple attacks per turn allowed. Post-conquest troop advance is offered after each conquest.
3. **Fortify** — one move per turn through connected own territory (BFS connectivity).

End-turn flow: `Attack → Fortify → next alive player's Reinforce`.

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
| [GameManager.cs](Assets/Scripts/GameManager.cs) | Turn/phase state machine, click + hover handling, missions, selection/highlight bookkeeping, win condition |
| [HexGridManager.cs](Assets/Scripts/HexGridManager.cs) | Procedural axial-coord hex map (radius 4), terrain assignment, decoration placement, territory naming, hex borders, background plane |
| [Tile.cs](Assets/Scripts/Tile.cs) | Per-tile state (terrain, owner, troops, forest, territory name, continent) and visuals (warrior formations, region tinting via `MaterialPropertyBlock`, conquest pulse) |
| [CombatManager.cs](Assets/Scripts/CombatManager.cs) | Pure dice resolution — returns `CombatResult` for the UI to render |
| [UIManager.cs](Assets/Scripts/UIManager.cs) | **Builds the entire HUD programmatically at runtime** — no UI prefabs. Top bar, mission panel, placement panel, end-turn button, dice combat overlay, phase banner, tooltip, game-over panel |
| [CameraController.cs](Assets/Scripts/CameraController.cs) | Civ-style tilt camera (40°→58° as you zoom in), WASD/arrow pan, scroll zoom, auto-fits grid on `OnGridReady` and on aspect change |
| [AudioManager.cs](Assets/Scripts/AudioManager.cs) | **MP3-based SFX** — plays combat, conquer, dice, and button sounds from `Assets/music/` |
| [AmbientEnvironment.cs](Assets/Scripts/AmbientEnvironment.cs) | Dust particles, breathing sun light (Perlin noise), per-tile sway |
| [WaterAnimator.cs](Assets/Scripts/WaterAnimator.cs) | Per-water-tile color lerp + Y bob via `MaterialPropertyBlock` |

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
- **Procedural assets where practical**: hex borders (LineRenderer), background plane (loads `Assets/background-image.jpg` at runtime).
- **Animations**: warrior clips found by keyword search ("Attack", "Walk"). `Tile.PlayAttackAnimation()` / `PlayWalkAnimation()` plays the clip then returns to idle.
- **Turn order**: eliminated players (no tiles) are automatically skipped.

## Adding Mechanics — Where Things Go

| Change | File |
|---|---|
| New phase or rule | `GameState` enum + handler in [GameManager.cs](Assets/Scripts/GameManager.cs) |
| New terrain type | `TerrainType` enum in [Tile.cs](Assets/Scripts/Tile.cs); add prefab array + case in `HexGridManager.GetPrefabForTerrain` and `DetermineTerrain` |
| Combat tweak (dice count, modifiers) | [CombatManager.cs](Assets/Scripts/CombatManager.cs) — keep it pure, return data via `CombatResult` |
| New tile property (e.g. fortress) | Field + setter on [Tile.cs](Assets/Scripts/Tile.cs); read it in `GameManager` rule checks; render in `UpdateVisuals` |
| HUD element | Add a `Build*` method in [UIManager.cs](Assets/Scripts/UIManager.cs), call from `BuildHUD()` |
| Hex math (range, pathfinding) | Extend [HexGridManager.cs](Assets/Scripts/HexGridManager.cs) — there's already `GetNeighbors`; add BFS helpers next to `GetConnectedOwn` pattern in `GameManager` |
| New mission type | Add to `MissionType` enum in [GameManager.cs](Assets/Scripts/GameManager.cs), add cases in `AssignMissions()` and `MissionComplete()` |
| New sound effect | Add MP3 to `Assets/music/`, add `SerializeField` + `Play*` method in [AudioManager.cs](Assets/Scripts/AudioManager.cs) |

## Running

Open the project in Unity 6, press Play in `SampleScene`. Everything bootstraps from `HexGridManager` on the scene's grid GameObject (it auto-attaches `AmbientEnvironment`/`WaterAnimator`/`AudioManager` if missing). No build steps; no test suite present.

## Specialized Agents Available

This repo ships with custom agents in [.claude/agents/](.claude/agents/):
- **unity-architect** — ScriptableObject-first, decoupled systems, anti-singleton refactors
- **unity-editor-tool-developer** — custom Inspectors, EditorWindows, property drawers
- **unity-multiplayer-engineer** — Netcode for GameObjects, lockstep, authority
- **unity-shader-graph-artist** — URP Shader Graph, VFX

Invoke them with the Agent tool when the task matches.
