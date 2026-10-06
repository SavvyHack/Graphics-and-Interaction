# Player and lives

## Implementation update - 6 October 2026
PlayerRatController retains crate push and moving-platform carry, now with jump buffering and RatPowerups double jump/jet thrust/speed support. RatLifeManager remains the sole life/checkpoint owner. Shields absorb one hazard contact with grace; deep coolant/falls bypass them. Pause, retry, powered traversal, shield crossing and three-loss failure passed developer checks.

Survivor carry-over (user direction, 6 October 2026): `CampaignProfile.Data.entryRats[level]` stores how many rats enter each level. Clearing a level writes its survivors (1-3) into the next level's entry. `RatLifeManager.Start` reads `CampaignProfile.StartingRats` into `EntryLives`. Retry restores the entry count, not three, and waiting-rat props reflect it. New Game resets every entry to three. This supersedes "Lives do not carry between levels" below. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Earlier acceptance specification
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

## Existing implementation
PlayerRatController uses CharacterController in the X/Y plane, locks Z, reads A/D or arrows, Space and Shift via Keyboard.current, and includes coyote time. Default source values: move 6, sprint 10, jump height 2, gravity -25, coyote time 0.1 seconds; scene overrides remain authoritative. Respawn disables controller, relocates, clears motion and re-enables it. GroundCheck is retained for inspector/gizmos; actual grounding uses controller contact.
RatLifeManager owns three lives, waiting props and checkpoint position, debounces death during 0.75-second default immunity, then delegates total loss to GameManager. GameManager currently supports Playing/Won/Lost and R restart. Checkpoints only advance to larger numbers.

## Target changes
Retain one active rat; do not convert spare-life props into cooperative players. Pause/menu state must block movement, jump, portal interaction and restart input. Portal travel is not death and must not alter lives/checkpoint. Reuse controller-safe movement reset, but keep semantic teleport entry separate if that helps future behaviour.
Add checkpoint-coordinated puzzle restoration before respawn, preventing a reset block/gate from trapping the next rat. Level restart restores three lives; room reset preserves remaining lives. Lives do not carry between levels under the proposed campaign defaults.
Movement polish: evaluate existing coyote time first; add short jump buffering only after observing missed-input problems. Do not change jump reach after authoring all levels without revisiting every required jump.

## Setup and manual checks
Assign player/start/waiting rats/camera/HUD to RatLifeManager; GameManager references the same life manager and HUD. Enable only GameManager + RatLifeManager, not RatTrialSession. Wire checkpoints with distinct increasing IDs and separate safe respawn transforms. Check three consecutive deaths, hazard overlap, respawn safety, restart, pause, checkpoint backtracking and portal arrival. No old input may cause a second action on leaving a menu.

## Camera correction — 6 October 2026
`FixedCameraFollow` uses the same target/offset/bounds calculation for smooth following and snaps. Zero-delta frames skip smoothing; Z and rotation stay fixed; disabling clears follow velocity and the glass target. Bounds constrain the camera centre and normalize reversed limits. Turning off vertical following retains the authored height. Existing serialized framing is preserved; aspect-ratio readability still needs a visual check.

Camera verification on 6 October: pinned Unity import/compilation and the existing ActiveGlassValidation utility's focused camera and headless Play-mode checks passed (snap, pause/resume, fixed pose, inactive target and respawn retarget). Visual/aspect-ratio checks remain. See [evidence and limits](../plans/active/03-camera-follow-review.md).
