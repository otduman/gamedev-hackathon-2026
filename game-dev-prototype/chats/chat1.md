# Game UI Design System

_Started 2026-04-26 01:15 UTC_

---

## User



## Assistant

I'll create wireframes exploring the Main Menu and Game HUD for "Rise of Nomads" — original designs inspired by your brief, not copies of any existing game's UI.

_[tool: questions_v2]_

## User

Questions answered:
- scope: All of the above
- variation_count: Decide for me
- fidelity: Mid-fi with texture suggestions
- exploration_focus: Layout structure (where things go), Information density (minimal vs rich), Phase/turn UX patterns, Ornamentation style (which culture leads)
- menu_metaphor: Decide for me
- hud_density: Balanced — key stats always visible
- hotseat_handling: Decide for me
- phase_indicator: Decide for me
- combat_overlay: Decide for me
- ornament_intensity: 30
- tweaks: Decide for me, Show/hide tooltip

## Assistant

I'll build a wireframe explorer covering all six screens with 3 variations each, mid-fi sketchy with faction color accents at ~30% ornament intensity. Let me set up the project.

_[tool: update_todos]_

_[tool: copy_starter_component]_

_[tool: copy_starter_component]_

_[tool: write_file]_

Now let me build the wireframe components in modular files.

_[tool: write_file]_

_[tool: write_file]_

_[tool: snip]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: snip]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: snip]_

_[tool: done]_

_[tool: fork_verifier_agent]_

_[tool: snip]_

Six screens × three variations are laid out on a pannable canvas — Main Menu, In-Game HUD, Combat Dice Overlay, Tile Tooltip, Game-Over, and Hotseat Leader Select. Faction palette is hinted via tinted swatches (Kazakh green / Slavic blue / Timurid crimson / Ming gold), ornamentation kept at ~30%, all imagery as striped placeholders.

Tweaks panel toggles ornament strips, tooltip visibility, active-faction tint, and current phase. Drag to reorder, click any artboard to focus.

Next steps once you've picked favorites: pick a winner per screen and I'll push to mid-fi (real textures, leather/felt/brass treatments, type system).

## User



## Assistant

_[tool: update_todos]_

I'll update the HUD and Combat wireframes to reflect the classic Risk rules — 4 phases (Setup/Reinforce/Attack/Fortify), secret missions, territory+warrior counts in the empire roster, and simplified tooltips.

_[tool: write_file]_

_[tool: write_file]_

_[tool: snip]_

_[tool: write_file]_

_[tool: done]_

_[tool: fork_verifier_agent]_

Updated wireframes are live. Key changes in v2:

- **HUD**: 4 phases (Setup → Reinforce → Attack → Fortify), secret mission parchment element, empire roster now shows hex-icon territory count + sword-icon warrior count per faction
- **Combat**: territory names on both attacker/defender ("Samarqand → Kashgar"), simplified result text with "defender wins ties" callout
- **Tooltips**: stripped to city name + owner + warrior count only

Menu, Game Over, and Hotseat Select carry over from v1. Pick your favorites per screen and I can push toward mid-fi with real texture treatments.

## User

Verifier agent check completed

## Assistant

All good — verifier confirmed everything rendered correctly. Let me know which variations you'd like to take forward!

## User

01 - B
02 - A
03 - A
04 - A
05 - B
06 - A

## Assistant

_[tool: update_todos]_

Great choices. I'll create a refined mid-fi version with just these six winners — richer textures, better typography, and more fleshed-out details.

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: snip]_

_[tool: done]_

_[tool: fork_verifier_agent]_

_[tool: snip]_

Mid-fi is live with your 6 picks rendered in the dark wood / leather / felt / brass aesthetic:

1. **Menu B** — asymmetric title left, felt-textured menu rows right, 4-leader strip bottom
2. **HUD A** — carved wood top bar (leader + 4 phases + secret mission parchment + end-phase boss), leather empire roster right with hex/sword counts
3. **Combat A** — two tilted felt mats (red attacker / stone defender) with territory names + result banner
4. **Tooltip A** — compact felt panel: city name, owner chip, warrior count
5. **Game Over B** — 4-up scoreboard, winner crowned with gold glow
6. **Hotseat A** — side-by-side 2×2 picker grids, VS center

