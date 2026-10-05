# Milestone 1 — Playable foundation

Status: planned. This delivery writes the plan; it does not implement the milestone.

## Goal
A user can start the existing prototype from Home, understand remaining lives, pause, adjust audio and recover from win/loss using visible buttons. This makes later portal levels usable as a game.

## Evidence
Main scene already enables GameManager/RatLifeManager/AudioManager, but lacks PrototypeHUD and has null HUD references. RatTrialSession/TrialZone are disabled legacy components. StartScene is not Home. PrototypePlayMode and build settings launch RatEnclosure. No persistence or settings exists.

## Ordered work
- [ ] Create a reusable UGUI/TMP HUD with life icons, checkpoint, success/failure and restart/home controls; assign both manager HUD references explicitly.
- [ ] Extend existing attempt state/input gating for pause. R must not silently reset while entering UI; pause must not leave stale velocity or active hazard input.
- [ ] Create `Assets/Scenes/Home.unity` with New Game, Settings, Controls and Credits. Add Continue/Level Select only when their stored-level behaviour exists; do not leave decorative dead buttons.
- [ ] Implement Master/Music/SFX and mute, persistence and immediate preview. Reuse event audio calls; isolate persistent audio from the existing shared gameplay-manager object.
- [ ] Add a minimal explicit Home↔prototype navigation path and clean time scale/audio lifetime handling.
- [ ] Deliberately update build entry and editor Play redirect behaviour; retain a way to play a currently open scene. Inspect validators for old entry-scene assumptions.
- [ ] Check boot, pause, all audio controls, three deaths, success, replay, home and repeated scene transitions in Unity and one target build.
- [ ] Update CURRENT_STATE and ARCHITECTURE; record results below.

## Files likely affected
Existing GameManager, PlayerRatController, PrototypeHUD, AudioManager, PrototypePlayMode, ProjectValidation and EditorBuildSettings; new UI/settings/scene-flow components and a Home scene/UI prefab. Exact class split follows repository patterns; avoid new packages.

## Acceptance
No blank frozen loss/win state; no legacy life harness enabled alongside the current life system; no duplicate audio; audio settings survive relaunch; gameplay pauses correctly; menu back navigation is predictable. If new music is unavailable, record missing asset work explicitly instead of claiming a Music slider controls an audible track.

## Follow-on
After this foundation, implement a single fixed-pair portal proof plus latched switch/room reset before authoring full levels. Use systems/portals.md and puzzles-and-hazards.md. Formal automated tests are not required.

## Results
Not yet implemented or run.
