# ⚔️ Rise of Nomads

A turn-based hex-grid strategy game set in the medieval Kazakh steppe, built with **Unity 6**. Four nomadic clans battle for control of the Great Steppe — each carrying a secret mission that only they know. Inspired by the board game *Risk*, reimagined through the rich history of the 15th–17th century Kazakh Khanate.

> 🏆 Built for the **Game Dev Hackathon 2026** — Theme: *Medieval*

[![Gameplay Video](https://img.youtube.com/vi/dAVAOejJ1xc/maxresdefault.jpg)](https://youtu.be/dAVAOejJ1xc)

▶️ **[Watch the Gameplay Video](https://youtu.be/dAVAOejJ1xc)**

---

## 🎮 About the Game

**Rise of Nomads** is a 4-player local hot-seat strategy game where each player commands a medieval Kazakh clan competing for dominance across a procedurally generated hex-grid map. Every game is different — the terrain is randomized, starting positions are shuffled, and each player receives a unique secret mission.

### The Four Clans

| Clan | Colour | Leader |
|------|--------|--------|
| **Argyn** | 🟢 Green | Kazakh warlord |
| **Naiman** | 🔴 Red | Timurid commander |
| **Kerey** | 🔵 Blue | Rus' warrior |
| **Kongrat** | 🟡 Gold | Ming general |

### Secret Missions

Each player is assigned one hidden objective at the start. The first to complete theirs wins:

- **Control Territories** — Conquer and hold 24 territories
- **Destroy a Clan** — Completely eliminate a specific rival from the map
- **Control Two Regions** — Dominate all tiles in two designated regions

You never know what your opponents are truly after.

---

## 🗺️ How to Play

### Turn Structure

Each turn has three phases:

**1. Reinforce** — Receive bonus troops based on territories held and region control. Place them on any territory you own.

**2. Attack** — Select a territory with 2+ troops and attack any adjacent enemy hex. You can attack as many times as you want per turn.

**3. Fortify** — Move troops once through a connected chain of your own territories.

### Combat System

Combat uses a Risk-style dice system with a unique twist:

- Attacker rolls up to **3 dice** (must leave at least 1 troop behind)
- Defender rolls up to **2 dice**
- Dice are compared in **reverse order** — attacker's highest vs defender's lowest
- **Defender wins ties**
- **Forest bonus** — attacking from a forest tile grants 1 extra die (roll 4, keep best 3)

### Regions & Bonuses

The steppe is divided into 6 historical regions. Controlling every tile in a region grants bonus reinforcements:

| Region | Bonus |
|--------|-------|
| Uly Zhuz | +5 |
| Zhetysu | +5 |
| Orta Zhuz | +4 |
| Kishi Zhuz | +4 |
| Dala | +3 |
| Syr Darya | +3 |

### Map Features

- **Plains** — Standard terrain, the backbone of the steppe
- **Forests** — Grant +1 attack die when fighting from a forested tile
- **Mountains** — Impassable natural borders along the outer ring
- **Water** — Impassable rivers and lakes

---

## 🎹 Controls

| Action | Input |
|--------|-------|
| Pan camera | `W` `A` `S` `D` or Arrow Keys |
| Zoom | Scroll Wheel |
| Select / Place troops | Left Click |
| Attack | Click your tile → Click adjacent enemy |
| Fortify | Click source → Click connected friendly tile |
| End Attack | `END ATTACK` button |
| End Turn | `END TURN` button |
| Dice selection | Click 1 / 2 / 3 → `RESOLVE` |
| Troop advance | `+` / `-` → `CONFIRM` |
| Tile info | Hover over any tile |

---

## 🏗️ Technical Overview

### Tech Stack

| Component | Technology |
|-----------|------------|
| Engine | Unity 6 (URP 17.4.0) |
| Language | C# |
| Rendering | Universal Render Pipeline (Linear) |
| UI | TextMesh Pro + uGUI — 100% built in code, zero prefabs |
| Input | Unity New Input System |
| Audio | MP3-based SFX + Dombyra background music |
| 3D Assets | KayKit hex tiles + Meshy AI warrior models |

### Architecture

The game runs on a singleton-based manager architecture with 9 core scripts:

```
Assets/Scripts/
├── GameManager.cs         — Turn state machine, input routing, missions, win conditions
├── HexGridManager.cs      — Procedural hex grid generation, terrain, territory naming
├── Tile.cs                — Per-tile state, visuals, warrior management
├── UIManager.cs           — Entire HUD built programmatically (no UI prefabs)
├── CombatManager.cs       — Pure dice combat resolution
├── CameraController.cs    — Civ-style camera with zoom-dependent tilt
├── AudioManager.cs        — SFX playback
├── AmbientEnvironment.cs  — Dust particles, light breathing, tile sway
└── WaterAnimator.cs       — Water tile colour animation + bobbing
```

### Key Design Decisions

- **Procedural hex grid** — Axial coordinate system with pointy-top hexes. 61 total hexes (radius 4), terrain assigned by distance from center
- **No UI prefabs** — The entire HUD (menus, combat panels, tooltips, game over screen) is built programmatically in `UIManager.Awake()` using factory methods
- **MaterialPropertyBlock rendering** — Tile tinting uses `MaterialPropertyBlock` to avoid per-tile material clones, keeping draw calls low
- **Single-scene bootstrap** — `HexGridManager.Start()` generates the grid and fires `OnGridReady`, which `GameManager` and `CameraController` subscribe to. Everything self-initialises from there

For the full technical reference, see [ARCHITECTURE.md](ARCHITECTURE.md).

---

## 🚀 Getting Started

### Prerequisites

- **Unity 6** (2023 LTS or later) with Universal Render Pipeline
- WebGL Build Support module (for browser builds)

### Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/gamedev-hackathon-2026.git
   ```
2. Open the project in Unity 6
3. Open `Assets/Scenes/SampleScene.unity`
4. Press **Play**

If prefabs are missing, run **Hackathon > Auto-Setup Assets** from the Unity menu bar.

### Building for WebGL

1. **File → Build Profiles** → Select **Web/WebGL**
2. In **Player Settings → Publishing Settings**: set Compression to **Gzip**, enable **Decompression Fallback**
3. Click **Build** and choose an output folder
4. Test locally:
   ```bash
   cd path/to/build
   python3 -m http.server 8080
   ```
5. Open `http://localhost:8080` in your browser

---

## 📁 Project Structure

```
gamedev-hackathon-2026/
├── Assets/
│   ├── Scripts/            ← All gameplay code (9 files)
│   ├── Editor/             ← Editor-only tooling
│   ├── Prefabs/            ← Tile, warrior, and yurt prefabs
│   ├── music/              ← SFX and background music (MP3)
│   ├── KayKit/             ← 3rd-party hex tile models
│   ├── portaits/           ← Clan leader portrait images
│   ├── Scenes/             ← Main game scene
│   └── background-image.jpg
├── ARCHITECTURE.md         ← Full technical reference
├── CLAUDE.md               ← AI assistant context file
└── README.md               ← This file
```

---

## 🎨 Theme Interpretation

We interpreted the **Medieval** theme through the history of the Kazakh Khanate — a medieval Central Asian empire where nomadic clans fought for control of the Great Steppe. While most medieval games focus on European castles and knights, we chose to explore a lesser-known but equally rich chapter of medieval history.

Rise of Nomads places four Kazakh clans on a hex-grid battlefield where they compete for territorial dominance through dice-based warfare, strategic troop placement, and secret missions. The game captures the essence of the medieval era: land is power, alliances are fragile, and every ruler has a hidden agenda.

From the yurt settlements to the warrior units to the steppe terrain itself, every element is rooted in the medieval nomadic world of the 15th–17th century Kazakh Khanate.

---

## 🛠️ Tools & Credits

| Tool | Usage |
|------|-------|
| [Unity 6](https://unity.com/) | Game engine |
| [KayKit Assets](https://kaylousberg.itch.io/) | Hex tile and decoration 3D models |
| [Meshy AI](https://www.meshy.ai/) | AI-generated warrior character model and animations |
| [Claude Code](https://claude.ai/) | AI pair-programming assistant |
| [TextMesh Pro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest) | UI text rendering |

---

## 📄 License

This project was built for the Game Dev Hackathon 2026. All game code is original. Third-party assets (KayKit, Meshy AI) are used under their respective licenses.