Tweaks toggle ornaments, faction tint, tooltip, and active phase. All portrait/landscape imagery is placeholders ready for real assets. Let me know what to refine or if you'd like to push any screen further!

## User

I love it, let's proceed to the next high fidelity prototype / final product

## Assistant

_[tool: questions_v2]_

## User

Questions answered:
- scope_hifi: All interactive
- assets: I have some, will upload a few
- bg_art: Use a subtle dark texture (no landscape)
- typography: Yes, keep it
- animations: All of the above
- sound_cues: Yes, annotate SFX cues
- unity_spec: Critical — annotate panel hierarchy + component names
- resolution: 1920×1080 (standard)
- tweaks_hifi: Decide for me

## Assistant

_[tool: update_todos]_

Big step — full interactive hi-fi prototype at 1920×1080. Let me plan and build this systematically.

_[tool: write_file]_

_[tool: copy_files]_

Beautiful portraits! Let me copy these into the project and continue building the hi-fi prototype with real art.

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

## Assistant

_[tool: write_file]_

_[tool: write_file]_

_[tool: write_file]_

_[tool: snip]_

_[tool: write_file]_

_[tool: done]_

_[tool: fork_verifier_agent]_

_[tool: snip]_

Hi-fi prototype is live! Here's what you've got:

**All 6 screens interactive at 1920×1080**, auto-scaled to viewport:

1. **Main Menu** — shimmer gold title, hover-reactive felt buttons, real leader portraits in the bottom strip. Click START GAME to proceed.
2. **Leader Select** — click portraits to assign P1/P2; taken leaders greyed out. Click BEGIN CAMPAIGN.
3. **In-Game HUD** — clickable phase pills (Setup→Reinforce→Attack→Fortify), hover hex tiles for tooltip, click enemy hexes in Attack phase to trigger combat, End Phase button cycles through.
4. **Combat Overlay** — animated dice roll with random results, attacker/defender mats slide in, Attack Again re-rolls.
5. **Game Over** — 4-up scoreboard with winner gold glow + shimmer.
6. **Hotseat Select** — same as #2, navigable from menu.

**Extras:**
- Press **A** anywhere to toggle Unity UIManager spec annotations (blue) and SFX cue annotations (red)
- Ambient ember particles throughout
- Gold pulse animations on active elements
- Navigation strip at bottom-right lets you jump to any screen

**Next steps:**
- Upload more assets (background paintings, icons) to replace remaining placeholders
- Add Tweaks panel for faction switching / phase cycling without clicking
- Refine specific screens based on your feedback

## User

i don't like the HUD. Can you update it. maybe bring the secret mission down

## Assistant

_[tool: view_image]_

_[tool: fork_verifier_agent]_

Let me wait for the screenshot to see the current state, then I'll redesign the HUD with the secret mission moved down and a cleaner layout.

## User

Verifier agent check completed

## Assistant

_[tool: read_file]_

Clear feedback. I'll redesign the HUD: secret mission moves to the right panel, top bar gets breathing room, hex grid fills the viewport better.

_[tool: write_file]_

_[tool: done]_

_[tool: fork_verifier_agent]_

_[tool: snip]_

HUD redesigned:

- **Top bar** now has just leader portrait + phase pills + contextual hint + end-phase button — much more breathing room
- **Secret mission** moved to the top of the right panel as a prominent parchment card
- **Hex grid** expanded to 10×8 staggered layout filling the map viewport
- **Right panel** now flows: mission → roster → turn counter

Waiting on verifier to confirm it looks good. Let me know if you want further tweaks!

## User

Verifier agent check completed

## Assistant

_[tool: read_file]_

_[tool: str_replace_edit]_

Now fix the mission text wrapping:

_[tool: read_file]_

_[tool: grep]_

_[tool: str_replace_edit]_

_[tool: snip]_

_[tool: done]_

_[tool: fork_verifier_agent]_

Fixed both issues — hex grid now has 14 rows (up from 8) filling the viewport, and mission text uses smaller font with better line wrapping. Waiting on verifier confirmation.

## User

Verifier agent check completed

## Assistant

