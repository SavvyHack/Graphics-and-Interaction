# Portal mechanic — proposed specification

Status: new design; no portal source or scene wiring exists. Fixed pairs are a proposed starting point, not a team-confirmed final rule.

## Player contract
- Authored pairs link exactly two pads in the same level. Label each pair with matching symbols/letters and distinct silhouettes as well as colour.
- Only the active rat can use them. At a pad, show “E — Transfer to [pair label]”; E is a proposed new key. No activation from menus, pause, death or completed state.
- Explicit input transfers once. Clear momentum, keep controller plane Z, and snap follow camera to the arrival. No free aiming, momentum puzzles, cross-level travel or crate transport in the first implementation.
- Arrival is a dedicated transform outside the receiving trigger on stable safe ground. Show the destination visually where feasible. Never drop the rat straight into an unavoidable hazard.
- Block reverse activation until the previous interaction is released and the rat has left the destination pad; target a short 0.25-second minimum debounce as a tuning default. This prevents a held key or overlapping triggers from looping.

## Transaction and invalid configuration
Validate enabled pair and destination before moving. Check controller capsule clearance against solid geometry and keep the arrival outside lethal trigger volumes. If invalid/blocked, leave the rat at the source, preserve its state, show “Destination blocked” or an unavailable indicator, and log a useful authoring warning once. No life cost and no fallback to world origin. Do not disable the CharacterController permanently if transfer fails.
On success: gate input for the transaction, reposition safely, clear motion/support state, update camera/glass target and play one departure/arrival cue. Avoid retaining moving-platform carry delta across the transfer. Release lock reliably on completion/reset.

## How portals replace the second player's travel
A portal moves the rat between routes; it does not hold a pressure plate remotely. Persistent bridge switches and crates provide that state. Every required gate must stay passable long enough for one player to use the route. A short hold-open timer is optional and belongs only after the untimed version is proven; no mandatory rapid switching between distant controls.

## Authoring and reset
Proposed fields: partner endpoint, arrival transform, pair label, enabled state and optional availability signal. Both endpoints must link reciprocally. Use same-level pairs, no shared endpoint between pairs. Portal enable state belongs to the room reset baseline. Default transport does not change checkpoints or progression.

## Acceptance
Use each direction repeatedly, release/repress E, stand still on destination, jump into source, approach from both sides, pause on pad, die near pad, disable partner, obstruct arrival and reset room. There must be no double transfer, stale camera, life change or softlock. Keep a dedicated proof room before placing portals throughout the campaign.
