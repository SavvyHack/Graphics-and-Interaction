// Laboratory observation glass, built-in render pipeline.
// One shared scene capture includes water (queue 3000) before the glass (3100).
Shader "ProjectRAT/Tavish/GlassEnclosure"
{
    Properties
    {
        _GlassTint ("Transmission Tint", Color) = (0.88, 0.97, 1.0, 1.0)
        _BaseAlpha ("Tint / Absorption", Range(0.0, 0.8)) = 0.06
        [HDR] _FresnelColor ("Polished Edge Colour", Color) = (0.8, 0.94, 1.0, 1.0)
        _FresnelStrength ("Edge Reflection Strength", Range(0.0, 3.0)) = 0.7
        _FresnelPower ("Fresnel Power", Range(0.5, 8.0)) = 5.0
        [HDR] _ShimmerColor ("Laboratory Light Colour", Color) = (0.94, 0.98, 1.0, 1.0)
        _ShimmerStrength ("Laboratory Light Reflection", Range(0.0, 1.5)) = 0.16
        _ShimmerScale ("Light Spacing (World Units)", Range(2.0, 24.0)) = 12.0
        _DistortionStrength ("Edge Refraction", Range(0.0, 0.01)) = 0.001
        _EdgeWidth ("Polished Border Width (UV)", Range(0.0005, 0.02)) = 0.002
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100" }
        GrabPass { "_ObservationScene" }
        Pass
        {
            // Transmission is composed explicitly with the captured scene.
            Blend Off
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ObservationScene;
            float4 _ObservationScene_TexelSize;
            float4 _GlassTint, _FresnelColor, _ShimmerColor;
            float _BaseAlpha, _FresnelStrength, _FresnelPower;
            float _ShimmerStrength, _ShimmerScale, _DistortionStrength, _EdgeWidth;

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
                float4 grabPos : TEXCOORD3;
            };
            v2f vert(appdata input)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.uv = input.uv;
                output.grabPos = ComputeGrabScreenPos(output.pos);
                return output;
            }
            float softStrip(float distance, float width, float feather)
            {
                return 1.0 - smoothstep(width, width + feather, abs(distance));
            }
            fixed4 frag(v2f input) : SV_Target
            {
                float3 normal = normalize(input.worldNormal);
                // Orthographic rays are parallel, not vectors from the camera position.
                float3 perspectiveView = normalize(_WorldSpaceCameraPos - input.worldPos);
                float3 viewDirection = normalize(lerp(perspectiveView,
                    UNITY_MATRIX_V[2].xyz, unity_OrthoParams.w));
                float fresnel = pow(1.0 - saturate(abs(dot(normal, viewDirection))), _FresnelPower);

                float2 edgeDistance = min(input.uv, 1.0 - input.uv);
                float edge = min(edgeDistance.x, edgeDistance.y);
                float aa = max(fwidth(edge), 0.0001);
                float border = 1.0 - smoothstep(_EdgeWidth, _EdgeWidth + aa, edge);
                float bevel = exp2(-edge * 160.0);

                // Stationary reflected ceiling panels with slight camera parallax.
                // World spacing avoids stretching one reflection across a long enclosure.
                float lightCoord = (input.worldPos.x + input.worldPos.y * 0.42
                    - _WorldSpaceCameraPos.x * 0.12) / max(_ShimmerScale, 2.0);
                float lightDistance = frac(lightCoord) - 0.5;
                // Unity's cube front face has V=0 at its top edge.
                float upperFalloff = smoothstep(0.24, 0.9, 1.0 - input.uv.y);
                float reflection = softStrip(lightDistance, 0.035, 0.075) * upperFalloff;
                reflection += softStrip(lightDistance - 0.13, 0.004, 0.009) * upperFalloff * 0.4;

                float2 screenUV = input.grabPos.xy / input.grabPos.w;
                float2 offset = (input.uv - 0.5) * bevel * _DistortionStrength;
                // Clamp to pixel centres so framebuffer edges cannot wrap.
                float2 halfTexel = abs(_ObservationScene_TexelSize.xy) * 0.5;
                float3 scene = tex2D(_ObservationScene, clamp(screenUV + offset, halfTexel, 1.0 - halfTexel)).rgb;
                float3 transmission = scene * lerp(float3(1, 1, 1), _GlassTint.rgb, _BaseAlpha);
                float3 light = _ShimmerColor.rgb * reflection * _ShimmerStrength;
                light += _FresnelColor.rgb * (fresnel + border * 0.32 + bevel * 0.018) * _FresnelStrength;
                // No constant coloured overlay: black stays black away from reflections.
                return fixed4(transmission + light, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
