Shader "ProjectRAT/Tavish/GlassEnclosure"
{
    Properties
    {
        [HDR] _GlassTint ("Glass Tint", Color) = (0.50, 0.72, 0.80, 1.0)
        _BaseAlpha ("Base Alpha", Range(0.02, 0.80)) = 0.18

        [HDR] _FresnelColor ("Fresnel Colour", Color) = (0.44, 0.91, 1.0, 1.0)
        _FresnelStrength ("Fresnel Strength", Range(0.0, 3.0)) = 1.1
        _FresnelPower ("Fresnel Power", Range(0.5, 8.0)) = 3.0

        [HDR] _ShimmerColor ("Shimmer Colour", Color) = (0.55, 0.95, 1.0, 1.0)
        _ShimmerStrength ("Shimmer Strength", Range(0.0, 1.5)) = 0.20
        _ShimmerScale ("Shimmer Scale", Range(0.25, 12.0)) = 3.0
        _ShimmerSpeed ("Shimmer Speed", Range(-5.0, 5.0)) = 0.35
        _DistortionStrength ("UV Distortion Strength", Range(0.0, 0.15)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "GlassForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                half3 normalWS     : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _GlassTint;
                half _BaseAlpha;
                half4 _FresnelColor;
                half _FresnelStrength;
                half _FresnelPower;
                half4 _ShimmerColor;
                half _ShimmerStrength;
                half _ShimmerScale;
                half _ShimmerSpeed;
                half _DistortionStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirectionWS = normalize(GetCameraPositionWS() - input.positionWS);

                // abs() makes the Fresnel response stable if the glass mesh normals
                // are flipped while the enclosure is being prototyped.
                half normalViewAlignment = saturate(abs(dot(normalWS, viewDirectionWS)));
                half fresnel = pow(1.0h - normalViewAlignment, _FresnelPower);
                fresnel *= _FresnelStrength;

                float timeValue = _Time.y * _ShimmerSpeed;

                float2 distortion;
                distortion.x = sin(input.uv.y * _ShimmerScale * 6.2831853 + timeValue);
                distortion.y = cos(input.uv.x * _ShimmerScale * 6.2831853 - timeValue * 0.85);
                float2 animatedUV = input.uv + distortion * _DistortionStrength;

                float shimmerWave = sin(
                    (animatedUV.x + animatedUV.y) * _ShimmerScale * 6.2831853
                    + timeValue
                );

                half shimmer01 = (half)(shimmerWave * 0.5 + 0.5);
                // Concentrate the wave into thinner, more glass-like highlights.
                half shimmer = shimmer01 * shimmer01 * shimmer01 * _ShimmerStrength;

                half3 finalColour = _GlassTint.rgb;
                finalColour += _FresnelColor.rgb * fresnel;
                finalColour += _ShimmerColor.rgb * shimmer;

                half alpha = saturate(
                    _BaseAlpha
                    + fresnel * 0.20h
                    + shimmer * 0.08h
                );

                return half4(finalColour, alpha);
            }
            ENDHLSL
        }
    }
}
