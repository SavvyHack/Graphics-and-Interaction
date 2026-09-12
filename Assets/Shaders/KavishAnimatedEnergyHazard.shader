// KavishAnimatedEnergyHazard.shader
// Project R.A.T. — Kavish individual shader
// Theme: animated electrical / energy hazard
//
// Graphics concepts demonstrated:
// - UV coordinates to position a procedural stripe pattern
// - _Time to animate the pattern without textures
// - sine waves and smoothstep for repeating energy bands
// - vertex displacement to make the hazard surface subtly pulse
// - user parameters for speed, scale, intensity, width and wave height
//
// Written as a vertex/fragment Cg/HLSL shader for the project's built-in
// render pipeline and deliberately kept lightweight for WebGL.

Shader "ProjectRAT/Kavish/AnimatedEnergyHazard"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.03, 0.22, 0.32, 0.82)
        _EnergyColor ("Energy Color", Color) = (0.44, 0.91, 1.0, 1.0)
        _Speed ("Animation Speed", Range(0.0, 8.0)) = 2.4
        _Scale ("Pattern Scale", Range(1.0, 30.0)) = 12.0
        _StripeWidth ("Stripe Width", Range(0.02, 0.48)) = 0.16
        _Intensity ("Energy Intensity", Range(0.0, 4.0)) = 1.7
        _WaveHeight ("Vertex Wave Height", Range(0.0, 0.25)) = 0.035
        _WaveFrequency ("Vertex Wave Frequency", Range(1.0, 20.0)) = 7.0
        _Alpha ("Overall Alpha", Range(0.1, 1.0)) = 0.86
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            fixed4 _BaseColor;
            fixed4 _EnergyColor;
            float _Speed;
            float _Scale;
            float _StripeWidth;
            float _Intensity;
            float _WaveHeight;
            float _WaveFrequency;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float pulse : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // A small procedural vertical displacement proves that custom
                // work happens in the vertex stage as well as the fragment stage.
                float wave = sin((v.vertex.x + v.vertex.z) * _WaveFrequency + _Time.y * _Speed);
                v.vertex.y += wave * _WaveHeight;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.pulse = wave * 0.5 + 0.5;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Move diagonal UV bands over time. frac() makes the pattern repeat.
                float diagonal = i.uv.x + i.uv.y * 0.55;
                float travelling = frac(diagonal * _Scale - _Time.y * _Speed);

                // Distance to the centre of each repeated band, then soften the
                // edge with smoothstep so the line is crisp but not aliased.
                float distanceToBand = abs(travelling - 0.5);
                float band = 1.0 - smoothstep(_StripeWidth, _StripeWidth + 0.06, distanceToBand);

                // A slower second pulse prevents the hazard from looking like a
                // flat scrolling texture and makes intensity visibly time-dependent.
                float slowPulse = 0.65 + 0.35 * sin(_Time.y * (_Speed * 1.35) + i.uv.x * 6.28318);
                slowPulse = slowPulse * 0.5 + 0.5;

                float energy = saturate(band * _Intensity * (0.65 + 0.35 * i.pulse) * slowPulse);
                fixed3 colour = lerp(_BaseColor.rgb, _EnergyColor.rgb, energy);
                float alpha = saturate(_BaseColor.a * _Alpha + energy * 0.18);

                return fixed4(colour, alpha);
            }
            ENDCG
        }
    }

    Fallback Off
}
