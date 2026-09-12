# Kavish hazards 09-11

Run **Project R.A.T. > Kavish > Install or Refresh Kavish Systems** once after Unity finishes compiling. The command creates the prefabs, places all three hazards in `Assets/Scenes/RatEnclosure.unity`, extends the enclosure, relocates the exit and saves the scene.

## Asset locations

| Reference | Scene hierarchy object | Reusable prefab | Material | Shader |
|---|---|---|---|---|
| 09 Fire Emitter | `07 - Kavish Hazard Course (Assets 09-11)/09 - Fire Emitter` | `Assets/Prefabs/Hazards/09 Fire Emitter.prefab` | `Assets/Materials/Hazards/KavishFireHazard.mat` | `Assets/Shaders/KavishFirePlume.shader` |
| 10 Electric Gate | `07 - Kavish Hazard Course (Assets 09-11)/10 - Electric Gate` | `Assets/Prefabs/Hazards/10 Electric Gate.prefab` | `Assets/Materials/Hazards/KavishElectricGate.mat` | `Assets/Shaders/KavishAnimatedEnergyHazard.shader` |
| 11 Laser Barrier | `07 - Kavish Hazard Course (Assets 09-11)/11 - Laser Barrier` | `Assets/Prefabs/Hazards/11 Laser Barrier.prefab` | `Assets/Materials/Hazards/KavishLaserBarrier.mat` | `Assets/Shaders/KavishLaserPulse.shader` |

## Placement and behaviour

- Fire emitter: extended test deck near X=68. Its procedural fire plume faces left and cycles for 1.35 seconds on, 1.25 seconds off.
- Electric gate: gate landing near X=75. Three procedural cyan arcs cycle for 1.55 seconds on, 1.20 seconds off.
- Laser barrier: final escape deck near X=81. Four red pulsing beams cycle for 1.80 seconds on, 0.85 seconds off. Wait for the safe phase or jump it.
- Checkpoint 5 is at X=73. The relocated escape trigger and hatch are near X=83.
- Each active effect contains a trigger with `HazardTrigger`. Contact calls `RatLifeManager.OnRatDied()` and respawns the next rat at the most recent checkpoint.
- `KavishTimedHazard` enables and disables each visual and its trigger together. An invisible effect cannot kill the player.

## Inspect shader changes live

Press Play, select a material under `Assets/Materials/Hazards`, and change its exposed values in the Inspector. The Game view updates immediately.

- Fire: Flame Speed, Flame Detail, Flame Distortion, Brightness.
- Electric gate: Animation Speed, Energy Intensity, Stripe Width, Gate Arc Distortion.
- Laser: Pulse Speed, Scan Density, Core Width, Beam Jitter, Brightness.

The original `KavishEnergyHazard.mat` remains on the cyan runoff surfaces. The electric-gate material uses the same shader in gate mode, which renders three moving arcs instead of runoff bands.

## Jump audio

The clip is `Assets/Audio/Kavish_Jump.wav`. `KavishIntegrationSetup.ConfigureAudio()` assigns it to `AudioManager.jumpClip`. `PlayerRatController.HandleJump()` calls `AudioManager.Instance.PlayJump()` only after confirming that the rat is grounded or within the coyote-time window.
