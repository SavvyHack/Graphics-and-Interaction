# Collectibles, presentation and profile extensions

Status: planned runtime work; specification preparation complete, runtime implementation unverified.

## Goal and design sources
Implement [coins/cosmetics](../../systems/coins-and-cosmetics.md), [settings](../../systems/settings-page.md), [help/lore](../../systems/help-and-lore.md), [victory](../../systems/victory-screen.md), [statistics](../../systems/statistics.md) and [extra obstacles](../../systems/extra-hazards-and-obstacles.md). Use [retirement guidance](../../systems/prototype-retirement.md) only when replacing obsolete content. Feature requests are confirmed; exact prices, lore, timings and counting rules are chosen defaults subject to playtest tuning.

## Current evidence
Existing game/life owners and their events can support outcomes/death accounting. Main-scene HUD is unwired. Shared UI/audio and progression documents already define menus and persistence targets. Coins, cosmetics, profile stats and these new screens have not been implemented in this documentation task.

## Changes and steps
- [x] Inspect applicable instructions, design, architecture and existing owner scripts.
- [x] Add feature specifications with wiring guidance and observable acceptance checks; connect design and navigation references.
- [ ] Complete playable foundation, then establish one versioned profile and transaction/recovery path.
- [ ] Implement settings/help and profile statistics with scene navigation and input gating.
- [ ] Implement one coin and one purchasable outfit end to end; verify reset/relaunch/duplicate transaction behaviour before populating catalog and levels.
- [ ] Wire all victory survivor variants, result snapshots and catalog-based final ending.
- [ ] Author selected hazards and coin routes against updated level briefs, then polish in the established style.
- [ ] Audit and retire obsolete prototype assets only after replacements work.
- [ ] Update CURRENT_STATE with scripts/assets, actual Unity checks and remaining limitations.

## Acceptance and validation
Each linked specification's checklist is required for its implementation; include existing VALIDATION scenarios. No new formal automated suite required. Documentation-only verification: relative links and diff hygiene. Runtime import, compilation, scene wiring, playtesting and real browser persistence/audio checks remain outstanding.

## Handoff
No runtime blockers established by this documentation task. Dependency: working foundation/navigation and a single shared profile owner before economy/stats integration. Keep plan active until implementation and recorded validation satisfy acceptance; do not archive based on completed Markdown alone.
