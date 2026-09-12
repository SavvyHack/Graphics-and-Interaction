# Shader Report

Read the [project specification](https://github.com/feit-comp30019/project-1-specification) for details on what _actually_ needs to be covered here.

## Table of Contents

- [First Shader](#first-shader)
- [Second Shader](#second-shader)
- [Third Shader](#third-shader)
- [Fourth Shader](#fourth-shader)

## First Shader

# Individual Shader Report — Rat Stylized Lighting

**Student:** Prajeet
**Shader:** `RatStylizedLighting.shader`
**Theme:** Theme 1 — Lighting and Stylised Shading
**Shader source:** `Assets/Shaders/RatStylizedLighting.shader`

## Overview

For my individual shader, I developed `RatStylizedLighting.shader`, a custom handwritten Cg/HLSL vertex and fragment shader designed for the player rat in Project R.A.T. The shader belongs to **Theme 1: Lighting and Stylised Shading** and combines custom diffuse lighting, quantised toon shading, view-dependent rim lighting, ambient lighting, and Blinn-Phong specular highlights.

The shader was chosen to support the game's clean, stylised visual direction while making the player rat visually distinct from the cool blue-grey enclosure environment. The rim lighting in particular helps separate the rat from surrounding platforms and objects.

## Shader Implementation

The vertex shader transforms each model vertex into clip space using `UnityObjectToClipPos`, while also transforming its normal into world space and calculating its world-space position. These values are passed to the fragment shader through the `v2f` structure.

The fragment shader calculates the lighting using the surface normal (**N**), light direction (**L**) and camera/view direction (**V**). Diffuse lighting is calculated using the dot product between the surface normal and light direction. The result is then quantised into configurable lighting bands using `_ToonSteps`. This produces a cartoon like style to match the art theme.

A configurable `_ShadowColor` is blended with the base rat colour according to the toon lighting value, allowing darker regions to have a deliberate stylised colour rather than simply becoming black.

The shader also calculates a view-dependent rim factor using the relationship between the surface normal and camera direction. `_RimPower` controls the sharpness of the effect, while `_RimThreshold` controls where the rim begins appearing. Finally, Blinn-Phong specular lighting is calculated using the halfway vector between the light and view directions.

## Exposed Parameters and Images

The shader exposes several parameters that can be adjusted directly in Unity, including; Base Color, Shadow Color, Toon Steps, Light Strength, Rim Color, Rim Strength, Rim Power, Rim Threshold, Specular Color, Specular Strength, and Specular Power.

For demonstration, screenshots will show different values for Toon Steps and rim power.
![toon1_rimpwr4](Documentation/Images/Prajeet/toon1_rimpwr4.png)
![toon6_rimpwr10.png](Documentation/Images/Prajeet/toon6_rimpwr10.png)


## Second Shader

TODO - see specification for details

## Third Shader — Kavish: Animated Energy Hazard

**Shader:** [`Assets/Shaders/KavishAnimatedEnergyHazard.shader`](Assets/Shaders/KavishAnimatedEnergyHazard.shader)  
**Theme:** Animated hazard / energy effect  
**Game use:** Applied to the cyan **Electric runoff** surfaces and configured in gate mode on modular asset **10 - Electric Gate** in `RatEnclosure.unity`.

My shader creates an animated electrical-energy surface for the prototype hazards. I chose this effect because the game design uses a cool blue-grey laboratory palette and reserves brighter cyan, red and orange colours for dangerous or interactive objects. The hazard therefore needs to be immediately readable from the fixed 2.5D camera while still fitting the science-laboratory setting. The effect is fully procedural: it does not depend on an animated texture, which keeps the asset small and makes the behaviour easy to control through shader parameters.

The **vertex shader** demonstrates custom geometry processing. It takes each vertex position and applies a small vertical sine-wave displacement using the vertex's object-space X/Z position, `_Time.y`, `_WaveFrequency` and `_Speed`. `_WaveHeight` controls the displacement amplitude. The movement is intentionally subtle because the electric runoff is still a gameplay boundary and large deformations could make its collision shape visually misleading. The calculated wave is also remapped from -1..1 to 0..1 and passed to the fragment stage as a pulse value.

The **fragment shader** uses the mesh UV coordinates to build moving diagonal energy bands. It combines the U and V coordinates, multiplies them by `_Scale`, subtracts time multiplied by `_Speed`, and uses `frac()` to repeat the pattern. The distance from the centre of each repeated band is converted into a soft stripe with `smoothstep()`. A second slower sine pulse varies the brightness over time, preventing the result from looking like a simple scrolling texture. The final colour is produced by interpolating between `_BaseColor` and `_EnergyColor`, with `_Intensity` controlling the visibility of the energy bands and `_Alpha` controlling overall transparency.

The exposed parameters are meaningful during marking: `_Speed` visibly changes animation rate, `_Scale` changes the density of the electrical pattern, `_StripeWidth` changes band thickness, `_Intensity` changes the brightness/strength of the effect, and `_WaveHeight` / `_WaveFrequency` change vertex deformation. The shader uses a lightweight vertex/fragment Cg/HLSL pass with shader target 2.0, no texture lookups and no Shader Graph, making it suitable for the project's WebGL-focused prototype. Its gate mode replaces runoff bands with three independently oscillating arcs while retaining the same UV, time, `smoothstep`, intensity and vertex-stage concepts. Assets 09 and 11 use small supporting handwritten shaders for their distinct flame and laser effects; the animated energy shader remains my primary individually assessed shader.

**Screenshots to capture before submission:**
- `Documentation/Images/Kavish/hazard-slow.png`: `_Speed = 0.5`, `_Intensity = 1.0`, `_WaveHeight = 0.01`.
- `Documentation/Images/Kavish/hazard-fast.png`: `_Speed = 5.0`, `_Intensity = 3.0`, `_WaveHeight = 0.08`.

## Fourth Shader

TODO - see specification for details
