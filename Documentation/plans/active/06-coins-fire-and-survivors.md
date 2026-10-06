# Coins, fire particles and surviving rats

Status: in progress.

## Goal and design sources
Address the user's report of absent level-one coins, missing basic fire particles, and lost rats being restored between levels. This latest direction overrides the previous per-level-three-lives default. See coins-and-cosmetics, player-and-lives and fire-effect-handoff system docs.

## Current evidence
20 serialized coins exist in AugmentationLab, but CoinPickup.Start hides every lifetime-collected ID. RatLifeManager.Start unconditionally sets three lives. Fire sockets are empty. User committed prior work as 6b41f6d; working tree was clean.

## Branch and delivery approvals
Continue authorized combined branch codex/level-adaptation-and-fire. Commit, push, PR and merge remain unapproved under [workflow](../../GIT_WORKFLOW.md).

## Changes
Repeatable per-attempt rewards with separately retained unique discoveries and backward-compatible wallet migration; saved level-entry survivor counts; matching waiting props/retry/Help; a saved basic fire ParticleSystem prefab, material and explicit scene hookups synchronized to hazard activation, pause and Slow Time. Record AI assistance honestly. No shader authorship or participant evaluation.

## Steps
- [x] Inspect actual scenes, runtime owners and applicable instructions.
- [x] Fix economy and survivor progression (code; not playtested).
- [x] Author and wire the particle effect through Unity APIs (`FireParticleAuthoring.Build`, batch mode; 4 sockets filled).
- [ ] Verify migration, duplicate/death/retry behavior, 2/1-rat transitions and particle phases.
- [x] Update documentation with actual evidence and remaining limitations.

## Acceptance and validation
Coins appear on replay without erasing wallet or outfits; duplicate contacts cannot reward twice in one attempt. Next stage retains one/two survivors and retry restores stage-entry count. New Game restores three. Fire effect emits only in live phase, freezes with pause, matches Slow Time, and has bounded particles. Preserve existing authored scene geometry and GUIDs.

## Handoff
Unity was closed. Batch mode compiled cleanly and built and placed the prefab. Still renders were checked. Remaining: in-editor playtest of coin respawn and wallet, survivor carry-over and retry (2- and 1-rat), and fire live/safe phases, pause and Slow Time. Also a WebGL performance check and a gameplay GIF for REPORT.md.
