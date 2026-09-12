# Shader Report

Read the [project specification](https://github.com/feit-comp30019/project-1-specification) for details on what _actually_ needs to be covered here.

## Table of Contents

- [First Shader](#first-shader)
- [Second Shader](#second-shader)
- [Third Shader](#third-shader)
- [Fourth Shader](#fourth-shader)

## First Shader

TODO - see specification for details

## Second Shader

TODO - see specification for details

## Third Shader — Kavish: Animated Energy Hazard

**Shader:** [`Assets/Shaders/KavishAnimatedEnergyHazard.shader`](Assets/Shaders/KavishAnimatedEnergyHazard.shader)  
**Theme:** Animated hazard / energy effect  
**Game use:** Applied to the cyan **Electric runoff** hazard surfaces in `RatEnclosure.unity`.

My shader creates an animated electrical-energy surface for the prototype hazards. I chose this effect because the game design uses a cool blue-grey laboratory palette and reserves brighter cyan, red and orange colours for dangerous or interactive objects. The hazard therefore needs to be immediately readable from the fixed 2.5D camera while still fitting the science-laboratory setting. The effect is fully procedural: it does not depend on an animated texture, which keeps the asset small and makes the behaviour easy to control through shader parameters.

The **vertex shader** demonstrates custom geometry processing. It takes each vertex position and applies a small vertical sine-wave displacement using the vertex's object-space X/Z position, `_Time.y`, `_WaveFrequency` and `_Speed`. `_WaveHeight` controls the displacement amplitude. The movement is intentionally subtle because the electric runoff is still a gameplay boundary and large deformations could make its collision shape visually misleading. The calculated wave is also remapped from -1..1 to 0..1 and passed to the fragment stage as a pulse value.

The **fragment shader** uses the mesh UV coordinates to build moving diagonal energy bands. It combines the U and V coordinates, multiplies them by `_Scale`, subtracts time multiplied by `_Speed`, and uses `frac()` to repeat the pattern. The distance from the centre of each repeated band is converted into a soft stripe with `smoothstep()`. A second slower sine pulse varies the brightness over time, preventing the result from looking like a simple scrolling texture. The final colour is produced by interpolating between `_BaseColor` and `_EnergyColor`, with `_Intensity` controlling the visibility of the energy bands and `_Alpha` controlling overall transparency.

The exposed parameters are meaningful during marking: `_Speed` visibly changes animation rate, `_Scale` changes the density of the electrical pattern, `_StripeWidth` changes band thickness, `_Intensity` changes the brightness/strength of the effect, and `_WaveHeight` / `_WaveFrequency` change vertex deformation. The shader uses a lightweight vertex/fragment Cg/HLSL pass with shader target 2.0, no texture lookups and no Shader Graph, making it suitable for the project's WebGL-focused prototype.

**Screenshots to capture before submission:**
- `Documentation/Images/Kavish/hazard-slow.png`: `_Speed = 0.5`, `_Intensity = 1.0`, `_WaveHeight = 0.01`.
- `Documentation/Images/Kavish/hazard-fast.png`: `_Speed = 5.0`, `_Intensity = 3.0`, `_WaveHeight = 0.08`.

## Fourth Shader

TODO - see specification for details
