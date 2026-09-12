// KavishFirePlume.shader
// Procedural, texture-free flame used by modular hazard 09.
Shader "ProjectRAT/Kavish/FirePlume"
{
    Properties
    {
        _OuterColor ("Outer Flame", Color) = (1.0, 0.12, 0.01, 1.0)
        _InnerColor ("Hot Core", Color) = (1.0, 0.9, 0.18, 1.0)
        _Speed ("Flame Speed", Range(0.0, 10.0)) = 4.0
        _Scale ("Flame Detail", Range(1.0, 30.0)) = 12.0
        _Distortion ("Flame Distortion", Range(0.0, 0.35)) = 0.12
        _Intensity ("Brightness", Range(0.0, 4.0)) = 2.2
        _Alpha ("Overall Alpha", Range(0.0, 1.0)) = 0.95
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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

            fixed4 _OuterColor;
            fixed4 _InnerColor;
            float _Speed;
            float _Scale;
            float _Distortion;
            float _Intensity;
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
            };

            v2f vert(appdata v)
            {
                v2f o;
                float tipInfluence = 1.0 - v.uv.x;
                float wave = sin(v.uv.x * _Scale + _Time.y * _Speed);
                v.vertex.y += wave * _Distortion * tipInfluence;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // The emitter is at U=1. The flame narrows toward the U=0 tip.
                float u = saturate(i.uv.x);
                float time = _Time.y * _Speed;
                float largeWave = sin(u * (_Scale * 0.85) + time) * _Distortion;
                float smallWave = sin(u * (_Scale * 1.9) - time * 1.37) * _Distortion * 0.45;
                float centre = 0.5 + (largeWave + smallWave) * (1.0 - u * 0.55);
                float halfWidth = lerp(0.025, 0.43, pow(u, 0.72));
                float distanceFromCentre = abs(i.uv.y - centre);
                float outer = 1.0 - smoothstep(halfWidth, halfWidth + 0.075, distanceFromCentre);
                float core = 1.0 - smoothstep(halfWidth * 0.28, halfWidth * 0.62, distanceFromCentre);

                float flicker = 0.78 + 0.22 * sin(time * 2.1 + u * 21.0);
                float brokenTip = smoothstep(0.0, 0.11 + 0.035 * sin(time * 3.0), u);
                float alpha = saturate(outer * brokenTip * _Alpha * flicker);
                clip(alpha - 0.015);

                fixed3 colour = lerp(_OuterColor.rgb, _InnerColor.rgb, saturate(core + u * 0.18));
                colour *= _Intensity * (0.82 + flicker * 0.18);
                return fixed4(colour, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
