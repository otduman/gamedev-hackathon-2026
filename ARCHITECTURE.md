# Project Architecture — Kazakh Nomadic Hex Strategy Game

> **Read this file first.** It contains everything you need to understand the codebase before making changes. Designed to save token budget on every prompt.

---

## Quick Facts

| Key | Value |
|---|---|
| Engine | Unity 6 (URP 17.4.0) |
| Language | C# |
| Input | New Input System (`UnityEngine.InputSystem`) |
| UI | TextMesh Pro + uGUI — **100% built in code**, zero UI prefabs |
| Audio | MP3 files loaded from `Assets/music/` — combat, conquer, dice, button SFX |
| Art | KayKit hex tiles + Meshy AI warrior FBX (rigged animations) |
| Scene | Single scene: `Assets/Scenes/SampleScene.unity` (also `Assets/dd.unity`) |
| Win | Complete your **secret mission** (control territories, destroy a clan, or control 2 regions) |
| Players | **4-player** local hot-seat (Argyn, Naiman, Kerey, Kongrat) |

---

## Directory Map

```
gamedev-hackathon-2026/
├── Assets/
│   ├── Scripts/               ← ALL gameplay code lives here (9 .cs files)
│   │   ├── GameManager.cs         (766 lines)  — Turn state machine, input, rules, missions
│   │   ├── HexGridManager.cs      (678 lines)  — Procedural hex grid, terrain, decorations, borders
│   │   ├── Tile.cs                 (511 lines)  — Per-tile state & visuals
│   │   ├── UIManager.cs            (981 lines)  — Builds entire HUD programmatically
│   │   ├── CombatManager.cs        (104 lines)  — Pure dice combat resolution
│   │   ├── CameraController.cs     (167 lines)  — Civ-style zoom/pan camera
│   │   ├── AudioManager.cs         (49 lines)   — MP3-based SFX playback
│   │   ├── AmbientEnvironment.cs   (141 lines)  — Dust particles, light breathing, tile sway
│   │   └── WaterAnimator.cs        (101 lines)  — Water tile colour lerp + Y bob
│   ├── Editor/                ← Editor-only tooling (not included in builds)
│   │   ├── HackathonAssetSetup.cs  — Menu item "Hackathon/Auto-Setup Assets"
│   │   └── FBXImportSettings.cs    — Strips cameras/lights from imported FBX
│   ├── Prefabs/               ← Prefabs (created by HackathonAssetSetup)
│   │   ├── Tile_Grass.prefab      — Used for Plain terrain
│   │   ├── Tile_Water.prefab
│   │   ├── Tile_Mountain.prefab
│   │   ├── WarriorUnit.prefab     — Meshy AI biped with Animator
│   │   └── YurtPrefab.prefab      — Nomadic base building
│   ├── music/                 ← Sound effect MP3 files
│   │   ├── sfx_combat.mp3
│   │   ├── sfx_conque.mp3
│   │   ├── sfx_dice.mp3
│   │   ├── sfx_button.mp3
│   │   └── Relaxing_Dombyra_Music_*.mp3 — Background music track
│   ├── KayKit/                ← 3rd-party hex tile + decoration FBX models
│   │   ├── fbx(unity)/tiles/      — hex_grass, hex_water, hex_mountain
│   │   ├── fbx(unity)/buildings/  — yurt model
│   │   ├── fbx(unity)/decoration/ — trees, hills, rocks, mountains
│   │   └── Meshy_AI_biped_*.fbx   — Warrior animations
│   ├── Scenes/
│   │   └── SampleScene.unity      — The main scene, everything bootstraps from here
│   ├── dd.unity                   — Secondary scene file
│   ├── background-image.jpg       — Loaded at runtime for the ground plane
│   └── InputSystem_Actions.inputactions
├── .claude/agents/            ← Specialised Claude agent configs
├── CLAUDE.md                  ← Claude Code context file
├── ARCHITECTURE.md            ← This file
├── ProjectSettings/           ← Unity project settings (URP, input, quality)
└── Packages/                  ← Unity Package Manager manifests
```

---

## Bootstrap Flow

