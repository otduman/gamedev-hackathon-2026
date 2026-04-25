# CLAUDE.md

Guidance for Claude Code when working in this repo.

## Project Overview

Unity 6 (URP) hex-grid strategy game inspired by Risk, themed around Kazakh nomadic clans (Argyn vs. Naiman). Two-player local hot-seat. First clan to **15 Prestige** — or to wipe the opponent off the map — wins.

- Engine: Unity 6 (`com.unity.render-pipelines.universal` 17.4.0)
- Input: New Input System (`UnityEngine.InputSystem`)
- UI: TextMesh Pro + uGUI (HUD built procedurally in code, no UI prefabs)
- Art: KayKit hex tiles + Meshy AI-generated rigged warrior animations (Idle / Attack / Walking / Alert)
- Single scene: `Assets/Scenes/SampleScene.unity` (also `Assets/1.unity`)
- Single bootstrap: `HexGridManager` builds the grid in `Start()`, then fires `OnGridReady` which `GameManager` and `CameraController` subscribe to. `AudioManager`, `AmbientEnvironment`, and `WaterAnimator` are auto-attached after grid generation.

## Core Game Loop

Each turn for the active clan runs three phases:

1. **Reinforce** — gain `max(3, ownedTiles/3)` warriors. Must be placed on the clan's **yurt** if one exists; otherwise on any owned tile.
2. **Action** — select an own tile with **2+ warriors**, attack an adjacent enemy/neutral tile (Risk-style dice). Multiple attacks per turn allowed.
3. **Migration** — one move per turn, with a **2 movement-point budget** through own territory. Plain/Pasture costs 1 MP, **Forest costs 2 MP**. Alternative: relocate the yurt to an adjacent own tile (always carries 1 warrior).

End-turn flow: `Action → Migration → next player's Reinforce`.

### Combat ([CombatManager.cs](Assets/Scripts/CombatManager.cs))
- Standard Risk dice: attacker rolls up to 3 (must leave 1 behind), defender up to 2.
- Sorted descending, paired comparison. **Defender wins ties.**
- **Forest bonus**: attacker on a forested tile rolls one extra die and discards the lowest.
- On conquest, surviving attackers move into the tile and it becomes **Contested** until the new owner survives their next Reinforce — contested tiles don't grant Prestige and don't count for reinforcement math.

### Prestige
- +1 per non-contested owned tile, **+2 per yurt**. Recalculated after every action.

## Architecture

Singleton-based managers, all in [Assets/Scripts/](Assets/Scripts/):

| Script | Role |
|---|---|
| [GameManager.cs](Assets/Scripts/GameManager.cs) | Turn/phase state machine, click + hover handling, selection/highlight bookkeeping, win condition |
| [HexGridManager.cs](Assets/Scripts/HexGridManager.cs) | Procedural axial-coord hex map (radius 3), terrain assignment, decoration placement, hex borders, background plane |
| [Tile.cs](Assets/Scripts/Tile.cs) | Per-tile state (terrain, owner, warriors, yurt, forest, contested) and visuals (warrior formations, owner tint via `MaterialPropertyBlock`, conquest pulse) |
| [CombatManager.cs](Assets/Scripts/CombatManager.cs) | Pure dice resolution — returns `CombatResult` for the UI to render |
| [UIManager.cs](Assets/Scripts/UIManager.cs) | **Builds the entire HUD programmatically at runtime** — no UI prefabs. Top bar, prestige panel, end-turn button, dice combat overlay, tooltip, game-over panel |
| [CameraController.cs](Assets/Scripts/CameraController.cs) | Civ-style tilt camera (40°→58° as you zoom in), WASD/arrow pan, scroll zoom, auto-fits grid on `OnGridReady` and on aspect change |
| [AudioManager.cs](Assets/Scripts/AudioManager.cs) | **Procedural SFX** — synthesizes attack/conquer/turn-start/migration clips at runtime (no audio assets) |
| [AmbientEnvironment.cs](Assets/Scripts/AmbientEnvironment.cs) | Dust particles, breathing sun light (Perlin noise), per-tile sway |
| [WaterAnimator.cs](Assets/Scripts/WaterAnimator.cs) | Per-water-tile color lerp + Y bob via `MaterialPropertyBlock` |

