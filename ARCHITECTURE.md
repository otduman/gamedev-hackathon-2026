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
| Audio | **Procedural** — no audio files, all SFX synthesised at runtime |
| Art | KayKit hex tiles + Meshy AI warrior FBX (rigged, 4 animations) |
| Scene | Single scene: `Assets/Scenes/SampleScene.unity` |
| Win | First clan to **15 Prestige** or wipe opponent off the map |
| Players | 2-player local hot-seat (Argyn vs Naiman) |

---

## Directory Map

```
gamedev-hackathon-2026/
├── Assets/
│   ├── Scripts/               ← ALL gameplay code lives here (9 .cs files)
│   │   ├── GameManager.cs         (858 lines)  — Turn state machine, input, rules
│   │   ├── HexGridManager.cs      (408 lines)  — Procedural hex grid, terrain, decorations
│   │   ├── Tile.cs                 (402 lines)  — Per-tile state & visuals
│   │   ├── UIManager.cs            (933 lines)  — Builds entire HUD programmatically
│   │   ├── CombatManager.cs        (105 lines)  — Pure dice combat resolution
│   │   ├── CameraController.cs     (168 lines)  — Civ-style zoom/pan camera
│   │   ├── AudioManager.cs         (117 lines)  — Procedural SFX synthesis
│   │   ├── AmbientEnvironment.cs   (142 lines)  — Dust particles, light breathing, tile sway
│   │   └── WaterAnimator.cs        (102 lines)  — Water tile colour lerp + Y bob
│   ├── Editor/                ← Editor-only tooling (not included in builds)
│   │   ├── HackathonAssetSetup.cs  — Menu item "Hackathon/Auto-Setup Assets"
│   │   └── FBXImportSettings.cs    — Strips cameras/lights from imported FBX
│   ├── Prefabs/               ← 5 prefabs (created by HackathonAssetSetup)
│   │   ├── Tile_Grass.prefab      — Used for Plain + Pasture terrain
│   │   ├── Tile_Water.prefab
│   │   ├── Tile_Mountain.prefab
│   │   ├── WarriorUnit.prefab     — Meshy AI biped with Animator
│   │   └── YurtPrefab.prefab      — Nomadic base building
│   ├── KayKit/                ← 3rd-party hex tile + decoration FBX models
│   │   ├── fbx(unity)/tiles/      — hex_grass, hex_water, hex_mountain
│   │   ├── fbx(unity)/buildings/  — yurt model
│   │   ├── fbx(unity)/decoration/ — trees, hills
│   │   └── Meshy_AI_biped_*.fbx   — 4 warrior animations (Alert/Attack/Running/Walking)
│   ├── Scenes/
│   │   └── SampleScene.unity      — The only scene, everything bootstraps from here
│   ├── background-image.jpg       — Loaded at runtime for the ground plane
│   └── InputSystem_Actions.inputactions
├── .claude/agents/            ← Specialised Claude agent configs
├── CLAUDE.md                  ← Existing Claude Code context file
├── ProjectSettings/           ← Unity project settings (URP, input, quality)
└── Packages/                  ← Unity Package Manager manifests
```

---

## Bootstrap Flow

```
Scene loads
  └─► HexGridManager.Awake() — sets Instance singleton
  └─► HexGridManager.Start()
        └─► GenerateHexagonalMap()
              ├── Creates all Tile GameObjects (axial hex grid, radius 3)
              ├── PlaceDecorations() — trees/rocks + marks forest tiles
              ├── AddHexBorders() — LineRenderer outlines
              ├── CreateBackgroundPlane() — loads background-image.jpg
              └── OnGridReady?.Invoke()
                    ├── GameManager.InitializeGame()
                    │     ├── Places 2 starting clusters (3 tiles each, 1 yurt per clan)
                    │     ├── RecalculatePrestige()
                    │     └── StartReinforce() — begins first turn
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
  Reinforce → Action → CombatSetup → Action → MigrationCheck → MigrationCount → (next player's Reinforce)
                                                                                   └──── GameOver
```

| State | What happens | User input |
|---|---|---|
| `Reinforce` | Player gains `max(3, ownedTiles/3) + yurtCount` warriors | Click yurt tile(s) to place warriors |
| `Action` | Player selects own tile (2+ warriors) and attacks adjacent enemy/neutral | Click source → click target; repeat; End Turn when done |
| `CombatSetup` | Modal panel — both sides pick dice count | Click 1/2/3 buttons → Resolve or Cancel |
| `MigrationCheck` | One move per turn (BFS through own territory) or yurt relocation | Click source → click destination |
| `MigrationCount` | Modal panel — pick how many warriors to move | +/- buttons → Confirm or Cancel |
| `GameOver` | Victory screen shown, "Play Again" button reloads scene | — |

