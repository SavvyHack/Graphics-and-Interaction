# Current state — inspected 5 October 2026

Evidence: source and serialized assets inspected, no Unity execution. This is the uploaded snapshot plus documentation changes only.

| Area | Evidence and status |
|---|---|
| Player | CharacterController, movement/sprint, coyote time, respawn and moving-platform support exist; one enabled player component in RatEnclosure |
| Lives/state | RatLifeManager and GameManager serialized enabled; references to player and life manager exist; legacy RatTrialSession disabled |
| Hazards/checkpoints | Main scene has enabled hazard and checkpoint components and one ExitTrigger; timed hazard prefabs exist |
| Camera | FixedCameraFollow serialized enabled; active-rat glass positioning implemented |
| Audio | AudioManager serialized enabled with five clip GUIDs and AudioSource; no volume persistence/music control |
| HUD | PrototypeHUD code exists but no component in main scene; both GameManager.hud and RatLifeManager.hud are null. Trial harness OnGUI cannot provide HUD while disabled |
| Block/plate/linked laser | Three sources and LaserPuzzleUpgrade editor tool exist, but none of those three runtime component GUIDs occur in the uploaded main scene |
| Campaign | Only RatEnclosure enabled in build; StartScene disabled; no Level1.unity, Level2.unity or Level3.unity files |
| Level-slot menus | Editor scene save/load only; not a player save system |
| Portals, home, pause, settings, Continue | Not found in gameplay implementation |
| Art | Primitive/placeholder rat and laboratory geometry; existing water, glass, rat and hazard shaders/materials |
| Formal tests | Existing editor smoke/validation utilities retained; no new automated suite added |

## Concrete gaps to address first
- Build a real HUD and outcome buttons; completion/failure code currently has no wired HUD to display them.
- Reconcile future portal puzzle reset with current death: RatLifeManager respawns the rat but does not snapshot or reset puzzle state.
- PressurePlate only detects PushBlock. Do not assume stepping on it works.
- LinkedLaser has a single suppression boolean; multiple plates writing to it would compete. Aggregate inputs before designing multi-plate puzzles.
- PushBlock stores origin in Awake and clamps X travel; it cannot be assumed to move vertically or pass through portals.
- GameManager has no paused state; setting timeScale alone is insufficient for input gating and menu focus.
- The active build does not start on Home. Editor redirect and old validation expectations must change together when Home is implemented.

## Next milestone

### Specification extension — 5 October 2026
Added target specifications for coins/cosmetics, detailed settings, Help/lore, survivor victory, statistics and extra obstacles; see [feature index](../OPEN-FIRST.md) and [active extension plan](plans/active/02-gameplay-and-presentation-specifications.md). The missing OPEN-FIRST.md is now a development entry point. This update changes documentation only: no runtime implementation, scene wiring, art/audio changes or prototype deletion. Unity compilation/import and playtesting remain unverified. Referenced prototype components and existing managers require a dependency/replacement audit before removal.
Execute [01-playable-foundation](plans/active/01-playable-foundation.md): HUD/outcomes, home/pause/audio settings, explicit single-level flow, and a safe portal proof room. Then author the full three-level campaign. See [roadmap](ROADMAP.md).

## Verification limits
No claim of successful compilation, import, rendering, browser build or gameplay is made for this delivery. Old documentation's September verification is historical. Static archive/reference findings and exact unchanged-file checks are recorded in CHANGELOG_SETUP.
