# Project R.A.T. — repository instructions

## Start here
- Read `OPEN-FIRST.md` and `Documentation/CURRENT_STATE.md` before choosing work.
- For intended behaviour, read `Documentation/GAME_DESIGN.md`, then the relevant system/level document. `README.md` retains the original GDD and coursework context; the new design explicitly overrides its conflicting level/progression requirements.
- Read `Documentation/ARCHITECTURE.md` before changes across systems. Inspect code and saved assets to confirm documentation; report discrepancies instead of assuming a feature works.
- Read applicable nested `AGENTS.md` files in every directory you edit, including when your working directory is the repository root.

## Scope and authority
Follow [Documentation/GIT_WORKFLOW.md](Documentation/GIT_WORKFLOW.md): each feature uses a separate branch and a pull request before merging. Every commit, push, PR creation (including drafts), and merge requires explicit user permission covering that specific action. Feature implementation requests do not grant these permissions. Prepare reviewable changes first; never automatically publish or merge, enable auto-merge, or treat approval of one action as approval of the next.

User instructions override repository defaults. Confirmed direction: one active player, three puzzle levels inspired by Fireboy and Watergirl, a portal mechanic, home screen and audio settings. Exact layouts and portal rules are proposed defaults, not a claim the team has approved them. Follow `Documentation/DECISIONS.md`; do not copy old concept layouts or implement every pictured power-up automatically. Preserve the rat/laboratory identity and three-rat lives unless the user changes them.

## Unity integrity
Use Unity `6000.3.18f1` from `ProjectSettings/ProjectVersion.txt`. Current rendering is built-in, despite the historical GDD's URP requirement. Do not upgrade engine/packages or migrate rendering without an explicit task. Preserve `.github/`, `metadata.json`, coursework report and attributions.
Keep the existing `Assets/Scripts`, `Assets/Scenes`, `Assets/Materials`, `Assets/Shaders`, `Assets/Prefabs`, `Assets/Audio` layout. Do not reorganise into `_Game` gratuitously. Leave third-party TextMesh Pro resources untouched.
Move/rename assets with their `.meta`; preserve GUIDs and serialized references. Do not regenerate existing metadata. New assets and folders under Assets need unique `.meta` files. Prefer Unity authoring APIs for scene/prefab changes; avoid broad YAML edits. Never silently run a builder that overwrites a hand-authored scene.

## Implementation
Prefer the smallest complete playable feature: scripts, scene/prefab wiring, feedback, reset behaviour and documentation. Do not call a script-only change a completed feature. Extend the established owners before adding managers. Only one life/state system may control a scene. Use explicit serialized references where practical; retain compatibility when changing serialized fields.
Do not guess package APIs from another engine version. Inspect local installed package sources/API usage first. Do not add dependencies merely to implement menus, saving or portals.

## Planning and verification
For multi-system work, use `Documentation/plans/TEMPLATE.md` and put the plan in `Documentation/plans/active/`; track completed, unverified and blocked work separately. Archive only completed plans under `completed/`.
No new formal automated test suite is required. Preserve existing optional validation tools. Check compilation/import, changed references and the relevant manual scenarios in `Documentation/VALIDATION.md` when Unity is available. If not, report static checks and exact remaining Unity steps; never claim compilation or playtesting passed.
Update CURRENT_STATE and affected system docs with each feature, including file paths, setup and limitations. Keep this instruction file short; detailed requirements belong in documentation.