**Turn flow:** End Turn during Action → starts Migration. End Turn during Migration → switches player, starts Reinforce.

---

## File-by-File Reference

### GameManager.cs (858 lines)
**Role:** Central controller — turn/phase state machine, click/hover handling, selection/highlight bookkeeping, win conditions.

**Key fields:**
- `CurrentState` (GameState enum) — drives all input routing
- `CurrentPlayerId` (0 = Argyn, 1 = Naiman)
- `PlayerPrestige[2]` — recalculated after every action
- `_selectedTile`, `_hoveredTile` — current interaction state
- `_highlightedTiles` — list of tiles with active highlights (always `ClearHighlights()` before re-highlighting)
- `_reachableTiles` — BFS result for migration destinations

**Key methods:**
| Method | Purpose |
|---|---|
| `InitializeGame()` | Places starting clusters, begins first turn |
| `HandleClick()` | Routes clicks by `CurrentState` to appropriate handler |
| `HandleReinforceClick()` | Places warriors on yurt tiles |
| `HandleActionClick()` | Selects source, validates target, initiates combat/expansion |
| `HandleMigrationClick()` | Handles warrior movement and yurt relocation |
| `BeginCombatSetup()` | Opens dice selection modal |
| `OnCombatSetupConfirm(atk, def)` | Called by UIManager button — resolves combat |
| `OnEndTurnButtonClicked()` | Action→Migration or Migration→next player |
| `RecalculatePrestige()` | +1 per non-contested owned tile, +2 per yurt |
| `CheckWinCondition()` | ≥15 prestige OR opponent has no tiles |
| `GetMigrationReachable()` | BFS through own territory — no MP cap (just connectivity) |
| `ClearSelection()` / `ClearHighlights()` | Reset interaction state |

**Highlight colours:**
- Red `(1.0, 0.2, 0.2)` — attack targets
- Green `(0.2, 0.9, 0.4)` — migration targets
- Gold `(1.0, 0.8, 0.0)` — yurt move targets / reinforce targets
- Yellow `(0.9, 0.85, 0.1)` — neutral expansion targets

---

### HexGridManager.cs (408 lines)
**Role:** Procedural hex grid generation, terrain assignment, decoration placement.

**Key concepts:**
- **Axial coordinates** `(q, r)` stored as `Vector2Int`
- **Pointy-top hex layout** with 6% spread between tiles
- **Grid radius:** 3 (37 total hexes)
- **Terrain generation:** Inner rings (dist ≤ 2) always passable (Plain/Pasture). Outer ring may have Mountain/Water.
- **Forest:** Post-pass tag — Plain/Pasture tiles with tree decorations become forests (`HasForest = true`, `MoveCost = 2`, +1 attack die)
- `Grid` dictionary: `Dictionary<Vector2Int, Tile>` — the single source of truth for all tiles

**Key methods:**
| Method | Purpose |
|---|---|
| `GenerateHexagonalMap()` | Main generation loop |
| `CalculateHexPosition()` | Axial coords → world position |
| `DetermineTerrain(q, r)` | Random terrain based on distance from origin |
| `GetPrefabForTerrain()` | Maps TerrainType → prefab array |
| `GetNeighbors(Vector2Int)` | Returns list of adjacent Tile objects (6 directions) |
| `PlaceDecorations()` | Spawns trees/rocks, marks forest tiles |
| `CreateFallbackTile()` | Fallback cylinder if no prefab assigned |

**Neighbor directions (axial):**
```
(+1,0) (+1,-1) (0,-1) (-1,0) (-1,+1) (0,+1)
```

---

### Tile.cs (402 lines)
**Role:** Per-tile state container and visual representation.

**Enums defined here:**
```csharp
enum TerrainType { Plain, Mountain, Water, Pasture }
```

**Key properties:**
| Property | Type | Notes |
|---|---|---|
| `HexCoordinates` | `Vector2Int` | Axial (q, r) — set once in `Initialize()` |
| `Terrain` | `TerrainType` | Set once in `Initialize()` |
| `OwnerPlayerId` | `int` | -1 = neutral, 0 = Argyn, 1 = Naiman |
| `CubeCount` | `int` | Number of warriors on this tile |
| `HasYurt` | `bool` | Nomadic base — +2 prestige, warriors spawn here |
| `HasForest` | `bool` | +1 attack die, 2 movement cost |
| `IsContested` | `bool` | Newly conquered — no prestige until owner's next Reinforce |
| `MoveCost` | `int` | Computed: `HasForest ? 2 : 1` |