```
Scene loads
  └─► HexGridManager.Awake() — sets Instance singleton, ensures EventSystem
  └─► HexGridManager.Start()
        └─► AutoPopulateAssets() — loads KayKit assets from known paths (Editor only)
        └─► GenerateHexagonalMap()
              ├── Creates all Tile GameObjects (axial hex grid, radius 4)
              ├── PlaceDecorations() — trees/rocks/hills/mountains + marks forest tiles
              ├── AssignTerritoryNames() — gives each passable tile a Kazakh city name + continent ID
              ├── AddHexBorders() — LineRenderer outlines (clan-coloured)
              ├── CreateBackgroundPlane() — loads background-image.jpg
              └── OnGridReady?.Invoke()
                    ├── GameManager.InitializeGame()
                    │     ├── Distributes all passable tiles evenly among 4 players
                    │     ├── Assigns secret missions to each player
                    │     └── BeginInitialPlacement() — round-robin troop placement
                    └── CameraController.FitToGrid()
        └─► Auto-attaches: AmbientEnvironment, WaterAnimator, AudioManager
```

**All managers live on a single "Game Controllers" GameObject** (created by HackathonAssetSetup). UIManager lives on a separate "UI Canvas" GameObject.

---

## Singleton Pattern

Every manager uses the same pattern:

```csharp
public static ManagerName Instance { get; private set; }
private void Awake() {
    if (Instance == null) Instance = this;
    else { Destroy(gameObject); return; }
}
```

**Always null-check before use:** `if (UIManager.Instance != null) UIManager.Instance.DoThing();`

Active singletons: `GameManager`, `HexGridManager`, `CombatManager`, `UIManager`, `AudioManager`

---

## Game State Machine

```
GameState enum:
  InitialPlacement → Reinforce → Attack → CombatSetup → PostConquest → Attack → Fortify → (next player's Reinforce)
                                                                                             └──── GameOver
```

| State | What happens | User input |
|---|---|---|
| `InitialPlacement` | Each player places 15 extra troops in round-robin | Click any owned territory to add 1 troop (or use Randomize buttons) |
| `Reinforce` | Player gains `max(3, ownedTiles/3) + regionBonuses` troops | Click any owned territory to place troops |
| `Attack` | Player selects own tile (2+ troops) and attacks adjacent enemy | Click source → click target; repeat; End Attack when done |
| `CombatSetup` | Modal panel — both sides pick dice count | Click 1/2/3 buttons → Resolve or Cancel |
| `PostConquest` | Post-conquest troop advance (optional) | +/- slider → Confirm or Cancel |
| `Fortify` | One move per turn through connected own territory | Click source → click destination → pick troop count |
| `GameOver` | Victory screen shown, "Play Again" button reloads scene | — |

**Turn flow:** End Attack → starts Fortify. End Turn during Fortify → switches to next alive player, starts Reinforce.

---

## File-by-File Reference

### GameManager.cs (766 lines)
**Role:** Central controller — turn/phase state machine, click/hover handling, selection/highlight bookkeeping, missions, win conditions.

**Key fields:**
- `CurrentState` (GameState enum) — drives all input routing
- `CurrentPlayerId` (0–3: Argyn, Naiman, Kerey, Kongrat)
- `PlayerCount` = 4 (const)
- `_playerMissions[4]` — one secret Mission per player
- `_selectedTile`, `_hoveredTile` — current interaction state
- `_highlightedTiles` — list of tiles with active highlights (always `ClearHighlights()` before re-highlighting)
- `_fortifySource`, `_fortifyDest` — fortify move endpoints
- `_postConquestSource`, `_postConquestDest` — post-conquest advance tiles

**Key methods:**
| Method | Purpose |
|---|---|
| `InitializeGame()` | Distributes tiles, assigns missions, begins placement |
| `HandleClick()` | Routes clicks by `CurrentState` to appropriate handler |
| `HandlePlacementClick()` | Places 1 troop on owned tile during initial placement |
| `HandleReinforceClick()` | Places reinforcement troops on any owned tile |
| `HandleAttackClick()` | Selects source, validates target, initiates combat |
| `HandleFortifyClick()` | Handles troop movement through connected territory |
| `BeginCombatSetup()` | Opens dice selection modal |
| `OnCombatSetupConfirm(atk, def)` | Called by UIManager button — resolves combat |
| `OnEndTurnButtonClicked()` | Attack→Fortify or Fortify→next player |
| `CalculateReinforcements()` | `max(3, owned/3) + region bonuses` |
| `CheckWinCondition()` | Checks all players' mission completion |
| `CheckPlayerEliminated()` | Checks if a player has no tiles left |
| `GetConnectedOwn()` | BFS through own territory for fortify targets |
| `ClearSelection()` / `ClearHighlights()` | Reset interaction state |

