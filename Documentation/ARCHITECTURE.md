# Architecture

## Actual uploaded implementation
Unity 6000.3.18f1; built-in rendering (`GraphicsSettings.m_CustomRenderPipeline` is zero; no URP package in manifest). Runtime scripts use global classes without namespaces. New Input System is installed; player input reads Keyboard.current directly. UGUI and TextMesh Pro are available. No game-specific ScriptableObject configuration classes were found.

| Owner | Existing responsibility | Important coupling |
|---|---|---|
| `Assets/Scripts/Gameplay/PlayerRatController.cs` | CharacterController movement, sprint, coyote jump, plane lock, pushing, respawn | PushBlock, TrialMovingPlatform.Delta, AudioManager |
| `Assets/Scripts/Gameplay/RatLifeManager.cs` | Three lives, monotonically advancing checkpoint, waiting props, respawn immunity | Player, camera, optional HUD, GameManager |
| `Assets/Scripts/Gameplay/GameManager.cs` | Playing/Won/Lost, restart, outcome audio/events | RatLifeManager, optional HUD; no pause/menu state yet |
| `Checkpoint`, `HazardTrigger`, `ExitTrigger` | Route collision events to the life/state owners | Do not attach legacy TrialZone behaviour alongside active equivalents |
| `PressurePlate`, `PushBlock`, `LinkedLaser` | Block-only plate, constrained crate, linked beam suppression | Sources exist, not serialized in uploaded gameplay scene |
| `Assets/Scripts/Presentation/FixedCameraFollow.cs` | Side-on follow/clamp; publishes active-rat shader position | Snap on teleport/respawn using SetTarget |
| `AudioManager` | Scene-local one-shot SFX service | No persistent volume settings or music bus |
| `PrototypeHUD` | Life icons, checkpoint label, win/loss panels | Source exists; not attached in the main scene |
| `Assets/Scripts/Prototype/RatTrialSession.cs` | Legacy life/respawn/outcome harness and OnGUI overlay | Disabled in main scene; do not enable to paper over missing UI |
| `TrialMovingPlatform`, `TrialWheel` | Existing movement/rotation | Still used; Prototype folder does not mean safe to delete |

## Scene/build map
`Assets/Scenes/RatEnclosure.unity`: only enabled build entry; GameManager and RatLifeManager enabled, RatTrialSession disabled. `Assets/StartScene.unity`: original scene, disabled in build; not a home screen. Three BeforeLaserPuzzle scene backups remain authoring history, not campaign levels. No Level1/2/3 scenes exist in this snapshot.

`Assets/Editor/PrototypePlayMode.cs` redirects Play to RatEnclosure unless disabled or a recognized slot is open. `RatLevelSlots.cs` only saves/opens editor scenes. `PrototypeAssetPaths.cs` centralises several authoring paths. Update this infrastructure deliberately when a Home scene becomes the build entry; current validators may assume the prototype remains first.

## Proposed integration boundaries
Keep GameManager as per-attempt gameplay state owner, extending it for pause/reset if appropriate. Add a small campaign/scene-flow owner only for cross-scene concerns: selected level, unlocks and navigation. Keep settings/progress persistence separate from per-attempt lives. Prefer an explicit ordered level list over assuming arbitrary build-index order. No requirement for a dependency-injection framework or event bus.

Proposed `PortalPair`/`PortalEndpoint` components provide pairing, validation and one transport transaction. They ask the controller to reposition safely, then retarget camera. They do not own lives/checkpoints. Proposed puzzle reset coordination registers room-owned blocks/latches, snapshots a baseline and resets those systems atomically with respawn. Do not create parallel death handlers.

Use existing audio entry points where possible. If making audio persistent, move it to a dedicated object: current AudioManager sits with gameplay managers, so DontDestroyOnLoad on that whole object would also preserve stale gameplay state. Ensure exactly one service and one AudioListener after transitions. Use one UI EventSystem per active UI context with compatible input modules.

## Presentation/rendering constraints
GlassEnclosure uses built-in scene capture/GrabPass and the global `_GlassActiveRatPosition`. Preserve camera cleanup and target updates. Imported/third-party TMP content is separate from project shaders. Avoid an unrequested URP conversion, renaming serialized class identities, or reassigning materials globally.
