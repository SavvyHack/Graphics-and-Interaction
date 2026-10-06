# Camera follow review — 6 October 2026

Status: implemented; Unity compilation and targeted Play-mode checks passed. Visual playtesting remains unverified.

## Goal and design sources
Audit the requested follow change against [architecture](../../ARCHITECTURE.md) and [player/lives](../../systems/player-and-lives.md). Preserve the outside-glass view, target shader and serialized fields.

## Current evidence
RatEnclosure has one orthographic camera with FixedCameraFollow, target PlayerRat, offset (2.5, 1.8), smoothing 0.15, centre bounds (5, 3.5)–(79, 4.8), size 5.4, depth -22. RatLifeManager snaps on start and accepted respawn. These are camera-centre limits, not enclosure edges.

## Branch and delivery approvals
Branch: codex/camera-follow-review. Base: main at b6c78c0 (clean at start). Follow [workflow](../../GIT_WORKFLOW.md). Commit, push, PR creation and merge are pending user permission. No Git publication or history changes were made.

The user's additional feature request follows this review. A question is pending about allowing one combined branch for the shared campaign/menu/profile update instead of separate feature branches. Default level-count interpretation is the existing enclosure plus four new levels (five total); the older three-level plan is not three implemented levels. The feature additions are not integrated or verified. Preparatory source drafts under ignored `.utmp/feature-drafts/` are incomplete and are not shipped runtime code.

## Changes
- `Assets/Scripts/Presentation/FixedCameraFollow.cs`: one destination calculation for following and snapping; lazy pose capture for calls before Awake; hard-locked depth/rotation; inactive targets ignored; stale velocity cleared on disable; reversed bounds normalized; fixed height retained when vertical following is disabled. Zero-delta frames skip SmoothDamp, avoiding invalid velocity while paused.
- `Assets/Editor/ActiveGlassValidation.cs`: extends the existing optional validation utility with a focused camera check and a batch Play-mode scenario, using the existing RatPlaytestDriver. Does not save or regenerate scenes.
- CURRENT_STATE and player/lives documentation updated. Existing framing, scene geometry, serialized fields and GUIDs are preserved.

## Steps
- [x] Inspect instructions, code, scene camera and respawn wiring.
- [x] Implement focused correction without changing authored framing.
- [x] Retain target/shader cleanup and all existing references.
- [x] Unity 6000.3.18f1 import/compilation.
- [x] Editor checks for pre-Awake snap, bounds, fixed height/depth, target publication and null cleanup.
- [x] Headless Play-mode check in RatEnclosure: scene offsets, 25 paused frames, resumed following, inactive-target cleanup and life-loss retarget.
- [x] Diff whitespace check; remove incidental Unity project-setting reserialization.
- [ ] Visual follow at low/high frame rate and across the full course.
- [ ] Inspect at 16:9 and narrow aspect ratios; centre clamping is not automatic viewport fitting.

## Acceptance and validation
6 October 2026, Unity 6000.3.18f1:
- Import completed successfully; no C# errors. Existing warning CS0414: PlayerRatController.turnSpeed is unused.
- `ActiveGlassValidation.VerifyCameraFollow` emitted `RAT_CAMERA_FOLLOW_OK`.
- `ActiveGlassValidation.VerifyCameraPlayMode` emitted `RAT_CAMERA_PLAYMODE_OK`; the checks execute against the saved RatEnclosure scene without saving modifications.
- Logs in ignored `Logs/campaign-import-authorized.log`, `Logs/camera-validation.log`, and `Logs/camera-playmode.log`.
- The Play-mode log also contains a UnityEditor.Search.SearchDatabase startup indexing exception. It is outside project gameplay code and did not stop camera assertions. This is not a clean-editor-console claim.
- Headless checks do not verify rendering, subjective smoothness, narrow-window visibility or the full campaign. No claim of a visual playthrough or browser-build check.

## Handoff
Prepared local camera changes only. Remaining requested screens, levels, economy, graphics settings and statistics are still outstanding; no script draft is a completed feature. Keep this plan active until visual acceptance or explicitly accepted validation limits.