**Highlight colours:**
- Orange `(1.0, 0.55, 0.0)` — attack targets
- Green `(0.25, 1.0, 0.5)` — fortify targets
- Yellow `(1.0, 0.85, 0.1)` — reinforce/placement targets

---

### HexGridManager.cs (678 lines)
**Role:** Procedural hex grid generation, terrain assignment, decoration placement, territory naming, hex borders.

**Key concepts:**
- **Axial coordinates** `(q, r)` stored as `Vector2Int`
- **Pointy-top hex layout** with 6% spread between tiles
- **Grid radius:** 4 (61 total hexes)
- **Terrain generation:** Inner rings (dist ≤ 2) always Plain. Distance 3 has mixed terrain (72% Plain, 16% Mountain, 12% Water). Outer ring is always Mountain (natural border).
- **Forest:** Post-pass tag — Plain tiles with tree decorations become forests (`HasForest = true`, +1 attack die)
- **Territory names:** 60 Kazakh city names assigned randomly to passable tiles
- **Continents/Regions:** 6 angular sectors, each tile assigned a continent ID (0–5)
- `Grid` dictionary: `Dictionary<Vector2Int, Tile>` — the single source of truth for all tiles

**Key methods:**
| Method | Purpose |
|---|---|
| `GenerateHexagonalMap()` | Main generation loop |
| `AutoPopulateAssets()` | Auto-loads KayKit assets from known paths (Editor only) |
| `CalculateHexPosition()` | Axial coords → world position |
| `DetermineTerrain(q, r)` | Random terrain based on distance from origin |
| `GetPrefabForTerrain()` | Maps TerrainType → prefab array |
| `GetNeighbors(Vector2Int)` | Returns list of adjacent Tile objects (6 directions) |
| `PlaceDecorations()` | Spawns trees/rocks/hills/mountains, marks forest tiles |
| `AssignTerritoryNames()` | Assigns Kazakh city names and continent IDs |
| `AddHexBorders()` / `RefreshAllBorders()` | Clan-coloured LineRenderer borders |
| `CreateFallbackTile()` | Fallback cylinder if no prefab assigned |

**Neighbor directions (axial):**
```
(+1,0) (+1,-1) (0,-1) (-1,0) (-1,+1) (0,+1)
```

---

### Tile.cs (511 lines)
**Role:** Per-tile state container and visual representation.

**Enums defined here:**
```csharp
enum TerrainType { Plain, Mountain, Water }
```

**Key properties:**
| Property | Type | Notes |
|---|---|---|
| `HexCoordinates` | `Vector2Int` | Axial (q, r) — set once in `Initialize()` |
| `Terrain` | `TerrainType` | Set once in `Initialize()` |
| `OwnerPlayerId` | `int` | -1 = neutral, 0–3 = clan |
| `CubeCount` | `int` | Number of troops on this tile |
| `HasYurt` | `bool` | Not actively used in current game rules |
| `HasForest` | `bool` | +1 attack die |
| `TerritoryName` | `string` | Kazakh city name displayed in tooltip |
| `ContinentId` | `int` | Region/continent assignment (0–5) |

**Visual system:**
- Tinting via `MaterialPropertyBlock` (no per-tile material clones)
- **Tile surfaces** coloured by continent/region (Risk board style), NOT by owner
- **Warriors and borders** coloured by owner: Argyn blue `(0.20, 0.40, 1.0)`, Naiman red `(1.0, 0.28, 0.28)`, Kerey green `(0.15, 0.80, 0.30)`, Kongrat purple `(0.75, 0.20, 0.90)`
- Max 3 warriors shown visually (triangle formation); count label for overflow (`TextMeshPro`)
- Yurt: instantiated from prefab, auto-scaled via `AutoScaleYurt` coroutine
- Decorations: tracked via `_decoRenderers` list, opacity changes on hover/occupation
- Conquest pulse: coroutine flashes tile white over 0.55s
- `SpawnTraveler()` — animated warrior walks between tiles for fortify/advance moves

