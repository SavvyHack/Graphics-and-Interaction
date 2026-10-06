# Enclosure rebuild of levels 3-7

Status: implemented; developer route checks run in batch mode; human playtest pending.

## Goal and design sources
User direction, 6 October 2026: levels 3-7 should follow the structure and look of levels 1-2. They should use the active shaders and the fire particle system, introduce new power-ups progressively and get harder. The user chose five new augments (one per level) and dropped the crate, plate, transfer-pad and latch puzzles. See [levels 3-7](../../levels/enclosure-levels-3-7.md).

## Branch and delivery approvals
Branch `feature/enclosure-levels-3-7`, stacked on `codex/level-adaptation-and-fire` (open PR #1, not merged); retarget to `main` after #1 merges. Under the [workflow](../../GIT_WORKFLOW.md), the user approved one commit, push and PR creation on 6 October 2026 ("yes commit and open a pr"). Merge remains pending.

## Changes
- Runtime: `RatPowerups` (Dash, WallJump, Glide, Phase and GroundPound appended), `PlayerRatController`, `HazardTrigger`, new `PhaseBarrier`, `Updraft`, `BreakableHatch`; HUD hints and Help text in `CampaignUI`.
- Editor: `EnclosureLevelBuilder` (overwrites levels 3-7 from Augmentation Lab, keeping scene GUIDs), `EnclosureRouteChecks`. `FireParticleAuthoring` now covers every campaign scene and skips sockets that already hold the prefab. `CampaignValidation` drops the old portal, latch and crate checks and runs the new route checks.
- Assets: `PhaseField.mat` and `Updraft.mat` (AnimatedEnergyHazard shader); rebuilt `RelayArchive`, `CoolantFoundry`, `ScannerGallery`, `ContainmentCore` and `EscapeSpire` scenes.

## Steps
- [x] Inspect levels 1-2 structure, templates, physics and harness.
- [x] Implement augments and level pieces.
- [x] Build the five scenes and place fire particles.
- [x] Inspect rendered overviews; fix label overlaps and an unforgiving perch.
- [x] Run play-mode campaign checks (see CURRENT_STATE for the result).
- [ ] Human playthrough of all five levels, difficulty tuning and a WebGL performance check.

## Handoff
Edit layouts in `EnclosureLevelBuilder`, not in the scenes; it overwrites them. Old puzzle scripts remain but are unused by the campaign.
