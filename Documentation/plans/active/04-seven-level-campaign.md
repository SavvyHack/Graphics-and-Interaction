# Seven-level campaign and presentation update

Status: implemented; developer runtime checks passed, human/browser verification outstanding.

## Goal and design sources
Implement the user's camera review and seven-level campaign (six additions to the existing enclosure), futuristic level map, Help, Pause, Failure, survivor-dependent ending, wardrobe/shop, optional coins, readable graphics/text, settings and persistent statistics. Use the contracts in [design](../../GAME_DESIGN.md), [saving](../../systems/progression-and-saving.md), [coins](../../systems/coins-and-cosmetics.md), [settings](../../systems/settings-page.md), [Help](../../systems/help-and-lore.md), [statistics](../../systems/statistics.md) and [victory](../../systems/victory-screen.md).

## Current evidence
Camera import/editor/Play-mode checks passed in the previous stage; see [camera review](03-camera-follow-review.md). Home and seven authored scenes, shared menus/profile, tokens and cosmetics now exist in user commit 8f89eec. Unity 6000.3.18f1 is installed. Existing artwork, rat/controller and managers will be retained.

## Branch and delivery approvals
Branch: codex/seven-level-campaign; base: main b6c78c0. User explicitly approved one branch for this complete update and seven levels total. This overrides the separate-feature-branch default for this update only. Commit, push, PR creation and merge still await explicit permission under [workflow](../../GIT_WORKFLOW.md).

## Changes
Extend existing gameplay owners for pause, outcome ordering and puzzle recovery. Add one profile writer, scene flow and shared UGUI/TMP UI. Author Home and six new saved level scenes with Unity APIs; narrowly wire the existing enclosure without regenerating its geometry. Explicit authoring tool must refuse to overwrite existing new scenes. Preserve all existing GUIDs, third-party resources and original coursework artifacts.

## Steps
- [x] Inspect current source, assets, applicable instructions and installed package APIs.
- [x] Correct and check camera.
- [x] Integrate profile, settings, lifecycle and all screens.
- [x] Author six distinct levels and wire seven-level catalog/build entry.
- [x] Configure coins, outfits, audio and sharp text/rendering.
- [x] Compile/import; validate saved references and meaningful runtime scenarios.
- [x] Capture and inspect rendered menus/gameplay.
- [x] Update current state, affected system/level docs and remaining manual checks.

## Acceptance and validation
Track implemented, checked and unverified separately. Target seven playable stages, 20 permanent unique coins each, three lives per stage, no cosmetic gameplay advantage. UI supports mouse/keyboard and correct pause/back routes. Saves obey attempt/economy invariants and recovery. No new formal automated test suite; targeted existing-editor utility checks as appropriate. Full route playthrough and WebGL remain unverified until actually performed.

## Handoff
In progress; no commit or publication authorized.

## Continued verification and source-level replacement
The user committed the initial work as 8f89eec and requested source layouts for the first two stages. Continue on codex/level-adaptation-and-fire; see [adaptation plan](05-reference-level-adaptation.md). CampaignValidation.RunPlayChecks passed in Unity 6000.3.18f1 on 6 October after correcting the synthetic input driver. Logs/adapted-campaign-checks.log records scene/profile checks, actual input traversal, pause/retry, portal/latch/crate reset, catalog flow and survivor outcomes. Full human route and WebGL verification remain outstanding; no publication performed.