**Visual system:**
- Tinting via `MaterialPropertyBlock` (no per-tile material clones)
- Owner colours: Argyn `(0.25, 0.55, 1.0)`, Naiman `(1.0, 0.28, 0.28)`
- Warrior formations: predefined `Vector3[][]` arrays for 1–5 positions
- Max 5 warriors shown visually; count label for overflow (`TextMeshPro`)
- Yurt: instantiated from prefab, auto-scaled via `AutoScaleYurt` coroutine
- Decorations: tracked via `_decoRenderers` list, opacity changes on hover/occupation
- Conquest pulse: coroutine flashes tile white over 0.55s

**Animation indices:** `0=Idle, 1=Attack, 2=Walking`
- `PlayAttackAnimation()` / `PlayWalkAnimation()` — temporary clip, returns to idle

**Important:** Always call `UpdateVisuals()` after any state change on a tile.

---

### CombatManager.cs (105 lines)
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
- Defender: 1–2 dice (up to warrior count)
- **Forest bonus:** Roll 1 extra die, discard lowest
- **Reverse pairing:** Attacker's highest vs defender's LOWEST (prevents a single high roll from sweeping both pairs)
- **Defender wins ties**
- On conquest: surviving attackers move into tile, tile becomes Contested

---

### UIManager.cs (933 lines)
**Role:** Builds the entire HUD programmatically in `Awake()`. No UI prefabs exist.

**HUD components built in `BuildHUD()`:**
1. **Top bar** — player turn label, phase text, instruction message
2. **Prestige panel** — bottom-right, shows both clan scores + "First to 15 wins"
3. **End Turn button** — bottom-center, wired to `GameManager.OnEndTurnButtonClicked()`
4. **Combat result panel** — centre overlay, auto-hides after 2s
5. **Dice combat panel** — centre, visual dice display, auto-hides after 3s
6. **Game Over panel** — centre, with "Play Again" button (reloads scene)
7. **Tooltip panel** — bottom-left, shows hovered tile info
8. **Combat setup panel** — modal, attacker/defender dice selection (1/2/3 buttons)
9. **Migration count panel** — modal, +/- warriors to move

**Factory methods:**
- `CreatePanel()` — anchored panel with `Image` background
- `CreateTMPLabel()` — positioned `TextMeshProUGUI` element
- `CreateButton()` — button with Image + click handler + label

**Key public API:**
| Method | Called by |
|---|---|
| `UpdateUI()` | GameManager — refreshes all HUD text from current state |
| `ShowMessage(string)` | GameManager — sets instruction text |
| `ShowCombatResult(CombatResult, atkName, defName)` | GameManager — populates dice panel |
| `ShowCombatSetup(maxAtk, maxDef, atkClan, defClan)` | GameManager — opens dice selection modal |
| `HideCombatSetup()` | GameManager — closes dice selection |
| `ShowMigrationCount(max, default)` | GameManager — opens warrior count picker |
| `HideMigrationCount()` | GameManager — closes warrior count picker |
| `ShowTooltip(Tile)` / `HideTooltip()` | GameManager — hover info |
| `ShowGameOver(winningPlayerId)` | GameManager — victory overlay |

**To add a new HUD element:** Create a `Build*` method, call it from `BuildHUD()`.

---

### CameraController.cs (168 lines)
**Role:** Civ-style camera with zoom-dependent tilt.

- **Tilt range:** 40° (zoomed in) → 58° (zoomed out)
- **Controls:** WASD / Arrow keys for pan, scroll wheel for zoom
- **Auto-fit:** Computes zoom to fit all tiles on `OnGridReady` and on aspect ratio changes
- **Start zoom:** 55% of max (close-up)

---

### AudioManager.cs (117 lines)
**Role:** Synthesises 4 SFX clips at runtime using `AudioClip.Create()`.

| Clip | Frequency | Duration | Waveform |
|---|---|---|---|
| Attack | 200 Hz | 0.15s | Decaying sine |
| Conquer | 330→440→550 Hz | 0.6s | Three ascending tones |
| Turn Start | 523 Hz (C5) | 0.4s | Bell with exponential decay |
| Migration | 300→150 Hz | 0.25s | Frequency-glide whoosh |

Master volume: 0.22

---

### AmbientEnvironment.cs (142 lines)
**Role:** Atmospheric effects.

- **Dust particles:** 80 max, billboard, warm steppe colours, drifting
- **Light breathing:** Perlin noise modulates directional light intensity (±8%)
- **Tile sway:** Each tile's first child rotates ±3° Y sinusoidally (unique phase/speed)

---

### WaterAnimator.cs (102 lines)
**Role:** Animates water tiles.

- **Colour lerp:** Deep blue ↔ light blue using `MaterialPropertyBlock`
- **Y bob:** ±0.07 units sinusoidal
- Speed: 1.4 Hz, each tile has random phase offset

---

### Editor/HackathonAssetSetup.cs (281 lines)
**Role:** One-click scene setup (menu: `Hackathon/Auto-Setup Assets`).

