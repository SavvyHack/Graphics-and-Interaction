# Extra hazards and obstacles

## Implementation update - 6 October 2026
Saved stages contain timed gates, scanner sweeps, shuttles, wheels, magnetic barriers, shield lasers and fire traps. Stages 1-2 retain existing hazard shaders, and Slow Time affects TrialMovingPlatform/TrialWheel/CampaignHazardCycle without slowing the player. See fire-effect-handoff.md for the pending assessed-particle integration. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Earlier acceptance specification
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

Status: target specification. Additional obstacle vocabulary requested by the user; author only what serves a level's solo solution. These are chosen defaults, not evidence that these components are present. Read [existing puzzles](puzzles-and-hazards.md) before extending them.

## Style and fairness
Match RatEnclosure's side-on 2.5D laboratory: simple metal frames, glass, pipes, blue-grey surfaces and restrained hazard accent lights. Reuse built-in shaders/material families and existing player proportions. No fantasy spikes, gore, photorealistic replacement art or rendering migration. Danger must have a clear physical source and visible collision bounds; communicate with shapes/motion and warning lights plus optional sound, never colour alone.

Every timed obstacle needs a visible safe waiting spot, a warning of at least 0.75 seconds and a measured crossing window at normal walking speed plus at least 0.5 seconds margin. Listed timings are starting values, adjustable only after measured traversal checks. No randomized schedules in required puzzles; no untelegraphed activation under a portal landing or checkpoint. Hazard contact routes through the one life owner. Pause freezes phases; death/Reset Puzzle restore a known safe phase for the current room, without undoing profile coins. Avoid overlapping multiple new timed hazards before each is taught separately.

## Authoring catalog
| Obstacle | Purpose and initial behaviour | Recovery and implementation boundary |
|---|---|---|
| Steam vent | Floor pipe alternates 3s safe, 1s hiss/warning, 1.5s lethal jet | Jet particles and lethal volume match; reset to safe. Reuse TimedHazard if it supports an explicit warning phase; otherwise extend it compatibly |
| Electrical floor strip | Laboratory contacts alternate 3s safe, 1s pulsing warning, 2s live | Raised dry waiting pad on each side; sparking strip alone is lethal. Reuse electric hazard prefab and phase owner |
| Sweeping scanner beam | Beam crosses a short exposed walkway in a fixed cycle with 1s endpoint warning and 3s clear interval | Recessed shelter outside full sweep bounds; never scan respawn/portal pads. Moving beam collider must track visible beam, reset parked/safe |
| Latched security shutter | E switch opens a door permanently until room reset; introduces remote routing | Close only when volume is clear; no crushing or pinning. Persistent open/closed baseline belongs to puzzle reset; display cable and matching symbol |
| Retracting bridge | Latched switch extends a walkway and keeps it extended until reset | Reset relocates rat to safe entry before retracting; no timed forced return or stranded checkpoint |

Suggested rollout: Level 1 uses existing hazards plus one shutter; Level 2 introduces electrical strip and latched bridge; Level 3 combines proven mechanics with a steam vent. Reserve the sweeping beam for an optional extra level or introduce it alone in a safe teaching room before any combination. This adds vocabulary, not a requirement to fill every room with all obstacles. Each level brief must be updated with an explicit solo solution, timings and reset baseline before scene authoring. Optional coin routes may add a taught challenge, never require a consumable shield or a lost life.

## Wiring and acceptance checklist
- [ ] Each prefab has explicit phase/visual/collider/audio references, correct layers and unique metadata; no duplicate life handlers.
- [ ] Safe/warning/active states match visuals and collision throughout a complete cycle and at low frame rates.
- [ ] Traversal at walk speed meets the margin; pause/resume, death during every phase and room reset return safely.
- [ ] Portals/checkpoints stay safe; shutters cannot pin rats/crates; bridges cannot strand the player.
- [ ] Solve each authored room with one rat, collect optional coins and recover from every interrupted puzzle state.
- [ ] Add shipped obstacles to Help and record measured timings/Unity results in the level doc and CURRENT_STATE.
