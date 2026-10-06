# Fire trap particle effect

## Files
- Prefab: [Assets/Prefabs/Hazards/FireParticles.prefab](../../Assets/Prefabs/Hazards/FireParticles.prefab)
- Material: `Assets/Materials/Hazards/FireParticle.mat` (`Legacy Shaders/Particles/Additive`, Unity's built-in soft `Default-Particle` texture)
- Runtime sync: [FireParticles.cs](../../Assets/Scripts/Presentation/FireParticles.cs)
- Builder: [FireParticleAuthoring.cs](../../Assets/Editor/FireParticleAuthoring.cs), menu **Project RAT > Build Fire Particles**. It can be re-run safely: it rebuilds the prefab (same GUID) and replaces the instance under every `Fire VFX socket`.

## Where it is used
The prefab is placed under each `Fire VFX socket` child of a fire trap's `CampaignHazardCycle.live` object. There are four: Augmentation Lab (Thermal vent, Slow-time vent A, Slow-time vent B) and Reactor Divide (Left thermal vent). It adds a hot glow and flying sparks on top of the existing `FirePlume` shader quad.

## How it stays in sync with the hazard
- **On/off:** the prefab is a child of the hazard's live object, so it is only active while the fire is lethal. It needs no timer of its own. `Play On Awake` and `Prewarm` mean the effect is already full when the trap switches on.
- **Pause:** the particle system runs on scaled time, so `Time.timeScale = 0` freezes it. No code is needed.
- **Slow Time:** this power-up slows machinery through `RatPowerups.WorldScale`, not `Time.timeScale`. `FireParticles.Update` copies that value into `main.simulationSpeed`, so the flames slow down with the trap's cycle.

## Particle attributes and reasoning
The existing shader plume blows left out of a nozzle on the trap's right. The particles add a white-hot glow at the nozzle mouth and sparks that fly along the plume, cool from yellow to red, and drift upwards.

| Module | Setting | Why |
|---|---|---|
| Main | Looping, duration 1s, prewarm | Continuous effect that is already full when the trap turns on |
| Main | Start lifetime 0.5-0.75s, start speed 2.6-3.4 (random) | Particles travel about 1.3-2.5m, roughly the length of the 2m damage box, so what you see matches what hurts |
| Main | Gravity modifier -0.35 | Negative gravity makes hot particles rise as they travel, like real flame |
| Main | Start size 0.45-0.8 (random) | Mix of large glows and small sparks |
| Main | Start colour random between white and pale orange | Small heat variation; kept near white because it multiplies with the lifetime gradient (darker values turned the colours muddy brown) |
| Main | Max particles 60 | Hard cap keeps WebGL cost bounded |
| Emission | 50 per second | About 30 particles alive at once: a dense core without heavy overdraw |
| Shape | Cone, angle 10°, radius 0.25, rotated -90° on Y | Cones emit along local Z; the rotation points it left, out of the nozzle and along the plume |
| Colour over lifetime | Pale yellow → orange → dark red; alpha fades in quickly, then out | Hot core at the nozzle that cools and fades as it travels |
| Size over lifetime | Linear 1 → 0.3 | Sparks shrink as they burn out |
| Noise | Strength 0.35, frequency 1.2, low quality | Random turbulence makes sparks flicker and wander instead of flying in straight lines |
| Renderer | Billboard, no shadows | Billboards always face the side-on camera |
| Material | `Legacy Shaders/Particles/Additive`, built-in soft round texture | Additive blending only brightens, so overlaps glow. Alpha blending was tried and left dark halos against the blue walls |

The instance sits at the nozzle mouth, 1.1m right of and 0.5m above its socket, and 0.3m nearer the camera so it draws over the shader plume.

## Randomness
Lifetime, speed, size and start colour are each "random between two constants", so no two particles look the same. Noise adds smooth random sideways movement over each particle's life. Together these keep the fire from looking like a repeating loop.
