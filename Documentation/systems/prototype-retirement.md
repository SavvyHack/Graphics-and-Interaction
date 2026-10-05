# Prototype retirement and visual continuity

Status: future implementation guidance. User permits removing unnecessary prototype levels/code and incomplete management systems when they are not needed for the full game. This documentation change removes no runtime assets.

Keep the current prototype's rat/laboratory identity, scale, side-on camera, simple geometry and material vocabulary while improving readability, lighting and feedback. Capture reference views and measure player/camera dimensions before replacing layouts. Existing backups are not completed campaign stages.

## Evidence before removal
RatTrialSession is documented as disabled in the main scene, but disabled alone is not proof it has no editor/prefab dependencies. GameManager and RatLifeManager are active owners and useful integration points. TrialMovingPlatform and TrialWheel are still used despite their folder name. PrototypeHUD has callers even though main-scene references are empty. Do not delete the Prototype folder or replace managers by name alone.

For each candidate, inspect source callers, serialized GUID references in all scenes/prefabs/assets, build entries, editor menu/build/validation tools and documentation. Record keep/replace/remove and dependencies in the implementation plan. Wire and verify the replacement first. One life owner and one attempt-state owner must remain; no competing legacy/new managers. Remove an asset with its .meta only once remaining references are migrated, preserving GUIDs for retained/moved assets.

RatEnclosure and BeforeLaserPuzzle backups may be retired after replacement campaign scenes preserve needed content and navigation, the Git history is confirmed to retain them, and editor redirects/builders/validators no longer require their paths. Do not run scene builders over hand-authored work. Preserve useful optional validation utilities, updating assumptions rather than deleting them to hide failures. Preserve coursework, .github, metadata.json, attributions and third-party TMP resources.

## Acceptance checklist
- [ ] Candidate/dependency inventory and replacement evidence recorded in an active implementation plan.
- [ ] No deleted GUID/class/path references remain in retained project content; historical docs explicitly identify removed content as historical.
- [ ] Unity import/compilation and every changed scene/prefab show no missing scripts/references.
- [ ] Boot from Home, play all levels, die/retry, pause, win and return Home with exactly one relevant state owner.
- [ ] Comparison against prototype reference views confirms continuity of laboratory/rat style and readable gameplay.
