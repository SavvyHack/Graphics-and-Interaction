## Tavish — Enclosure Glass Shader

### Shader Source
[`Assets/Shaders/Tavish/GlassEnclosure.shader`](../Assets/Shaders/Tavish/GlassEnclosure.shader)

### Primary Theme
**Theme 2 — Surface Appearance and Procedural Effects**

### Effect and Purpose
My shader creates the transparent sci-fi glass used on the front wall of the enclosure in *Project R.A.T.*. The game is presented from a fixed camera outside a glass habitat, so the glass needs to be visible enough to reinforce the idea that the player is observing the rats through an enclosure without obscuring the platforming area. The shader therefore combines alpha transparency with a Fresnel-style edge highlight and a subtle animated procedural shimmer. This supports the blue-grey science-laboratory visual direction in the GDD while keeping the effect lightweight enough for the WebGL prototype.

### Implementation
The vertex stage transforms each vertex from object space into homogeneous clip space so Unity can render it, and also passes the world-space position, world-space normal and UV coordinates to the fragment stage. The fragment stage first calculates a normalised direction from the surface toward the camera. It then takes the dot product between this view direction and the surface normal. When the surface faces the camera directly the value is high, while it decreases at glancing angles. I invert this relationship and raise it to the configurable Fresnel power, producing a stronger highlight around the glass edges.

The second part of the fragment shader uses the mesh UV coordinates together with Unity's time value. Sine and cosine functions create a small animated UV offset, and a second sine function converts this into a moving procedural shimmer. Cubing the resulting 0–1 value narrows the highlight so it appears as a subtle moving reflection rather than an evenly flashing surface. The final RGB colour combines the base glass tint, Fresnel contribution and shimmer contribution. Alpha is calculated from the base transparency plus small Fresnel and shimmer contributions, then standard alpha blending combines the glass with the scene behind it.

### Exposed Parameters
The material exposes **Glass Tint** and **Base Alpha** for the overall colour and transparency; **Fresnel Colour**, **Fresnel Strength** and **Fresnel Power** for the edge response; and **Shimmer Colour**, **Shimmer Strength**, **Shimmer Scale**, **Shimmer Speed** and **UV Distortion Strength** for the procedural animation. These values allow the effect to be demonstrated interactively during marking without modifying the shader source.

### Screenshots
Add screenshots from the same camera angle so the differences are clear:

1. `Documentation/Images/tavish-glass-subtle.png` — low Fresnel and low shimmer.
2. `Documentation/Images/tavish-glass-fresnel.png` — increased Fresnel strength.
3. `Documentation/Images/tavish-glass-shimmer.png` — increased shimmer strength/speed.

### External Resources Consulted
- Unity Manual — Writing custom shaders in URP and `Core.hlsl`.
- Unity Manual — URP normal transformation with `TransformObjectToWorldNormal`.
- Unity Manual — accessing the camera position in a custom URP shader with `GetCameraPositionWS`.
