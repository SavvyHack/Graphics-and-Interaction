# Current game design

Status: development specification, 5 October 2026. Read [decisions](DECISIONS.md) for what the user confirmed versus defaults proposed here. All new mechanics below are targets, not implemented features.

## Vision and retained identity
Project R.A.T. is a short single-player 2.5D puzzle-platformer about escaping three laboratory enclosures. One rat is controlled at a time; two waiting rats represent spare lives. Retain the outside-the-glass camera, readable blue-grey laboratory art, stylised non-graphic hazards and simple keyboard controls from the original GDD.

The user wants three levels inspired by Fireboy and Watergirl rather than the old GDD layouts, with portals adapting traversal for one player. No particular reference levels were supplied. Build original layouts using switches, spatial routing, gates and understandable cause/effect. Do not infer elemental immunity, two playable characters or copied geometry/art from the reference.

## What supersedes the original GDD
| Historical statement in README | Current target |
|---|---|
| One continuous enclosure with no level loading | Three distinct playable stages in one consistent laboratory setting; separate scenes are the proposed implementation |
| Level1/2/3 concept images dictate layouts | Retain as historical art references; use the new level briefs |
| Obstacles do not interact | Blocks, plates, gates and portals deliberately interact through explicit puzzle rules |
| Mandatory illustrated power-ups | Defer jetpack, double-jump, shield, speed boost, gravity pulse and slow time; none required for the first complete game |
| URP technology requirement | Uploaded implementation uses built-in rendering. Preserve it for now; confirm coursework constraint before any migration |
| Minimal HUD and outcome panels | Add home, level select, pause, settings, progress persistence, controls, credits and final ending |

## Core loop
Observe a small chamber, understand the gate or route, arrange a block or activate a persistent switch, use a portal to reach the next route, traverse the hazard, secure a checkpoint, then reach the exit. Levels introduce, practice and recombine mechanics. Required paths must be solvable by one active rat without simultaneous distant inputs.

## Lives and recovery — proposed campaign defaults
Start each level with three rats. Death consumes one life and respawns at the most recent safe checkpoint. On the third loss, show failure with Retry Level and Home; retry restores three rats and resets that level. Previously unlocked levels remain unlocked. A local Reset Puzzle action restores the current room's puzzle baseline without spending a life, and cannot resurrect lost lives. It exists to recover from awkward block positions.
Checkpoint progress is for the current attempt; Continue restarts the last unfinished/unlocked level from its beginning, not from a mid-puzzle physics snapshot. Label that behaviour in the UI. See [progress/reset specification](systems/progression-and-saving.md).

## Portals
Use fixed, authored two-way pairs as the initial proposal, with explicit interaction at the pad. Portals transport only the active rat, preserve the gameplay plane and clear velocity; crates stay on their tracks. A block or latched switch, not a portal alone, holds a distant gate open. Avoid free placement and momentum-flinging in the first version. See [portal contract](systems/portals.md).

## Campaign
| Level | Learning goal | Core puzzle | Approximate first-clear target |
|---|---|---|---|
| 1 — Transfer Training | Understand one portal pair and block-held gate | Place block, transfer, pass visible safe gate | 3–5 minutes |
| 2 — Relay Chambers | Understand persistent state across two routes | Latch bridge, return, hold laser off with a block | 5–7 minutes |
| 3 — Escape Circuit | Combine routing with one predictable timing challenge | Open access to second pair, suppress final laser, reach exit | 6–9 minutes |
These are tuning targets, not measured durations. Detailed sequences, reset rules and completion criteria live in [levels](levels/README.md).

## Complete-game requirements
Home: New Game, Continue when available, Level Select, Settings, Controls, Credits; desktop Quit only where meaningful. Pause: Resume, Settings, Controls, Reset Puzzle where supported, Restart Level and Home. Confirm destructive resets. Win: Next Level / Replay / Home; Level 3 instead shows a brief escape ending and credits/replay access. Loss: explicit retry/home choice.
Settings: Master, Music and SFX volume, mute, immediate preview and persistent preferences; accessible through home and pause. Use consistent keyboard/mouse focus and readable labels. Pause freezes hazards and movement, while menu input/audio continue appropriately.
HUD: three visible life icons, level/checkpoint, contextual interaction prompts, clear portal pair labels, attempt coins and wallet with distinct labels. Introduce controls inside safe gameplay, not only on a help page. Coins are optional cosmetic currency; no score, time limit or collectible quota gates completion.

## Polish priorities
Required: responsive movement; reliable respawn; safe portal landings; readable hazards; visible switch-to-gate connections; music/ambient loop and distinct event cues; short transitions; no dead-end menu flows; camera framing that reveals puzzle relationships; credits and asset attribution; a browser build playable from beginning to ending.
After those work: jump buffering, optional hints after repeated failures, reduced motion/flash options, cosmetic rat differences and optional best-time replay records. Optional objectives must not block exits or inflate the three-level scope.

## Out of scope for first completion

The following scope remains unchanged by the requested extensions below.
Multiplayer, AI companion rats, elemental character swaps, free-placement portals, portal render textures, crate teleportation, inventory, combat, online services and a broad power-up roster.

## Requested profile and presentation extensions
Implement [coins/cosmetics](systems/coins-and-cosmetics.md), [Settings](systems/settings-page.md), [Help/lore](systems/help-and-lore.md), [survivor victory](systems/victory-screen.md), [Statistics](systems/statistics.md) and selected [extra hazards](systems/extra-hazards-and-obstacles.md). Home includes Wardrobe, Help and Statistics; Controls is a shortcut to Help's Controls tab. These extend the earlier requirements and remain unimplemented targets.

New Game resets campaign progress only; lifetime coins, outfits, statistics and settings survive. Separately confirmed Erase All Data clears the profile/preferences. Extra levels can extend the ordered catalog; the initial campaign remains three levels. Preserve the prototype's rat/laboratory style. Follow [prototype retirement](systems/prototype-retirement.md) before removing superseded content. Help does not commit the deferred power-ups to implementation.
