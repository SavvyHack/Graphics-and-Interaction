// RatStylizedLighting.shader
// Project R.A.T. — Individual Shader (Prajeet)
// Theme 1 — Lighting and Stylised Shading
//
// Custom diffuse lighting combined with configurable rim lighting, used to make the
// player rat visually pop against the enclosure's cool blue-grey palette.
// Handwritten vertex/fragment Cg shader — no Surface Shader, no Shader Graph.

Shader "Custom/RatStylizedLighting"
{
    Properties
    {
        _BaseColor      ("Base Color", Color)          = (0.85, 0.6, 0.3, 1)
        _RimColor       ("Rim Color", Color)            = (0.3, 0.8, 1.0, 1)
        _RimStrength    ("Rim Strength", Range(0, 3))   = 1.0
        _RimPower       ("Rim Power", Range(0.1, 10))   = 3.0
        _LightStrength  ("Light Strength", Range(0, 3)) = 1.0
        _SpecularColor  ("Specular Color", Color)       = (1, 1, 1, 1)
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.3
        _SpecularPower  ("Specular Power", Range(1, 128)) = 24
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            fixed4 _BaseColor;
            fixed4 _RimColor;
            float  _RimStrength;
            float  _RimPower;
            float  _LightStrength;
            fixed4 _SpecularColor;
            float  _SpecularStrength;
            float  _SpecularPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos          : SV_POSITION;
                float3 worldNormal  : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);

                float NdotL = max(dot(N, L), 0.0);
                float3 diffuse = NdotL * _LightColor0.rgb * _LightStrength;

                float rimFactor = 1.0 - max(dot(N, V), 0.0);
                rimFactor = pow(saturate(rimFactor), _RimPower);
                float3 rim = rimFactor * _RimStrength * _RimColor.rgb;

                float3 halfDir = normalize(L + V);
                float NdotH = max(dot(N, halfDir), 0.0);
                float3 specular = pow(NdotH, _SpecularPower) * _SpecularStrength * _SpecularColor.rgb * _LightColor0.rgb;

                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * _BaseColor.rgb;
                float3 litColor = _BaseColor.rgb * (diffuse + ambient) + rim + specular;

                return fixed4(litColor, _BaseColor.a);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