**Animation:** `PlayAttackAnimation()` / `PlayWalkAnimation()` — temporary clip by keyword search, returns to idle.

**Important:** Always call `UpdateVisuals()` after any state change on a tile.

---

### CombatManager.cs (104 lines)
**Role:** Pure dice resolution — no side effects beyond warrior counts.

**`CombatResult` data class:**
```csharp
int[]  AttackerRolls      // all dice, sorted descending (includes forest bonus die)
int[]  DefenderRolls      // sorted descending
int    AttackerLosses
int    DefenderLosses
bool   Conquered          // true if defender reduced to 0
bool   ForestBonus        // true if attacker was on forest tile
int    AttackingDiceUsed  // top N dice used (excludes discarded forest die)
```

**Combat rules:**
- Attacker: 1–3 dice (must leave 1 warrior behind)
- Defender: 1–2 dice (up to their warrior count)
- **Forest bonus:** Roll 1 extra die, discard lowest
- **Reverse pairing:** Attacker's highest vs defender's LOWEST (prevents a single high roll from sweeping both pairs)
- **Defender wins ties**
- On conquest: surviving attackers move into tile; ownership set by GameManager

---

### UIManager.cs (981 lines)
**Role:** Builds the entire HUD programmatically in `Awake()`. No UI prefabs exist.

**HUD components built in `BuildHUD()`:**
1. **Top bar** — player turn label, phase text, instruction message
2. **Mission panel** — bottom-right, shows current player's secret mission + progress
3. **Placement panel** — bottom-center, "Randomize for me" and "Auto-fill all" buttons (initial placement only)
4. **End Turn button** — bottom-center, label changes to "End Attack" during attack phase
5. **Combat result panel** — centre overlay, auto-hides after 2s
6. **Dice combat panel** — centre, animated dice with Monopoly-style shake, auto-hides after 3s
7. **Phase banner** — centre, fade-in/out phase name overlay
8. **Game Over panel** — centre, with mission description + "Play Again" button (reloads scene)
9. **Tooltip panel** — bottom-left, shows hovered tile info (name, region, terrain, owner, troops)
10. **Combat setup panel** — modal, attacker/defender dice selection (1/2/3 buttons)
11. **Migration count panel** — modal, +/- warriors to move (post-conquest advance + fortify)

**Factory methods:**
- `CreatePanel()` — anchored panel with `Image` background
- `CreateTMPLabel()` — positioned `TextMeshProUGUI` element
- `CreateButton()` — button with Image + click handler + label
- `AddGoldTrim()` — decorative gold border on panels
- `SpawnDie()` — visual dice element for combat display

**Key public API:**
| Method | Called by |
|---|---|
| `UpdateUI()` | GameManager — refreshes all HUD text from current state |
| `ShowMessage(string)` | GameManager — sets instruction text |
| `ShowPhaseTransition(label, color)` | GameManager — animated phase banner |
| `ShowCombatResult(CombatResult, ...)` | GameManager — animated dice panel |
| `ShowCombatSetup(atkPid, defPid, maxAtk, maxDef, ...)` | GameManager — opens dice selection modal |
| `HideCombatSetup()` | GameManager — closes dice selection |
| `ShowMigrationCount(max, default, title)` | GameManager — opens troop count picker |
| `HideMigrationCount()` | GameManager — closes troop count picker |
| `ShowTooltip(Tile)` / `HideTooltip()` | GameManager — hover info |
| `ShowGameOver(winningPlayerId, Mission)` | GameManager — victory overlay with mission |
| `GetClanColor(int)` | Various — returns clan colour for player ID |

**To add a new HUD element:** Create a `Build*` method, call it from `BuildHUD()`.

---

### CameraController.cs (167 lines)
**Role:** Civ-style camera with zoom-dependent tilt.

- **Tilt range:** 40° (zoomed in) → 58° (zoomed out)
- **Controls:** WASD / Arrow keys for pan, scroll wheel for zoom
- **Auto-fit:** Computes zoom to fit all tiles on `OnGridReady` and on aspect ratio changes
- **Start zoom:** 55% of max (close-up)

---

### AudioManager.cs (49 lines)
**Role:** Plays SFX from MP3 audio files in `Assets/music/`.

