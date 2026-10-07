# Help page, field guide and lore

## Implementation update - 6 October 2026
CampaignUI.ShowHelp implements Controls, Power-ups, Field guide and Our story, reachable from Home and Pause. The Power-ups section (7 October) lists all 11 shipped augments using RatPowerups.Names, Hints and Durations, plus collection, stacking, fuel, shield/pulse/phase limits and reset rules. The field guide covers the seven current routes. Static review only for this revision; Unity scrolling, readability and Home/Pause navigation remain to check. Historical specifications below do not describe current shipped scope.

## Earlier acceptance specification
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

Status: target specification. User requested main-menu help explaining power-ups and the game's background. Lore text below is original proposed in-game copy, available for implementation.

## Navigation and presentation
Home has Help; Pause may open the same page and return still paused. Tabs: Controls, Field Guide, Our Story. Back/Escape returns one navigation level and restores focus. Controls remains a shortcut to the Controls tab where existing menu specifications mention it. Use scrollable laboratory dossier panels with icons, readable text and keyboard tab/scroll support. All content is available from the start; no paid hints or lore unlock requirements.

## Our Story — proposed copy
"Behind the glass of a quiet research laboratory, three rats have learned the rhythm of the tests. Doors click, warning lamps blink, and every enclosure promises another reward token. Tonight, the transfer equipment has been left running. One rat ventures ahead while the others wait their turn. Move the laboratory's blocks, outwit its security systems and use its transfer pads to find a way outside. Every rat that makes it through is a reason to celebrate."

R.A.T. remains the working project name; do not invent an official acronym expansion. Keep the tone curious and hopeful, with stylised non-graphic danger. Losing a life means a rat is out of the attempt; avoid claiming killed rats are resurrected by cosmetics or coins. Three rats are available anew per level, as specified by progression; the ending count describes the final level, not campaign-wide survivors.

## Controls and field guide
Read actual configured bindings when possible; otherwise keep displayed labels synchronized with the controller. Document movement, jump, sprint, E interaction, pause and confirmed Restart Level. No help shortcut may bypass pause/input gating. Every entry has an icon, plain purpose, activation, duration/reset rule and one short practical tip.

Required entries: rats/lives and checkpoint recovery; blocks and block-only plates; latched switches; fixed portal pairs (rat only, no velocity carry); coins and cosmetic-only Wardrobe; room reset; each shipped hazard from [hazards](extra-hazards-and-obstacles.md). State explicitly that coins/outfits do not grant protection and that a checkpoint does not restore lives. Distinguish reusable mechanisms from consumable power-ups.

## Power-ups: explain only what exists
There are currently no committed playable power-ups in the three-level design. Do not implement every old concept solely to populate Help. Before power-ups ship, show: 'This escape uses laboratory mechanisms, not power-ups. Use blocks, switches and transfer pads to make a safe route.'

The following are design-backlog concepts, not active Help entries: jetpack (temporary upward travel), double-jump (extra airborne jump), shield (limited hazard protection), speed boost (temporary faster movement), gravity pulse (changes local movement/puzzle forces), slow time (slows selected hazards). Their bindings, duration, stacking, consumption, affected hazards, checkpoint/reset and portal interactions are undecided. Keep them out of the player-facing guide until an explicit feature specification settles those rules and the feature is wired and verified. When one is approved, add a matching icon and entry describing its exact implemented purpose and limits; help is part of that feature's acceptance.

## Acceptance checklist
- [ ] Home/Pause access, tabs, scrolling and Back work with keyboard/mouse and retain correct pause state.
- [ ] Every shipped mechanism/hazard has accurate help; no absent power-up is advertised as usable.
- [ ] Instructions match bindings, plate detection, portal/reset behaviour and coin persistence.
- [ ] Lore, icons and text remain readable at supported window sizes; content uses the rat/lab identity.
