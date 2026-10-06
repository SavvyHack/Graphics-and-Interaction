> 6 October update: the seven-level implementation supersedes the early three-level sequencing below. See [CURRENT_STATE](CURRENT_STATE.md) and [adaptation plan](plans/active/05-reference-level-adaptation.md) for verified work and remaining tasks.

# Development roadmap

Requested extensions are tracked in the [active feature plan](plans/active/02-gameplay-and-presentation-specifications.md). Add settings/Help and shared profile accounting during foundation, prove one coin/purchase/outfit end to end before populating levels, and include stats/survivor outcomes in the Level 1 slice. Author optional coin routes and selected extra hazards with each level; finish outfits/lore/presentation during polish. These are full-game targets, not completed items. Retire prototype content only after replacements pass reference and gameplay checks.

All milestones below are pending runtime implementation. Documentation preparation is complete; gameplay completion is not.

| Order | Milestone | Complete when |
|---|---|---|
| 1 | Playable foundation | HUD/outcomes, Home, pause, audio settings and one-level navigation work; no duplicate state owners; first plan below |
| 2 | Portal and puzzle proof | One safe fixed pair, crate-held laser, latched bridge and room reset work together with one rat |
| 3 | Level 1 vertical slice | Tutorial → puzzle → exit → results; save/reload and audio preferences work in a build |
| 4 | Level 2 | Relay brief authored; all checkpoint/reset paths remain solvable |
| 5 | Level 3 and campaign ending | Escape brief, final ending, unlock flow and replay complete |
| 6 | Completion pass | Controls/credits, transitions, audio/visual feedback, browser build, readability/performance and full manual run |

Prioritise fixing missing outcome UI before extending level length. Reuse placeholder art until the complete route is playable. Existing scene backups are not finished levels. No automatic tests need to be created for these milestones.

Optional after completion: short contextual hints; jump buffer; reduced effects; cosmetic rat variation; best time/replay medals. Defer additional power-ups, online services and complex portal rendering.

Next actionable task: implement [playable foundation](plans/active/01-playable-foundation.md), then create a bounded portal proof plan from the template. Each milestone must update CURRENT_STATE with actual Unity checks and remaining limitations.