| Method | Clip File |
|---|---|
| `PlayCombatSound()` | `sfx_combat.mp3` |
| `PlayConquerSound()` | `sfx_conque.mp3` |
| `PlayDiceSound()` | `sfx_dice.mp3` |
| `PlayButtonSound()` | `sfx_button.mp3` |

Clips are loaded via `SerializeField` or auto-loaded from `Assets/music/` paths in Editor mode. Single `AudioSource`, no volume control exposed.

---

### AmbientEnvironment.cs (141 lines)
**Role:** Atmospheric effects.

- **Dust particles:** 80 max, billboard, warm steppe colours, drifting
- **Light breathing:** Perlin noise modulates directional light intensity (±8%)
- **Tile sway:** Each tile's first child rotates ±3° Y sinusoidally (unique phase/speed)

---

### WaterAnimator.cs (101 lines)
**Role:** Animates water tiles.

- **Colour lerp:** Deep blue ↔ light blue using `MaterialPropertyBlock`
- **Y bob:** ±0.07 units sinusoidal
- Speed: 1.4 Hz, each tile has random phase offset

---

### Editor/HackathonAssetSetup.cs
**Role:** One-click scene setup (menu: `Hackathon/Auto-Setup Assets`).

What it does:
1. Finds KayKit hex tile models by name variants
2. Creates prefabs in `Assets/Prefabs/` (Tile_Grass, Tile_Water, Tile_Mountain, YurtPrefab, WarriorUnit)
3. Each tile prefab gets: `Tile` component, `CapsuleCollider`, `meshRenderer` wired, `cubeCountText` (TMP), yurt/warrior prefab references
4. Creates "Game Controllers" GameObject with `HexGridManager` + `GameManager` + `CombatManager`
5. Creates "UI Canvas" with `UIManager`
6. Creates directional light, `EventSystem`, wires `CameraController` to main camera
7. Saves scene

### Editor/FBXImportSettings.cs
**Role:** `AssetPostprocessor` — disables camera/light import on all `.fbx` files.

---

## Data Flow Diagram

```
User Click
  │
  ▼
GameManager.HandleClick()
  │
  ├── InitialPlacement? → HandlePlacementClick() → Tile.AddCubes()
  │
  ├── Reinforce? → HandleReinforceClick() → Tile.AddCubes()
  │
  ├── Attack? → HandleAttackClick()
  │     ├── Select source tile (own, 2+ troops)
  │     └── Click enemy → BeginCombatSetup()
  │           └── UIManager.ShowCombatSetup()
  │                 └── User picks dice → OnCombatSetupConfirm()
  │                       └── CombatManager.ResolveCombat()
  │                             └── If conquered → PostConquest advance
  │                                   └── UIManager.ShowMigrationCount()
  │
  └── Fortify? → HandleFortifyClick()
        ├── Select source (own, 2+ troops)
        └── Select destination (connected own territory)
              └── UIManager.ShowMigrationCount()
                    └── OnMigrationCountConfirm() → animated troop transfer
```

---

## Missions (Win Condition)

Each player is assigned a secret mission at game start:

| Mission Type | Description | Win Check |
|---|---|---|
| `ControlTerritories` | Control 24 territories | Count owned tiles ≥ 24 |
| `DestroyPlayer` | Eliminate a specific clan | Target player has 0 tiles |
| `ControlTwoRegions` | Control 2 specific regions entirely | All passable tiles in both regions owned |

**A player cannot be assigned a "Destroy self" mission** — it falls back to ControlTerritories.

Mission progress is shown in the bottom-right HUD panel.

---

## Reinforcement Formula

```
reinforcements = max(3, ownedTiles / 3) + regionBonuses
```

Region bonuses (when a player controls ALL passable tiles in a region):
| Region | Bonus |
|---|---|
| Uly Zhuz | +5 |
| Zhetysu | +5 |
| Orta Zhuz | +4 |
| Kishi Zhuz | +4 |
| Dala | +3 |
| Syr Darya | +3 |

---

## Combat Rules (Risk-style)

1. Attacker picks 1–3 dice (must leave ≥1 warrior behind)
2. Defender picks 1–2 dice (up to their warrior count)
3. Forest bonus: attacker rolls +1 extra die, discards lowest
4. **Reverse pairing:** Attacker's highest ↔ defender's lowest
5. Defender wins ties
6. Losses applied immediately to warrior counts
7. If defender reaches 0: surviving attackers move in, post-conquest advance offered

