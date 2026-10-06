# Runtime architecture

Unity 6000.3.18f1, built-in rendering, existing Input System and UGUI/TextMesh Pro packages. No new dependencies or rendering migration.

## Ownership
- `GameManager`: sole attempt state owner (Playing, Paused, Won, Lost); freezes outcome and dispatches one completion event after the frame's triggers.
- `RatLifeManager`: three lives, checkpoints, immunity and room reset; one active `PlayerRatController` plus two waiting props.
- `PlayerRatController`: movement, buffered/double jump, jet thrust, crate push and `TrialMovingPlatform.Delta` carry. Pause and terminal states gate input.
- `FixedCameraFollow`: one position/size owner, fixed depth/rotation, shared follow/snap bounds, zero-delta guard, narrow-view minimum width and `_GlassActiveRatPosition` publication/cleanup.
- `CampaignSession`: scene-to-profile bridge, attempt timing/lifecycle and checked navigation through `CampaignCatalog`.
- `CampaignProfile`: versioned PlayerPrefs JSON writer, journal recovery, stable token IDs, purchases, settings, progression and statistics. Validators use a temporary profile key.
- `CampaignUI`: runtime canvas and menus with explicit scene references to managers, TMP font and display prefab. One InputSystem EventSystem. Preview rats use layer 31, excluded from gameplay cameras.
- `AudioManager` and `CampaignSettings`: scene audio and saved settings. Audio does not preserve stale gameplay managers across scenes.

## Traversal and reset
`PuzzleRoom` resets registered crates/switches/portals/hazards for its checkpoint. `PortalEndpoint` transports only the rat between reciprocal safe arrivals. `LatchedSwitch`, `PressurePlate`, `PushBlock` and `LinkedLaser` retain distinct responsibilities.

In stages 1-2, `RatPowerups` owns six temporary timers, shield grace and jet fuel; `RatPickup` stations recharge. Death or Reset Puzzle clears powers and replenishes stations. `CampaignPulseGate` remains latched for the attempt. `CampaignHazardCycle` controls one visible/lethal object; its child fire VFX socket shares activation. Machinery uses `RatPowerups.WorldScale`, not global time scaling, for Slow Time. Augments are distinct from permanent `CoinPickup` rewards and `RatCosmetic` appearance.

## Scenes and assets
Build starts at `Assets/Scenes/Home.unity` then seven explicit paths in CampaignCatalog. See [scene table](levels/README.md). Original enclosure, TransferWorks, starter and backups remain disabled in the campaign build. Editor Play honours a current campaign scene and otherwise opens Home. Imported scene GUIDs are new; shared project assets retain GUIDs. Source moving-platform/wheel GUIDs resolve to retained local Trial components.

`ReferenceLevelAdaptation` makes one-time changes to copied source scenes and refuses repeat adaptation. `CampaignAuthoring` is an explicit initial generation tool, not a runtime builder. Never run either to repair an already authored scene. Third-party TMP resources are not intentionally changed; discard incidental generated font-cache changes.

## Verification boundaries
Optional `CampaignValidation`, `ReferenceRouteChecks` and `ActiveGlassValidation` are focused developer utilities, not human usability evaluation. See [CURRENT_STATE](CURRENT_STATE.md) for actual runs and limits.
