# Levels 3-7: enclosure rebuild

User direction, 6 October 2026: levels 3-7 follow the five-floor glass-enclosure structure and look of levels 1-2. Each level introduces one new augment, then combines it with older ones, and each gets harder. Crate, plate, transfer-pad and latch puzzles are dropped from the campaign. Their scripts stay in the project, unused.

## How the scenes are made
`Assets/Editor/EnclosureLevelBuilder.cs` (menu **Project RAT > Rebuild Enclosure Levels 3-7**) copies `AugmentationLab.unity` over each target scene but keeps the target's `.meta`, so scene GUIDs and build settings are unchanged. It then replaces the route sections.
- **Kept from the template:** glass enclosure, test-subject rats, camera, UI, lighting, systems and the 20 tokens.
- **Cloned from the template:** stations, checkpoints, fire vents, lasers, electric gates, wheels, elevator, magnetic gate, coolant hazards and labels. They keep level 1's shaders and materials.
- **Built in code:** decks, risers and walls use level 1's deck style.
- **Fire particles:** `FireParticleAuthoring.Build` places the fire particle prefab in every vent.

Re-running the builder overwrites levels 3-7, so lay out changes in the builder rather than by hand-editing those scenes.

Floors sit at deck tops 1.0, 6.2, 11.4, 16.6 and 21.8. The route zig-zags upwards, and the exit is always at x = 40 on floor 5.

## New augments
| Level | Augment | Control | Duration | Level piece |
|---|---|---|---|---|
| 3 Relay Archive | Dash | Q, once per jump; flies level for 0.22s at 20 m/s | 15s | Wide gaps, wheels |
| 4 Coolant Foundry | Wall jump | Hold towards a wall to slide, Space kicks off it | 18s | Chimneys (`Chimney inner/outer wall`, cyan climb guides) |
| 5 Scanner Gallery | Glide | Hold Space while falling (max fall 1.6 m/s) | 15s | `Updraft` columns lift only a gliding rat; lift eases off over the top 1.2m so the rat hovers instead of overshooting |
| 6 Containment Core | Phase | Passive: pass `PhaseBarrier` fields and every hazard except coolant and falls | 7s | Violet containment fields |
| 7 Escape Spire | Ground pound | S / Down in mid-air | 15s | `BreakableHatch` cracked hatches over service channels |

Runtime code: `RatPowerups` (augment enum appended, so existing station kinds keep their values), `PlayerRatController` (dash, wall jump, glide, pound), `HazardTrigger` (phase immunity), and the new `PhaseBarrier`, `Updraft` and `BreakableHatch`. New materials, `PhaseField.mat` and `Updraft.mat`, reuse the AnimatedEnergyHazard shader. Players in levels 3-7 have dash streak, glide canopy and phase shimmer visuals.

## Wheel traps
The reference wheel had two crossed arms on a hub 1.3m above the deck. A blade always swept through rat height, so no wheel could be passed, including level 1's. `EnclosureLevelBuilder.FixWheelTraps` (menu **Project RAT > Make Wheel Traps Passable**, also run first by the level builder) changes level 1's wheel; every level clones it.
- **One arm:** the cross arm is removed, leaving two blades.
- **Higher hub:** it now sits 2.1m above the deck.
- **Result:** blades only reach a standing rat within about 43 degrees of vertical, leaving a clear window every half turn. That is about 1.5s at 60 degrees/s and 1s at 95 degrees/s.
- **Speeds:** wheels you must time run at 60-95 degrees/s. Wheels inside phase gauntlets are faster, since phasing passes them.
- **Jumping:** a jumping rat can still be hit.

## Routes
- **3 Relay Archive (9 checkpoints):** a dash practice over a safe trench, then a dash across the coolant and a double-jump perch. On floor 2: shield the laser, dash the gap and wait out two vents, then double jump to a perch. Floor 3 needs speed and dash for a 14m gap, then an electric gate and the elevator. Floor 4: dash past the wheel and across the gap, then wait out the vents and jetpack up. Floor 5: shield the scan laser, pass an electric gate and dash over a pit.
- **4 Coolant Foundry (9):** coolant pools, a slow-time shuttle, and four wall-jump chimneys alternating right and left. Between them: a dash over pits, a coolant shuttle, a fast wheel, and a dash across a gap above coolant.
- **5 Scanner Gallery (8):** glide practice over a safe trench, then updrafts to floors 2, 3 and 5. A scanner corridor of timed lasers and a gate leads to a gravity-pulse magnetic gate. Floor 3 is a long glide with a mid-air updraft. A wall-jump chimney climbs to floor 4, where you glide over coolant using an updraft. Floor 5 has a triple scanner and a second magnetic gate.
- **6 Containment Core (9):** containment cells closed by violet fields. Fire, lasers, wheels and electric gates are passable while phasing, but coolant is not. Also: a phase-gated wall-jump chimney, a jetpack shaft, an updraft and a dash gap. Phase lasts 7s, so the gauntlets are timed.
- **7 Escape Spire (7, fewest):** ground-pound practice into a safe trench, two service channels reached by pounding hatches, a wall-jump chimney, and a glide with an updraft to a chimney. Floor 5 is a phase gauntlet and a pit; at the end you double jump onto the exit vault roof and pound through its hatch. Hazards cycle faster and there are fewer checkpoints.

Every new augment is first used where failing is safe: a trench or chimney floor, never coolant. Glides are sized for a walking (not sprinting) rat, and the route checks glide at walking speed.

## Verification
`Assets/Editor/EnclosureRouteChecks.cs` runs inside `CampaignValidation.RunPlayChecks` with simulated keyboard input. Each level runs its own segments. Positive checks cover every new-augment crossing and every climb between floors. Negative checks confirm that the dash and glide practice gaps, a chimney, a containment field and a hatch's bulkhead are impassable without the augment. These are developer checks that reposition the rat between segments, not a continuous human playthrough. See CURRENT_STATE for the latest result.