What it does:
1. Finds KayKit hex tile models by name variants
2. Creates 5 prefabs in `Assets/Prefabs/` (Tile_Grass, Tile_Water, Tile_Mountain, YurtPrefab, WarriorUnit)
3. Each tile prefab gets: `Tile` component, `CapsuleCollider`, `meshRenderer` wired, `cubeCountText` (TMP), yurt/warrior prefab references
4. Creates "Game Controllers" GameObject with `HexGridManager` + `GameManager` + `CombatManager`
5. Creates "UI Canvas" with `UIManager`
6. Creates directional light, `EventSystem`, wires `CameraController` to main camera
7. Saves scene

### Editor/FBXImportSettings.cs (18 lines)
**Role:** `AssetPostprocessor` — disables camera/light import on all `.fbx` files.

---

## Data Flow Diagram

```
User Click
  │
  ▼
GameManager.HandleClick()
  │
  ├── Reinforce? → HandleReinforceClick() → Tile.AddCubes()
  │
  ├── Action? → HandleActionClick()
  │     ├── Select source tile (own, 2+ warriors)
  │     ├── Click neutral → ExpandIntoNeutral()
  │     └── Click enemy → BeginCombatSetup()
  │           └── UIManager.ShowCombatSetup()
  │                 └── User picks dice → OnCombatSetupConfirm()
  │                       └── CombatManager.ResolveCombat()
  │                             └── Returns CombatResult
  │                                   └── UIManager.ShowCombatResult()
  │
  └── Migration? → HandleMigrationClick()
        ├── Yurt move → ExecuteYurtMove()
        └── Warrior move → BeginMigrationCount()
              └── UIManager.ShowMigrationCount()
                    └── OnMigrationCountConfirm()
```

---

## Prestige Calculation

```
For each player:
  prestige = 0
  for each tile:
    if tile.OwnerPlayerId == player AND NOT tile.IsContested:
      prestige += 1
      if tile.HasYurt: prestige += 2
```

Contested tiles: Newly conquered tiles don't count until the owner survives their next Reinforce phase (then `SetContested(false)` is called).

---

## Reinforcement Formula

```
reinforcements = max(3, ownedNonContestedTiles / 3) + yurtCount
```
- Each yurt grants +1 warrior per round
- Conquering enemy yurt gives you their bonus

---

## Combat Rules (Risk-style)

1. Attacker picks 1–3 dice (must leave ≥1 warrior behind)
2. Defender picks 1–2 dice (up to their warrior count)
3. Forest bonus: attacker rolls +1 extra die, discards lowest
4. **Reverse pairing:** Attacker's highest ↔ defender's lowest
5. Defender wins ties
6. Losses applied immediately to warrior counts
7. If defender reaches 0: surviving attackers move in, tile marked Contested

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
| Add a new sound effect | `AudioManager.cs` | Add `Generate*` method, build clip in `Awake()`, add `Play*` method |
| Add visual polish | `AmbientEnvironment.cs` or `WaterAnimator.cs` | Particle systems, light effects, per-tile animations |
| Change camera behaviour | `CameraController.cs` | Modify zoom range, tilt curve, pan speed, or `FitToGrid()` |

---

## Conventions & Rules

1. **Singletons:** All managers use `public static Instance` set in `Awake()`. Always null-check.
2. **Visual updates:** Go through `MaterialPropertyBlock` — never clone materials per tile.
3. **Highlighting:** Always `ClearHighlights()` before re-highlighting. Always `ClearSelection()` on phase change.
4. **No UI prefabs:** Everything is built in `UIManager.Awake()`. Add new widgets there.
5. **Procedural assets:** SFX, hex borders (LineRenderer), background plane — all built at runtime.
6. **Animations:** Warrior Mecanim clips: `0=Idle, 1=Attack, 2=Walking`. Use `Tile.PlayAttackAnimation()` / `PlayWalkAnimation()`.
7. **Coordinate system:** Axial `(q, r)` everywhere. Use `HexGridManager.GetNeighbors()` for adjacency.
8. **Colliders:** Tiles use `CapsuleCollider` (not MeshCollider) for reliable raycasting. Warriors/decorations have colliders stripped.

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
| `pasturePrefabs[]` | `Tile_Grass.prefab` (same as plain) |
| `waterPrefabs[]` | `Tile_Water.prefab` |
| `mountainPrefabs[]` | `Tile_Mountain.prefab` |
| `treePrefabs[]` | KayKit tree decoration model |
| `rockPrefabs[]` | KayKit hills decoration model |
| `mapRadius` | 3 |
| `hexSize` | 1.0 |

---

## Running

Open in Unity 6, press Play in `SampleScene`. Everything bootstraps from `HexGridManager`. No build steps, no test suite.

If prefabs are missing, run `Hackathon > Auto-Setup Assets` from the Unity menu bar.
