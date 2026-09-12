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
// Handwritten vertex/fragment Cg shader.

Shader "Custom/RatStylizedLighting"
{
    Properties
    {
        // Main rat colour
        _BaseColor ("Base Color", Color) = (0.85, 0.6, 0.3, 1)

        // Colour used in darker/toon-shaded areas
        _ShadowColor ("Shadow Color", Color) = (0.25, 0.3, 0.4, 1)

        // Number of toon lighting bands
        _ToonSteps ("Toon Steps", Range(1, 6)) = 1

        // Overall lighting intensity
        _LightStrength ("Light Strength", Range(0, 3)) = 1.0


        // -----------------------------
        // Rim Lighting
        // -----------------------------

        _RimColor ("Rim Color", Color) = (0.3, 0.8, 1.0, 1)

        _RimStrength ("Rim Strength", Range(0, 3)) = 1.0

        _RimPower ("Rim Power", Range(0.1, 10)) = 3.0

        // Controls where the rim starts appearing
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.35


        // -----------------------------
        // Specular Lighting
        // -----------------------------

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


            // ============================================
            // Properties
            // ============================================

            fixed4 _BaseColor;
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


            // ============================================
            // Vertex Input
            // ============================================

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };


            // ============================================
            // Vertex → Fragment Data
            // ============================================

            struct v2f
            {
                float4 pos : SV_POSITION;

                float3 worldNormal : TEXCOORD0;

                float3 worldPos : TEXCOORD1;
            };


            // ============================================
            // Vertex Shader
            // ============================================

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


            // ============================================
            // Fragment Shader
            // ============================================

            fixed4 frag(v2f i) : SV_Target
            {
                // --------------------------------------------
                // Normalisation
                // --------------------------------------------

                float3 N = normalize(i.worldNormal);

                // Direction toward the main directional light
                float3 L = normalize(_WorldSpaceLightPos0.xyz);

                // Direction toward the camera
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);


                // --------------------------------------------
                // Diffuse Lighting
                // --------------------------------------------

                float NdotL = max(dot(N, L), 0.0);


                // --------------------------------------------
                // Toon
                // --------------------------------------------

                float toonLight = floor(NdotL * _ToonSteps) / _ToonSteps;

                toonLight = saturate(toonLight);


                // --------------------------------------------
                // Shadow → Base Colour
                // --------------------------------------------

                // Blend between the custom shadow colour
                // and the rat's base colour.

                float3 shadowColour = _ShadowColor.rgb;

                float3 litColour = _BaseColor.rgb;

                float3 toonColour = lerp(
                    shadowColour,
                    litColour,
                    toonLight
                );


                // Apply main light colour and strength.

                float3 diffuse = toonColour
                    * _LightColor0.rgb
                    * _LightStrength;


                // --------------------------------------------
                // Ambient Lighting
                // --------------------------------------------

                float3 ambient =
                    UNITY_LIGHTMODEL_AMBIENT.rgb
                    * _BaseColor.rgb;


                // --------------------------------------------
                // Rim Lighting
                // --------------------------------------------

                // Calculate how perpendicular the surface is
                // relative to the camera.

                float viewDot = max(dot(N, V), 0.0);

                float rimFactor = 1.0 - viewDot;


                // Control the sharpness of the rim.

                rimFactor = pow(
                    saturate(rimFactor),
                    _RimPower
                );


                // Apply threshold so the rim only appears
                // strongly around the outer edge.

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


                // --------------------------------------------
                // Blinn-Phong Specular Lighting
                // --------------------------------------------

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


                // --------------------------------------------
                // Final Colour
                // --------------------------------------------

                float3 finalColour =
                    diffuse
                    + ambient
                    + rim
                    + specular;


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