---

## Where to Make Changes

| I want to... | Edit this file | Specifically... |
|---|---|---|
| Add a new game phase | `GameManager.cs` | Add to `GameState` enum, add handler in `HandleClick()`, add state transitions |
| Add a new terrain type | `Tile.cs` + `HexGridManager.cs` | Add to `TerrainType` enum, add prefab array + case in `GetPrefabForTerrain()` and `DetermineTerrain()` |
| Change combat rules | `CombatManager.cs` | Modify `ResolveCombat()` — keep it pure, return data via `CombatResult` |
| Add a tile property | `Tile.cs` | Add field + setter, read in `GameManager` rules, render in `UpdateVisuals()` |
| Add HUD element | `UIManager.cs` | Add `Build*` method, call from `BuildHUD()`, use `CreatePanel`/`CreateTMPLabel`/`CreateButton` factories |
| Add hex math (pathfinding, range) | `HexGridManager.cs` | Extend with BFS helpers next to `GetNeighbors()` |
| Add a new sound effect | `AudioManager.cs` | Add `SerializeField` clip + `Play*` method, add MP3 to `Assets/music/` |
| Add visual polish | `AmbientEnvironment.cs` or `WaterAnimator.cs` | Particle systems, light effects, per-tile animations |
| Change camera behaviour | `CameraController.cs` | Modify zoom range, tilt curve, pan speed, or `FitToGrid()` |
| Add a new mission type | `GameManager.cs` | Add to `MissionType` enum, add case in `AssignMissions()` and `MissionComplete()` |
| Add a new clan/player | `GameManager.cs` + `Tile.cs` | Increase `PlayerCount`, add to `ClanNames[]`, `PlayerTints[]`, `BorderClanColors[]` |

---

## Conventions & Rules

1. **Singletons:** All managers use `public static Instance` set in `Awake()`. Always null-check.
2. **Visual updates:** Go through `MaterialPropertyBlock` — never clone materials per tile.
3. **Tile colouring:** Tiles are tinted by **region/continent** (Risk board style). Warriors and borders show **owner** colour.
4. **Highlighting:** Always `ClearHighlights()` before re-highlighting. Always `ClearSelection()` on phase change.
5. **No UI prefabs:** Everything is built in `UIManager.Awake()`. Add new widgets there.
6. **Procedural assets:** Hex borders (LineRenderer), background plane — all built at runtime.
7. **Animations:** Warrior clips found by keyword search ("Attack", "Walk"). Use `Tile.PlayAttackAnimation()` / `PlayWalkAnimation()`.
8. **Coordinate system:** Axial `(q, r)` everywhere. Use `HexGridManager.GetNeighbors()` for adjacency.
9. **Colliders:** Tiles use `CapsuleCollider` (not MeshCollider) for reliable raycasting. Warriors/decorations have colliders stripped.
10. **Turn order:** Players who have been eliminated are skipped automatically.

---

## Prefab Wiring (SerializedFields)

### Tile prefab
| Field | Wired to |
|---|---|
| `meshRenderer` | First `Renderer` on the tile model |
| `yurtPrefab` | `Assets/Prefabs/YurtPrefab.prefab` |
| `warriorPrefab` | `Assets/Prefabs/WarriorUnit.prefab` |
| `cubeCountText` | Child `TextMeshPro` component (world-space, initially hidden) |

### HexGridManager
| Field | Wired to |
|---|---|
| `plainPrefabs[]` | `Tile_Grass.prefab` |
| `waterPrefabs[]` | `Tile_Water.prefab` |
| `mountainPrefabs[]` | `Tile_Mountain.prefab` |
| `treePrefabs[]` | KayKit tree decoration models |
| `rockPrefabs[]` | KayKit rock decoration models |
| `hillPrefabs[]` | KayKit hill decoration models |
| `mountainDecoPrefabs[]` | KayKit large mountain FBX models (outer ring) |
| `mapRadius` | 4 |
| `hexSize` | 1.0 |

---

## Running

Open in Unity 6, press Play in `SampleScene`. Everything bootstraps from `HexGridManager`. No build steps, no test suite.

If prefabs are missing, run `Hackathon > Auto-Setup Assets` from the Unity menu bar.
