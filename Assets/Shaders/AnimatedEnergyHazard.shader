// AnimatedEnergyHazard.shader
// Project R.A.T. — Game Systems individual shader
// Theme: animated electrical / energy hazard
//
// WHAT THIS SHADER DOES :
// It draws a glowing hazard surface that can look like either:
//   - "Runoff" mode: diagonal glowing stripes scrolling across the surface
//   - "Gate" mode: three wiggly electric arc lines
// The whole surface also gently bobs up and down over time.
// No textures are used — every pattern is generated with math (sin waves,
// frac() for repetition, smoothstep() for soft edges).


Shader "ProjectRAT/GameSystems/AnimatedEnergyHazard"
{
    Properties
    {
        // These are the knobs that show up in Unity's Inspector so artists
        // can tweak the look without touching code.
        _BaseColor ("Base Color", Color) = (0.03, 0.22, 0.32, 0.82)      // the "cold/off" color
        _EnergyColor ("Energy Color", Color) = (0.44, 0.91, 1.0, 1.0)    // the "hot/glowing" color
        _Speed ("Animation Speed", Range(0.0, 8.0)) = 2.4                // how fast everything animates
        _Scale ("Pattern Scale", Range(1.0, 30.0)) = 12.0                // how many stripes fit on the surface
        _StripeWidth ("Stripe Width", Range(0.02, 0.48)) = 0.16          // how thick each stripe/arc is
        _Intensity ("Energy Intensity", Range(0.0, 4.0)) = 1.7           // how bright the glow gets
        _WaveHeight ("Vertex Wave Height", Range(0.0, 0.25)) = 0.035     // how far the surface bobs up/down
        _WaveFrequency ("Vertex Wave Frequency", Range(1.0, 20.0)) = 7.0 // how many bobs/bumps across the surface
        _Alpha ("Overall Alpha", Range(0.1, 1.0)) = 0.86                 // overall transparency
        _GateMode ("Effect Mode (0 Runoff, 1 Gate)", Range(0.0, 1.0)) = 0.0   // switch: stripes vs arcs
        _ArcDistortion ("Gate Arc Distortion", Range(0.0, 0.2)) = 0.07   // how much the arcs wiggle
    }

    SubShader
    {
        Tags
        {
            // Tells Unity: this is see-through, render it in the
            // transparent pass (after solid objects, sorted back-to-front).
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            // Standard transparency setup
            Blend SrcAlpha OneMinusSrcAlpha // normal alpha blending with whatever is behind it
            ZWrite Off                      // don't write to depth buffer (avoids see-through sorting glitches)
            Cull Off                        // draw both front and back faces of the surface

            CGPROGRAM
            #pragma vertex vert     // vert() runs once per vertex
            #pragma fragment frag   // frag() runs once per pixel
            #pragma target 2.0
            #include "UnityCG.cginc" // gives us _Time, UnityObjectToClipPos, etc.

            // Mirrors of the Properties above, now available inside the shader code.
            fixed4 _BaseColor;
            fixed4 _EnergyColor;
            float _Speed;
            float _Scale;
            float _StripeWidth;
            float _Intensity;
            float _WaveHeight;
            float _WaveFrequency;
            float _Alpha;
            float _GateMode;
            float _ArcDistortion;

            // What comes IN to the vertex shader per vertex.
            struct appdata
            {
                float4 vertex : POSITION; // vertex position (object space)
                float2 uv : TEXCOORD0;    // UV coordinate on the mesh
            };

            // What comes OUT of the vertex shader (and gets interpolated
            // across each triangle before reaching the fragment shader).
            struct v2f
            {
                float4 pos : SV_POSITION; // final screen position
                float2 uv : TEXCOORD0;    // pass-through UV
                float pulse : TEXCOORD1;  // the bob wave value, so frag() can use it too
            };

            // VERTEX SHADER — moves each vertex a little to make the
            // whole surface gently bob up and down over time.
            v2f vert(appdata v)
            {
                v2f o;

                // A sine wave based on position + time = a smooth bobbing
                // motion that's different at every point on the surface,
                // and changes as time passes. Range is -1 to 1.
                float wave = sin((v.vertex.x + v.vertex.z) * _WaveFrequency + _Time.y * _Speed);

                // Nudge the vertex up/down by a small amount (scaled by
                // _WaveHeight so it's subtle, not a huge bounce).
                v.vertex.y += wave * _WaveHeight;

                o.pos = UnityObjectToClipPos(v.vertex); // convert to screen position
                o.uv = v.uv;                             // just pass the UV along unchanged

                // Remap wave from [-1, 1] to [0, 1] so it's safe to use
                // later as a brightness/blend factor.
                o.pulse = wave * 0.5 + 0.5;
                return o;
            }

 
            // FRAGMENT SHADER — decides the final color of every pixel.
            fixed4 frag(v2f i) : SV_Target
            {
                // RUNOFF MODE: scrolling diagonal stripes

                // Combine U and V so the stripe direction is diagonal,
                // not just horizontal or vertical.
                float diagonal = i.uv.x + i.uv.y * 0.55;

                // frac() keeps only the decimal part of a number, so as
                // time grows this value keeps looping 0 -> 1 -> 0 -> 1...
                // That loop is what makes the stripes look like they're
                // scrolling forever instead of running off screen.
                float travelling = frac(diagonal * _Scale - _Time.y * _Speed);

                // How far is this pixel from the middle of one stripe cycle?
                float distanceToBand = abs(travelling - 0.5);

                // smoothstep gives a soft fade instead of a hard on/off
                // line, so the stripe edges don't look jagged.
                // Inverted (1.0 - ...) so INSIDE the stripe = bright (1),
                // outside the stripe = dark (0).
                float band = 1.0 - smoothstep(_StripeWidth, _StripeWidth + 0.06, distanceToBand);

                // A second, slower sine wave so the glow flickers a bit
                // instead of looking like a flat, static scroll.
                float slowPulse = 0.65 + 0.35 * sin(_Time.y * (_Speed * 1.35) + i.uv.x * 6.28318);
                slowPulse = slowPulse * 0.5 + 0.5; // squash into a safe [0.65, 1.0]-ish range

                // GATE MODE: three wiggly electric arcs

                float gateTime = _Time.y * _Speed;

                // Each arc is a horizontal line at a fixed height
                // (0.22 / 0.50 / 0.78), but I bend that height with a sine
                // wave so the line wiggles left-to-right along its length.
                // Different sine speeds/frequencies per arc so they don't
                // all move in sync (looks more chaotic/electric).
                float arcA = abs(i.uv.y - (0.22 + sin(i.uv.x * 17.0 + gateTime) * _ArcDistortion));
                float arcB = abs(i.uv.y - (0.50 + sin(i.uv.x * 22.0 - gateTime * 1.3) * _ArcDistortion));
                float arcC = abs(i.uv.y - (0.78 + sin(i.uv.x * 14.0 + gateTime * 1.7) * _ArcDistortion));

                // Distance to whichever arc is closest to this pixel.
                float nearestArc = min(arcA, min(arcB, arcC));

                // Two falloffs: a tight bright "core" line, and a wider
                // soft "glow" around it. Both use smoothstep for softness.
                float gateCore = 1.0 - smoothstep(_StripeWidth * 0.20, _StripeWidth, nearestArc);
                float gateGlow = 1.0 - smoothstep(_StripeWidth, _StripeWidth * 3.2, nearestArc);

                // Combine core + glow (glow weighted lower so it's dimmer),
                // scale by intensity and flicker, then clamp to [0,1].
                float gateEnergy = saturate((gateCore + gateGlow * 0.42) * _Intensity * slowPulse);

                // Combine runoff brightness the same way
                float runoffEnergy = saturate(band * _Intensity * (0.65 + 0.35 * i.pulse) * slowPulse);

                // Pick ONE mode: Runoff or Gate
                // step(0.5, _GateMode) turns the 0-1 slider into a clean
                // switch: 0 if GateMode < 0.5, else 1.
                // lerp(a, b, t) blends between a and b using t — but since
                // t is always exactly 0 or 1 here, this just SELECTS
                // whichever mode is active (not an actual blend).
                float energy = lerp(runoffEnergy, gateEnergy, step(0.5, _GateMode));

                // Turn "how energetic" into an actual color
                // High energy -> lean toward EnergyColor.
                // Low energy -> stay near BaseColor.
                fixed3 colour = lerp(_BaseColor.rgb, _EnergyColor.rgb, energy);

                // Work out transparency, same mode-switch trick
                float gateAlpha = saturate(0.10 + gateGlow * 0.55 + gateCore * 0.35);
                float runoffAlpha = saturate(_BaseColor.a * _Alpha + energy * 0.18);
                float alpha = lerp(runoffAlpha, gateAlpha * _Alpha, step(0.5, _GateMode));

                return fixed4(colour, alpha); // final pixel color + transparency
            }
            ENDCG
        }
    }

    Fallback Off // don't substitute another shader if this one fails to compile
}
