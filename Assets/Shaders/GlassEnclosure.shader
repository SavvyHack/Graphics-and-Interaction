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
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _GlassTint;
            float _BaseAlpha;
            fixed4 _FresnelColor;
            float _FresnelStrength;
            float _FresnelPower;
            fixed4 _ShimmerColor;
            float _ShimmerStrength;
            float _ShimmerScale;
            float _ShimmerSpeed;
            float _DistortionStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float3 normal = normalize(input.worldNormal);
                float3 viewDirection = normalize(_WorldSpaceCameraPos - input.worldPos);
                float alignment = saturate(abs(dot(normal, viewDirection)));
                float fresnel = pow(1.0 - alignment, _FresnelPower) * _FresnelStrength;

                float timeValue = _Time.y * _ShimmerSpeed;
                float2 distortion;
                distortion.x = sin(input.uv.y * _ShimmerScale * 6.2831853 + timeValue);
                distortion.y = cos(input.uv.x * _ShimmerScale * 6.2831853 - timeValue * 0.85);
                float2 animatedUV = input.uv + distortion * _DistortionStrength;
                float shimmerWave = sin((animatedUV.x + animatedUV.y) * _ShimmerScale * 6.2831853 + timeValue);
                float shimmer01 = shimmerWave * 0.5 + 0.5;
                float shimmer = shimmer01 * shimmer01 * shimmer01 * _ShimmerStrength;

                float3 colour = _GlassTint.rgb;
                colour += _FresnelColor.rgb * fresnel;
                colour += _ShimmerColor.rgb * shimmer;
                float alpha = saturate(_BaseAlpha + fresnel * 0.20 + shimmer * 0.08);
                return fixed4(colour, alpha);
            }
            ENDCG
        }
    }
}
