// RatStylizedLighting.shader
// Project R.A.T. — Individual Shader (Prajeet)
// Theme 1 — Lighting and Stylised Shading
//
// Custom stylised lighting shader for the player rat.
// Features:
// - Quantised toon/diffuse lighting
// - Configurable shadow colour
// - View-dependent rim lighting
// - Configurable rim threshold
// - Blinn-Phong specular highlights
//

Shader "Custom/RatStylizedLighting"
{
    Properties
    {
        // Main rat colour
        _BaseColor ("Base Color", Color) = (0.85, 0.6, 0.3, 1)
        // Codex-assisted cosmetic extension: optional scrolling rainbow stripes.
        _RainbowBlend ("Rainbow Stripes", Range(0, 1)) = 0
        _RainbowPhase ("Rainbow Scroll Phase", Float) = 0
        _LuxuryFinish ("Luxury Finish (0 Off, 1 Gold, 2 Diamond)", Float) = 0

        // Colour used in darker/toon-shaded areas
        _ShadowColor ("Shadow Color", Color) = (0.25, 0.3, 0.4, 1)

        // Number of toon bands
        _ToonSteps ("Toon Steps", Range(1, 6)) = 1

        // Overall lighting intensity
        _LightStrength ("Light Strength", Range(0, 3)) = 1.0


        // Rim Lighting

        _RimColor ("Rim Color", Color) = (0.3, 0.8, 1.0, 1)

        _RimStrength ("Rim Strength", Range(0, 3)) = 1.0

        _RimPower ("Rim Power", Range(0.1, 10)) = 3.0

        // Controls where rim starts appearing
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.35


        // Specular Lighting

        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)

        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.3

        _SpecularPower ("Specular Power", Range(1, 128)) = 24
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }

        LOD 200

        Pass
        {
            Tags
            {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"


            // Properties

            fixed4 _BaseColor;
            float _RainbowBlend;
            float _RainbowPhase;
            float _LuxuryFinish;
            float4x4 _RainbowWorldToRat;
            fixed4 _ShadowColor;

            float _ToonSteps;
            float _LightStrength;

            fixed4 _RimColor;
            float _RimStrength;
            float _RimPower;
            float _RimThreshold;

            fixed4 _SpecularColor;
            float _SpecularStrength;
            float _SpecularPower;


            // Vertex Input

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            // Vertex to Fragment Data

            struct v2f {
                float4 pos : SV_POSITION;

                float3 worldNormal : TEXCOORD0;

                float3 worldPos : TEXCOORD1;
            };


            // Vertex Shader

            v2f vert(appdata v) 
            {
                v2f o;

                // Transform vertex into clip space
                o.pos = UnityObjectToClipPos(v.vertex);

                // Transform normal into world space
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                // Calculate world-space position
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }


            // Fragment Shader

            fixed4 frag(v2f i) : SV_Target
            {
                // Normalisation

                float3 N = normalize(i.worldNormal);

                // Direction toward the main directional light
                float3 L = normalize(_WorldSpaceLightPos0.xyz);

                // Direction toward the camera
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);


                // Diffuse Lighting
                float NdotL = max(dot(N, L), 0.0);


                // Toon

                float toonLight = floor(NdotL * _ToonSteps) / _ToonSteps;

                toonLight = saturate(toonLight);


                // Shadow to Base Colour

                // Blend shadow colour and rats base colour.

                // A shared rat-local frame aligns the stripes across head and body,
                // independent of each mesh's scale, pose and preview world position.
                float3 ratPosition = mul(_RainbowWorldToRat, float4(i.worldPos, 1)).xyz;
                float stripe = frac((ratPosition.z + ratPosition.y * 0.35) * 2.0 - _RainbowPhase);
                float hue = floor(stripe * 7.0) / 7.0;
                float3 rainbow = saturate(abs(frac(hue + float3(0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0) - 1.0);
                float3 baseColour = lerp(_BaseColor.rgb, rainbow, _RainbowBlend);
                float3 shadowColour = lerp(_ShadowColor.rgb, rainbow * 0.4, _RainbowBlend);

                float3 litColour = baseColour;

                float3 toonColour = lerp(
                    shadowColour,
                    litColour,
                    toonLight
                );


                // Apply main light colour and strength.

                float3 diffuse = toonColour
                    * _LightColor0.rgb
                    * _LightStrength;


                // Ambient Lighting

                float3 ambient =
                    UNITY_LIGHTMODEL_AMBIENT.rgb
                    * baseColour;


                // Rim Lighting

                // Calculate how perpendicular the surface is
                // relative to the camera.

                float viewDot = max(dot(N, V), 0.0);

                float rimFactor = 1.0 - viewDot;


                // Control the sharpness of the rim.

                rimFactor = pow(
                    saturate(rimFactor),
                    _RimPower
                );

                rimFactor = smoothstep(
                    _RimThreshold,
                    1.0,
                    rimFactor
                );


                // Calculate final rim colour.

                float3 rim =
                    rimFactor
                    * _RimStrength
                    * _RimColor.rgb;


                // Blinn-Phong Specular Lighting

                float3 halfDir = normalize(L + V);

                float NdotH = max(
                    dot(N, halfDir),
                    0.0
                );


                float3 specular =
                    pow(NdotH, _SpecularPower)
                    * _SpecularStrength
                    * _SpecularColor.rgb
                    * _LightColor0.rgb;


                // Final Colour

                float3 finalColour =
                    diffuse
                    + ambient
                    + rim
                    + specular;

                // Codex-assisted outfit finishes; zero preserves the original shading.
                if (_LuxuryFinish > 0.5 && _LuxuryFinish < 1.5)
                {
                    // Warm metallic body, broad polished reflection and a tight glint.
                    float reflection = pow(saturate(dot(reflect(-V, N), normalize(float3(-0.4, 0.8, -0.5)))), 10.0);
                    float highlight = pow(NdotH, 72.0);
                    float edge = pow(1.0 - saturate(dot(N, V)), 3.0);
                    finalColour = baseColour * (0.3 + 0.5 * NdotL)
                        + float3(1.0, 0.81, 0.36) * reflection * 0.65
                        + float3(1.0, 0.96, 0.75) * highlight * 1.1
                        + float3(1.0, 0.7, 0.18) * edge * 0.3;
                }
                else if (_LuxuryFinish > 1.5)
                {
                    // Triangular cuts share the same body-local frame as the stripes.
                    float2 crystal = float2(ratPosition.z + ratPosition.x * 0.45, ratPosition.y) * 9.0;
                    float2 cell = floor(crystal);
                    float2 facet = frac(crystal);
                    float facetSide = step(facet.x, facet.y);
                    float variation = frac(sin(dot(cell, float2(127.1, 311.7)) + facetSide * 74.7) * 43758.5453);
                    float edgeDistance = min(min(facet.x, 1.0 - facet.x), min(facet.y, 1.0 - facet.y));
                    edgeDistance = min(edgeDistance, abs(facet.x - facet.y) * 0.707);
                    float cutLine = 1.0 - smoothstep(0.015, 0.055, edgeDistance);
                    float3 crystalColour = lerp(float3(0.12, 0.48, 0.78), float3(0.8, 0.98, 1.0), variation);
                    float glint = pow(saturate(NdotH + (variation - 0.5) * 0.12), 96.0);
                    float edge = pow(1.0 - saturate(dot(N, V)), 3.0);
                    finalColour = crystalColour * (0.55 + 0.35 * NdotL)
                        + float3(0.65, 0.9, 1.0) * cutLine * 0.16
                        + float3(0.8, 0.95, 1.0) * edge * 0.35
                        + glint * 0.85;
                }


                // Prevent colour values from exceeding
                // the valid 0–1 range.

                finalColour = saturate(finalColour);


                return fixed4(
                    finalColour,
                    _BaseColor.a
                );
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
