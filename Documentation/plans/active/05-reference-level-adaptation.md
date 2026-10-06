# Reference level adaptation



Status: level/gameplay integration verified; particle requirement awaiting specification clarification.



## Goal and design sources

Replace campaign stages 1–2 with the user-supplied Graphics-and-Interaction layouts, retaining the current shaders, campaign, tokens and cosmetics. Update Help. Keep seven levels total. Source pinned to 11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9.



## Current evidence

The source levels require six temporary augments, nine checkpoints each and multi-tier traversal. Local campaign and menus exist in user commit 8f89eec. Previous runtime menu checks remain incomplete.



## Branch and delivery approvals

User approved one local branch for this update. Working from their tavish commit 8f89eec on codex/level-adaptation-and-fire. No commit, push, PR or merge authorized; see [workflow](../../GIT_WORKFLOW.md).



## Changes

New AugmentationLab/ReactorDivide scene copies, compatible augment components, existing controller/camera integration, source attribution and Help. Existing RatEnclosure/TransferWorks retained outside enabled build list. Stable enclosure/transfer save IDs retained. Explicit one-time adapter refuses to overwrite an already adapted scene.



## Steps

- [x] Inspect source, layout requirements and applicable instructions.

- [x] Import scene copies and required gameplay code.

- [x] Wire scenes, shader materials, cosmetics and 20 stable tokens each.

- [x] Compile/import and verify state, camera, UI and augment traversal.

- [x] Inspect gameplay and menu captures.

- [x] Update state, feature docs, source credits and manual checks.



## Acceptance and validation

Seven playable campaign scenes plus Home. One controller/state/life owner each. No broken references; existing profile IDs preserved. Actual runtime checks and unverified human/WebGL scenarios recorded separately.



## Handoff

The published course specification explicitly excludes AI authorship of the assessed particle effect. Prepare fire VFX sockets for a team-authored effect; do not claim a completed assessed particle system. Local assignment spec.md is ignored per user request. No shader code or evaluation authored in this update.


## Verification evidence
Campaign pass succeeded: Logs/adapted-campaign-checks.log. Actual controller input crossed required gaps/shafts/shuttles; profile/menu/reset/progression/endings passed. Screens inspected; camera and presentation checks repeated in Logs/adapted-campaign-final.log. Human continuous route and WebGL checks remain outstanding; WebGL support is absent.

Final Unity run passed: Logs/adapted-campaign-final.log. Camera narrow-aspect/pause/resume checks passed in both imported scenes; corrected map/ending images inspected. No browser build claimed.
