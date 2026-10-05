# Development rules

## Before editing
Read root and scoped AGENTS instructions; inspect affected scene/component references and current user changes. Pick a bounded feature with observable completion criteria. Preserve existing asset paths and original GDD/report attribution. Keep design intent separate from actual implementation.

## Unity changes
Use serialized private fields for new configuration where consistent; do not bulk-convert old public fields. Use FormerlySerializedAs when a serialized field must be renamed; retain compatible class/file identities. Keep editor code under Assets/Editor. New scripts/assets and directories under Assets require metadata; preserve all existing GUIDs. Do not add documentation outside Assets to Unity import just to obtain metadata.
Prefer scene/prefab authoring through Unity APIs with Undo, dirty marking and deliberate saves. Tools must describe what they overwrite. Use disposable scene copies for destructive builder experiments. Never replace a hand-edited course by running RatLevelBuilder without a task specifically authorizing regeneration.

## Small complete features
Implement behaviour, visual/audio feedback, Inspector hookups and recovery together. Document defaults and tuning values. Use existing owners; avoid another life manager, controller or camera just to integrate a feature. Any singleton/persistent service needs a clear lifetime and duplicate prevention. Preserve coursework workflows and team metadata.

## Documentation maintenance
Keep root README as historical GDD/coursework entry with a prominent current-design link. Update only relevant system docs and CURRENT_STATE; avoid duplicated contradictory specifications. Plans record actual completion and remaining validation. New speculative ideas go in ROADMAP/DECISIONS as optional, not mandatory scope.

## Finish report
State changed files/behaviour, how to open/use the feature, static checks performed, Unity/manual checks actually run, and unresolved blockers. No formal unit tests are required; existing tools are optional and must not be removed just because this workflow is informal.
