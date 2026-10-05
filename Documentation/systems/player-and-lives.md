# Player and lives

## Existing implementation
PlayerRatController uses CharacterController in the X/Y plane, locks Z, reads A/D or arrows, Space and Shift via Keyboard.current, and includes coyote time. Default source values: move 6, sprint 10, jump height 2, gravity -25, coyote time 0.1 seconds; scene overrides remain authoritative. Respawn disables controller, relocates, clears motion and re-enables it. GroundCheck is retained for inspector/gizmos; actual grounding uses controller contact.
RatLifeManager owns three lives, waiting props and checkpoint position, debounces death during 0.75-second default immunity, then delegates total loss to GameManager. GameManager currently supports Playing/Won/Lost and R restart. Checkpoints only advance to larger numbers.

## Target changes
Retain one active rat; do not convert spare-life props into cooperative players. Pause/menu state must block movement, jump, portal interaction and restart input. Portal travel is not death and must not alter lives/checkpoint. Reuse controller-safe movement reset, but keep semantic teleport entry separate if that helps future behaviour.
Add checkpoint-coordinated puzzle restoration before respawn, preventing a reset block/gate from trapping the next rat. Level restart restores three lives; room reset preserves remaining lives. Lives do not carry between levels under the proposed campaign defaults.
Movement polish: evaluate existing coyote time first; add short jump buffering only after observing missed-input problems. Do not change jump reach after authoring all levels without revisiting every required jump.

## Setup and manual checks
Assign player/start/waiting rats/camera/HUD to RatLifeManager; GameManager references the same life manager and HUD. Enable only GameManager + RatLifeManager, not RatTrialSession. Wire checkpoints with distinct increasing IDs and separate safe respawn transforms. Check three consecutive deaths, hazard overlap, respawn safety, restart, pause, checkpoint backtracking and portal arrival. No old input may cause a second action on leaving a menu.
