// KavishLaserPulse.shader
// Pulsing scan-line beam used by modular hazard 11.
Shader "ProjectRAT/Kavish/LaserPulse"
{
    Properties
    {
        _BeamColor ("Beam Color", Color) = (1.0, 0.01, 0.02, 1.0)
        _CoreColor ("Core Color", Color) = (1.0, 0.86, 0.72, 1.0)
        _Speed ("Pulse Speed", Range(0.0, 12.0)) = 5.0
        _Scale ("Scan Density", Range(1.0, 40.0)) = 16.0
        _BeamWidth ("Core Width", Range(0.02, 0.48)) = 0.18
        _Intensity ("Brightness", Range(0.0, 5.0)) = 3.0
        _Jitter ("Beam Jitter", Range(0.0, 0.2)) = 0.035
        _Alpha ("Overall Alpha", Range(0.0, 1.0)) = 0.95
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            fixed4 _BeamColor;
            fixed4 _CoreColor;
            float _Speed;
            float _Scale;
            float _BeamWidth;
            float _Intensity;
            float _Jitter;
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
                float jitter = sin(v.uv.x * _Scale + _Time.y * _Speed * 2.0) * _Jitter;
                v.vertex.y += jitter;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float centre = 0.5 + sin(i.uv.x * _Scale + _Time.y * _Speed) * _Jitter;
                float distanceFromBeam = abs(i.uv.y - centre);
                float glow = 1.0 - smoothstep(_BeamWidth, 0.5, distanceFromBeam);
                float core = 1.0 - smoothstep(_BeamWidth * 0.18, _BeamWidth, distanceFromBeam);
                float travellingScan = 0.72 + 0.28 * sin(i.uv.x * _Scale * 2.0 - _Time.y * _Speed * 3.0);
                float pulse = 0.72 + 0.28 * sin(_Time.y * _Speed * 1.7);
                float strength = saturate(glow * travellingScan * pulse);
                fixed3 colour = lerp(_BeamColor.rgb, _CoreColor.rgb, core) * _Intensity * strength;
                float alpha = saturate(glow * _Alpha * (0.7 + core * 0.3));
                return fixed4(colour, alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}
