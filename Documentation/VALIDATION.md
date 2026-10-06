# Lightweight validation

No new formal automated test suite is required. Existing optional editor validation remains available. Use checks proportional to the feature; document what actually ran.

## Every gameplay/asset change
1. Open with the pinned Unity version; allow import/compilation and inspect Console errors.
2. Inspect changed scenes/prefabs for missing scripts, references, materials and unexpected overrides.
3. Play the relevant scenario and a failure/reset path. Check that the visible state matches the collider/state.
4. Stop Play; inspect the diff for accidental serialization or generated-file changes.
If Unity is unavailable, run only honest static checks (paths, metadata, references and code review), then list these remaining editor steps. A generic C# build is not a substitute for Unity import and package compilation.

## Milestone manual matrix
| Area | Minimum scenario |
|---|---|
| Boot | Fresh build opens Home; New Game enters correct level; editor redirects do not conceal wrong build entry |
| UI | Every button and Escape route; keyboard focus; pause/settings return; normal time scale after restart/home |
| Lives | Repeated hazard contact costs one life; safe respawn; three losses show failure; retry restores three |
| Puzzles | Solve/unsolve/reset, crate bounds, latch state, death at every checkpoint; no impossible recovery |
| Portal | Both directions, held E, blocked destination, disabled partner, pause/death/reset during use |
| Audio | All buses zero/full, mute restore, persistence, UI while paused, no duplicate loops |
| Campaign | Unlock stages in order, replay earlier stage, Continue from a fresh launch, final ending, reset progress confirmation |
| Visuals | Rat stays visible, glass does not conceal controls, hazard state readable without colour/audio alone |
| WebGL | Browser launch, first-gesture audio, input focus after tab switch, storage fallback, loading, resize and acceptable target-device performance |

## Extended feature checks
Use the acceptance checklists in [coins/cosmetics](systems/coins-and-cosmetics.md), [Settings](systems/settings-page.md), [Help](systems/help-and-lore.md), [victory](systems/victory-screen.md), [statistics](systems/statistics.md), [extra hazards](systems/extra-hazards-and-obstacles.md) and [retirement](systems/prototype-retirement.md). Include duplicate events, death/reset/retry/relaunch, purchase consistency, interrupted-attempt recovery, all survivor variants, optional-route solvability and browser audio/storage. Record observed results separately from unchecked requirements.

## Optional validation tools
Project R.A.T. > Validate Project References and Game Systems > Validate Game Systems may help. Read the implementations before use: old tools assume the prototype layout/build entry and some batch suites exercise the legacy harness. Do not run builders as validation. Do not mandate old smoke suites for unrelated documentation changes; preserve them for useful future checks.

For each milestone, record date, Unity version, scene/build, exact scenarios, observed result and remaining issues in the plan. Never reuse September's historical “passed” statement for a new snapshot.

## Current campaign developer checks (6 October)
Use `CampaignValidation.RunPlayChecks` as a Unity batch executeMethod without `-quit`; it exits on completion and uses an isolated profile. It first checks saved references and transactions, then exercises menus, selected real-input traversal sections, failure/retry, portals/crates, progression and survivor variants. `ReferenceRouteChecks` covers the imported augments and shuttles. Generated images/logs belong in ignored Logs. Do not run the authoring tools as validators.

Manual additions: continuously complete both five-tier reference stages; die/reset at each checkpoint, test shield exclusions and augments expiring, collect optional raised tokens, verify cosmetic accessories on waiting rats, and navigate the new Help content by keyboard/mouse. Check all five later stages end-to-end and all UI at browser sizes. Install matching Unity WebGL support before attempting browser acceptance; it is absent on the current machine. Never claim a completed assessed particle effect while only VFX sockets are present.
