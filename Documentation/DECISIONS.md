# Decisions and assumptions

## Confirmed user direction
- Each feature uses its own branch and PR before merge. Every commit, push, PR creation and merge needs explicit user permission for that action; no automatic publication or merging. See [Git workflow](GIT_WORKFLOW.md).
- Prepare the repository for continued Codex development using AGENTS.md and appropriate supporting Markdown.
- Three levels; explore Fireboy and Watergirl-inspired designs instead of the old GDD layouts.
- Single-player portal feature to adapt puzzles designed around two players.
- Home screen and settings, mainly audio; include missing features needed for polish/completion/fun.
- No new formal automated tests required.

## Proposed defaults, available for revision
Fixed portal pairs with E interaction; only the rat teleports; clear velocity and snap camera. Crates hold plates and latches hold remote state. Three independent stage scenes; three rats per stage; checkpoint-local reset; level-boundary persistence. Keyboard/mouse first; retain existing movement controls. Original level briefs, not selected reproductions of specific reference levels. Extra power-ups deferred.
These defaults allow implementation plans to be concrete. They are not statements of user approval or shipped behaviour. If a later task asks to implement a documented milestone, use them unless the user changes them.

## Decisions that need evidence before broad changes
1. GDD claims URP is required by coursework; manifest and shaders use built-in. Confirm rubric/teacher requirement before rendering migration. This does not block documentation, gameplay or UI work in the current pipeline.
2. Exact reference levels are not supplied. If the team chooses some, record screenshots/source and the solo adaptation before replacing these briefs.
3. Final title remains Project R.A.T. as a working title. Do not rename the whole project without a naming decision.
4. Target WebGL is stated in the GDD and submission workflow. Browser performance and audio behaviour still need a real build check.

## Requested extensions — 5 October 2026
Confirmed: coin pickups purchasing cosmetic rat outfits; detailed settings; main-menu Help explaining power-up purpose and lore; survivor-dependent victory; persistent deaths/time/coins/runs statistics; extra hazards; continuity with prototype visuals. Unneeded prototype scenes/code and incomplete managers may be retired when replacement/dependency evidence supports it.

The [feature index](../OPEN-FIRST.md) links chosen defaults: 20 permanent unique coins per level, outfit costs 10/20/30, profile retention across New Game, runs defined as level attempts, final-attempt survivor endings, audio defaults, original lore and proposed hazard timings. User delegated these details; tuning remains subject to playtesting. None is implemented by documentation alone. Playable power-ups remain deferred. No runtime assets were removed in this task.

## Evidence reporting
Use “source exists”, “serialized in scene”, “checked in Unity” and “playtested” separately. Historical docs describing successful checks are not evidence that this uploaded snapshot passes now. Never turn a proposed behaviour into a CURRENT_STATE success merely because documentation exists.
