# Kavish — exact integration steps

This repository copy contains Kavish's assigned work from the group plan:

- three-rat life / death / respawn / checkpoint / win-loss game systems;
- lightweight prototype SFX;
- handwritten animated hazard shader;
- Kavish's shader-report section.

The current team prototype already contains a temporary `RatTrialSession` / `TrialZone` harness. Do not delete it manually. The setup tool disables it and wires Kavish's team-facing components to the existing scene so the change is reversible.

## Files and exact locations

Keep these files at these paths:

```text
Assets/
├── Editor/
│   └── KavishIntegrationSetup.cs
├── Scripts/
│   ├── Gameplay/
│   │   ├── GameManager.cs
│   │   ├── RatLifeManager.cs
│   │   ├── Checkpoint.cs
│   │   ├── HazardTrigger.cs
│   │   └── ExitTrigger.cs
│   └── Presentation/
│       └── AudioManager.cs
├── Shaders/
│   └── KavishAnimatedEnergyHazard.shader
└── Audio/
    ├── Kavish_Checkpoint.wav
    ├── Kavish_RatLost.wav
    ├── Kavish_Completion.wav
    └── Kavish_Failure.wav

REPORT.md
Documentation/Images/Kavish/README.md
```

Keep the corresponding `.meta` files beside each Unity asset. Do not move the scripts to the repository root.

## Install into the existing prototype

1. Close Unity if this project is currently open.
2. Copy the files above into the matching paths in your Git working copy, or use the provided completed repository ZIP instead.
3. Open the project with **Unity 6000.3.18f1**.
4. Wait until Unity finishes importing and there are no red compile errors in the Console.
5. In the top menu choose:

   **Project R.A.T. > Kavish > Install or Refresh Kavish Systems**

6. The tool opens `Assets/Scenes/RatEnclosure.unity` and performs all scene wiring automatically:
   - disables the temporary `RatTrialSession`;
   - adds/enables `GameManager`, `RatLifeManager`, `AudioManager` and an `AudioSource` on `Playtest Session`;
   - assigns `PlayerRat`, `Start`, `Waiting rat 2`, `Waiting rat 3` and the existing fixed camera;
   - converts every existing `TrialZone` hazard to `HazardTrigger`;
   - converts the four existing checkpoint zones to `Checkpoint` and preserves their safe respawn transforms;
   - converts the existing exit zone to `ExitTrigger`;
   - creates `Assets/Materials/Environment/KavishEnergyHazard.mat`;
   - applies the animated hazard material to every object named `Electric runoff`;
   - assigns Kavish's four SFX clips;
   - saves `RatEnclosure.unity`.

7. Run:

   **Project R.A.T. > Kavish > Validate Kavish Systems**

   The Console should print a line beginning with `KAVISH_VALIDATION_OK`.

## Play test

Press **Play** and verify these behaviours in order:

1. `A/D` or arrow keys move the rat; `Space` jumps; `Shift` sprints.
2. The cyan `Electric runoff` surfaces visibly animate.
3. Reach checkpoint 1, then deliberately fall into a hazard.
4. One life is consumed, one waiting rat disappears, and the player respawns at the latest checkpoint.
5. Repeat once: the second waiting rat disappears and the final life respawns at the checkpoint.
6. Lose the third life: the attempt enters the failed state. Press `R` to restart.
7. Reach the exit: the attempt enters the completed state and movement stops.
8. Listen for checkpoint, rat-lost, completion and failure cues.

If you need to compare against the repository's original harness, run:

**Project R.A.T. > Kavish > Restore Original Trial Harness**

Then re-run **Install or Refresh Kavish Systems** to switch back to Kavish's implementation.

## Shader demonstration settings

Select `Assets/Materials/Environment/KavishEnergyHazard.mat` after installation. The useful oral-demonstration parameters are:

| Property | What it proves |
|---|---|
| Animation Speed | `_Time` controls motion speed |
| Pattern Scale | UV coordinates control pattern density |
| Stripe Width | `smoothstep` controls procedural band shape |
| Energy Intensity | fragment-stage energy strength |
| Vertex Wave Height | amount of vertex displacement |
| Vertex Wave Frequency | frequency of geometry deformation |

For the two required contrasting screenshots, use:

```text
Screenshot A — slow/subtle
Speed = 0.5
Intensity = 1.0
Wave Height = 0.01

Screenshot B — fast/strong
Speed = 5.0
Intensity = 3.0
Wave Height = 0.08
```

Save the images as:

```text
Documentation/Images/Kavish/hazard-slow.png
Documentation/Images/Kavish/hazard-fast.png
```

Kavish's ~300–500 word shader explanation is already inserted under **Third Shader — Kavish: Animated Energy Hazard** in `REPORT.md`. Add the two screenshots there before submission.

## Files to commit after running the installer

In addition to the source files above, the Unity installer changes/creates these assets. Commit them too:

```text
Assets/Scenes/RatEnclosure.unity
Assets/Materials/Environment/KavishEnergyHazard.mat
Assets/Materials/Environment/KavishEnergyHazard.mat.meta
Documentation/Images/Kavish/hazard-slow.png        # after you capture it
Documentation/Images/Kavish/hazard-slow.png.meta   # generated by Unity
Documentation/Images/Kavish/hazard-fast.png        # after you capture it
Documentation/Images/Kavish/hazard-fast.png.meta   # generated by Unity
```

Do not commit Unity-generated `Library/`, `Temp/`, `Logs/` or `UserSettings/` directories.
