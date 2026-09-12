# Kavish - exact integration steps

This repository copy contains Kavish's assigned game systems, five sound effects, and animated hazard work. It also adds reference assets 09-11 as working modular hazards and extends the playable level.

## Files and exact locations

```text
Assets/
|-- Editor/KavishIntegrationSetup.cs
|-- Scripts/Gameplay/
|   |-- GameManager.cs
|   |-- RatLifeManager.cs
|   |-- Checkpoint.cs
|   |-- HazardTrigger.cs
|   |-- ExitTrigger.cs
|   `-- KavishTimedHazard.cs
|-- Scripts/Presentation/AudioManager.cs
|-- Shaders/
|   |-- KavishAnimatedEnergyHazard.shader
|   |-- KavishFirePlume.shader
|   `-- KavishLaserPulse.shader
|-- Materials/Hazards/
|   |-- KavishFireHazard.mat
|   |-- KavishElectricGate.mat
|   `-- KavishLaserBarrier.mat
|-- Prefabs/Hazards/                 generated when the installer runs
|   |-- 09 Fire Emitter.prefab
|   |-- 10 Electric Gate.prefab
|   `-- 11 Laser Barrier.prefab
`-- Audio/
    |-- Kavish_Jump.wav
    |-- Kavish_Checkpoint.wav
    |-- Kavish_RatLost.wav
    |-- Kavish_Completion.wav
    `-- Kavish_Failure.wav

REPORT.md
Documentation/KAVISH_HAZARDS_09_11.md
```

Keep every `.meta` file beside its Unity asset.

## Install everything into the level

1. Open the project with Unity `6000.3.18f1`.
2. Wait for import and compilation to finish. Resolve any red Console errors before continuing.
3. Select **Project R.A.T. > Kavish > Install or Refresh Kavish Systems**.
4. The command opens and saves `Assets/Scenes/RatEnclosure.unity`. It:
   - enables Kavish's `GameManager`, `RatLifeManager`, checkpoint, hazard, exit and audio systems;
   - disables the temporary `RatTrialSession`/`TrialZone` harness;
   - assigns the player, waiting rats, camera, five sound clips and existing zones;
   - applies `KavishEnergyHazard.mat` to the existing runoff;
   - extends the enclosure from approximately X=64 to X=85;
   - moves the old escape hatch and exit trigger to the far end;
   - updates the camera and enclosure bounds;
   - adds three platform sections, two runoff gaps and checkpoint 5;
   - places **09 Fire Emitter**, **10 Electric Gate**, and **11 Laser Barrier** in the new section;
   - creates reusable prefabs under `Assets/Prefabs/Hazards`.
5. Select **Project R.A.T. > Kavish > Validate Kavish Systems**.
6. Confirm the Console reports `KAVISH_VALIDATION_OK`.

Running the installer again is safe. It refreshes the generated hazard course without duplicating it.

## Play test

1. Press Play. Use `A/D` or the arrow keys to move, `Space` to jump, and `Shift` to sprint.
2. Confirm a short sound plays only when a valid jump begins.
3. Confirm the cyan runoff animates.
4. Lose a rat after a checkpoint. Confirm one waiting rat disappears and the active rat respawns at that checkpoint.
5. Continue beyond the former exit deck into section 06.
6. Confirm the fire, electric field and lasers visibly cycle between active and safe states.
7. Touch an active effect. Confirm one rat is lost.
8. Cross during each safe phase; the laser can also be jumped.
9. Reach checkpoint 5 near X=73 and the relocated escape hatch near X=83.
10. Confirm completion stops player movement and plays the completion sound.

## Shader screenshots

The primary individually assessed shader remains `Assets/Shaders/KavishAnimatedEnergyHazard.shader`. It is used by the runoff and by asset 10 in gate mode.

For the two contrasting screenshots, select `Assets/Materials/Hazards/KavishElectricGate.mat`, press Play, and use:

```text
Screenshot A - slow/subtle
Animation Speed = 0.5
Energy Intensity = 1.0
Vertex Wave Height = 0.01

Screenshot B - fast/strong
Animation Speed = 5.0
Energy Intensity = 3.0
Vertex Wave Height = 0.08
```

Save the images as:

```text
Documentation/Images/Kavish/hazard-slow.png
Documentation/Images/Kavish/hazard-fast.png
```

## Files created or changed after installation

Commit these generated scene/prefab changes with the source files:

```text
Assets/Scenes/RatEnclosure.unity
Assets/Prefabs/Hazards/09 Fire Emitter.prefab
Assets/Prefabs/Hazards/10 Electric Gate.prefab
Assets/Prefabs/Hazards/11 Laser Barrier.prefab
Documentation/Images/Kavish/hazard-slow.png
Documentation/Images/Kavish/hazard-fast.png
```

Do not commit `Library/`, `Temp/`, `Logs/` or `UserSettings/`.
