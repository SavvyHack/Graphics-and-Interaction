# Puzzles and hazards

## Reusable code and limitations
`PushBlock` is a kinematic gravity-free X-track crate with bounded travel; player ground movement calls Push. `PressurePlate` checks overlapping PushBlock colliders and requires the crate centre inside its X/Z footprint. It does not accept the rat. It drives exactly one LinkedLaser and updates its top/indicator each physics tick.
`LinkedLaser` positions a beam between two endpoints and synchronizes visible beam/lethal collider; `SetSuppressed` stores one boolean. Do not attach several writers and expect OR/AND logic. Existing fire/electric/laser prefabs instead use TimedHazard + HazardTrigger. These are different laser implementations; choose deliberately per puzzle.

## Target puzzle vocabulary
1. Block-held plate: weight disables one laser while block stays centred.
2. Latched switch: explicit interaction opens a bridge/gate until room reset; new implementation needed.
3. Fixed portal pair: routes one rat between locations.
4. Moving platform or clearly telegraphed timed hazard: reuse existing components with readable waiting zones.
Use visible cables, matching symbols and state lights to connect controls to outputs. Never rely solely on sound or colour. If multiple signals are needed, define and aggregate ALL/ANY requirements in one owner before applying gate state.

## Recovery contract
Rooms have bounded crate tracks with access to both sides where repositioning is necessary. No unavoidable crate loss. Local Reset Puzzle restores crates, latches, gates and portal availability to the current room baseline, resets input locks and places the rat at that room's safe entry without restoring lives. Death uses the checkpoint's corresponding baseline. Do not reset an earlier bridge that is the only way out of a checkpoint.
Show hazard activity honestly: disabled collider matches safe visual, active collider matches visible danger. Add warning cues before timed activation; tune safe durations against actual traversal time. Avoid unavoidable damage after portal/respawn.

## Setup and verification

See [extra hazards and obstacles](extra-hazards-and-obstacles.md) for additional vocabulary, style, warning timings and acceptance. Inspect existing components before reuse; those targets are not claims about current prefabs. Optional [coin routes](coins-and-cosmetics.md) obey the same solo-solution and safe-reset rules.
Inspect current scene before running LaserPuzzleUpgrade; the tool makes backup/geometry changes and is not a general multi-room authoring framework. Build reusable prefabs only after one puzzle works. Check no-weight/centred/edge-weight plate states, crate travel bounds, laser safe/dangerous states, reset before and after solve, death during each puzzle state and room re-entry. Never require the player to stand on the current block-only plate.