### Hex Coordinates
Axial `(q, r)` stored as `Vector2Int`. Neighbor offsets in `HexGridManager.GetNeighbors()`. World position is **pointy-top with 6% spread** between tiles (see `CalculateHexPosition`). Distance to origin uses cube-coord max formula in `HexDistanceFromOrigin`.

### Terrain Generation
`HexGridManager.DetermineTerrain` keeps inner rings (dist ≤ 2) passable (Plain/Pasture only) so every outer-ring tile borders something reachable. Outer ring may roll Mountain or Water. Forest is a **post-pass tag** set during `PlaceDecorations` — any Plain/Pasture tile that gets tree decorations becomes a forest (`Tile.HasForest = true`, `MoveCost = 2`, +1 attack die).

## Conventions

- **Singletons**: every manager exposes `public static Instance { get; private set; }` set in `Awake`. Always null-check before use (`if (UIManager.Instance != null) ...`).
- **Visual updates**: tile tinting goes through `MaterialPropertyBlock` (no per-tile material instances). Call `Tile.UpdateVisuals()` after any state change.
- **Highlighting**: `GameManager` tracks `_highlightedTiles` for the current selection — always `ClearHighlights()` before re-highlighting, `ClearSelection()` when phase changes.
- **No UI prefabs**: HUD elements are built in `UIManager.Awake` via `CreatePanel`/`CreateTMPLabel` factories. Add new HUD widgets there, not in the scene.
- **Procedural assets where practical**: SFX, hex borders (LineRenderer), background plane (loads `Assets/background-image.jpg` at runtime).
- **Animations**: warrior prefab uses Mecanim with clip indices `0=Idle, 1=Attack, 2=Walking`. `Tile.PlayAttackAnimation()` / `PlayWalkAnimation()` plays the clip then returns to idle.

## Adding Mechanics — Where Things Go

| Change | File |
|---|---|
| New phase or rule | `GameState` enum + handler in [GameManager.cs](Assets/Scripts/GameManager.cs) |
| New terrain type | `TerrainType` enum in [Tile.cs](Assets/Scripts/Tile.cs); add prefab array + case in `HexGridManager.GetPrefabForTerrain` and `DetermineTerrain` |
| Combat tweak (dice count, modifiers) | [CombatManager.cs](Assets/Scripts/CombatManager.cs) — keep it pure, return data via `CombatResult` |
| New tile property (e.g. fortress) | Field + setter on [Tile.cs](Assets/Scripts/Tile.cs); read it in `GameManager` rule checks; render in `UpdateVisuals` |
| HUD element | Add a `Build*` method in [UIManager.cs](Assets/Scripts/UIManager.cs), call from `BuildHUD()` |
| Hex math (range, pathfinding) | Extend [HexGridManager.cs](Assets/Scripts/HexGridManager.cs) — there's already `GetNeighbors`; add BFS helpers next to `GetMigrationReachable` pattern in `GameManager` |

## Running

Open the project in Unity 6, press Play in `SampleScene`. Everything bootstraps from `HexGridManager` on the scene's grid GameObject (it auto-attaches `AmbientEnvironment`/`WaterAnimator`/`AudioManager` if missing). No build steps; no test suite present.

## Specialized Agents Available

This repo ships with custom agents in [.claude/agents/](.claude/agents/):
- **unity-architect** — ScriptableObject-first, decoupled systems, anti-singleton refactors
- **unity-editor-tool-developer** — custom Inspectors, EditorWindows, property drawers
- **unity-multiplayer-engineer** — Netcode for GameObjects, lockstep, authority
- **unity-shader-graph-artist** — URP Shader Graph, VFX

Invoke them with the Agent tool when the task matches.
