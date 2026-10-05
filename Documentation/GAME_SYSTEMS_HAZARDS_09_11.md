> **Historical prototype notes:** See [CURRENT_STATE.md](CURRENT_STATE.md) for the inspected uploaded snapshot and [GAME_DESIGN.md](GAME_DESIGN.md) for the new target. Setup instructions and past verification claims below are not proof of current scene wiring or successful validation.

# Game Systems hazards 09-11

Existing scenes already contain the placed hazards. Normal **Project R.A.T. > Game Systems > Install or Refresh Game Systems** preserves their positions. Use the separate rebuild command only to reset the default course.

## Asset locations

| Reference | Scene hierarchy object | Reusable prefab | Material | Shader |
|---|---|---|---|---|
| 09 Fire Emitter | `07 - Game Systems Hazard Course (Assets 09-11)/09 - Fire Emitter` | `Assets/Prefabs/Hazards/09 Fire Emitter.prefab` | `Assets/Materials/Hazards/FireHazard.mat` | `Assets/Shaders/FirePlume.shader` |
| 10 Electric Gate | `07 - Game Systems Hazard Course (Assets 09-11)/10 - Electric Gate` | `Assets/Prefabs/Hazards/10 Electric Gate.prefab` | `Assets/Materials/Hazards/ElectricGate.mat` | `Assets/Shaders/AnimatedEnergyHazard.shader` |
| 11 Laser Barrier | `07 - Game Systems Hazard Course (Assets 09-11)/11 - Laser Barrier` | `Assets/Prefabs/Hazards/11 Laser Barrier.prefab` | `Assets/Materials/Hazards/LaserBarrier.mat` | `Assets/Shaders/LaserPulse.shader` |

## Placement and behaviour

- Fire emitter: extended test deck near X=68. Its procedural fire plume faces left and cycles for 1.35 seconds on, 1.25 seconds off.
- Electric gate: gate landing near X=75. Three procedural cyan arcs cycle for 1.55 seconds on, 1.20 seconds off.
- Laser barrier: final escape deck near X=81. Four red pulsing beams cycle for 1.80 seconds on, 0.85 seconds off. Wait for the safe phase or jump it.
- Checkpoint 5 is at X=73. The relocated escape trigger and hatch are near X=83.
- Each active effect contains a trigger with `HazardTrigger`. Contact calls `RatLifeManager.OnRatDied()` and respawns the next rat at the most recent checkpoint.
- `TimedHazard` enables and disables each visual and its trigger together. An invisible effect cannot kill the player.

## Inspect shader changes live

Press Play, select a material under `Assets/Materials/Hazards`, and change its exposed values in the Inspector. The Game view updates immediately.

- Fire: Flame Speed, Flame Detail, Flame Distortion, Brightness.
- Electric gate: Animation Speed, Energy Intensity, Stripe Width, Gate Arc Distortion.
- Laser: Pulse Speed, Scan Density, Core Width, Beam Jitter, Brightness.

The original `EnergyHazard.mat` remains on the cyan runoff surfaces. The electric-gate material uses the same shader in gate mode, which renders three moving arcs instead of runoff bands.

## Jump audio

The clip is `Assets/Audio/Jump.wav`. `IntegrationSetup.ConfigureAudio()` assigns it to `AudioManager.jumpClip`. `PlayerRatController.HandleJump()` calls `AudioManager.Instance.PlayJump()` only after confirming that the rat is grounded or within the coyote-time window.
