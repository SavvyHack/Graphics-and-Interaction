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
        [HDR] _ShimmerColor ("Light Bar Colour", Color) = (1.0, 0.98, 0.91, 1.0)
        _ShimmerStrength ("Light Bar Strength", Range(0.0, 1.5)) = 0.32
        _ShimmerScale ("Light Spacing (World Units)", Range(2.0, 24.0)) = 12.0
        _BarWidth ("Main Bar Half Width (World Units)", Range(0.05, 1.5)) = 0.48
        _BarFeather ("Bar Edge Softness (World Units)", Range(0.01, 0.5)) = 0.06
        _BarSlant ("Bar Diagonal Slant", Range(-1.5, 1.5)) = -0.55
        _RatParallax ("Active Rat Reflection Movement", Range(0.0, 1.0)) = 0.55
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
            float _BarWidth, _BarFeather, _BarSlant, _RatParallax;
            // Global, supplied by FixedCameraFollow; W is 1 while a target is available.
            // Keep out of Properties so a material cannot override the active rat.
            float4 _GlassActiveRatPosition;

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

                // Cartoon sunlight: a wide slash paired with a fine parallel highlight.
                // Drive parallax from the rat itself, even while the camera is clamped.
                // No time scroll: stopping the rat also stops the reflected bars.
                float ratCoord = (_GlassActiveRatPosition.x
                    + _GlassActiveRatPosition.y * _BarSlant) * _GlassActiveRatPosition.w;
                float spacing = max(_ShimmerScale, 2.0);
                float lightCoord = input.worldPos.x + input.worldPos.y * _BarSlant
                    - ratCoord * _RatParallax;
                float lightDistance = (frac(lightCoord / spacing + 0.5) - 0.5) * spacing;
                float thinDistance = (frac((lightCoord - _BarWidth - 0.6) / spacing + 0.5) - 0.5) * spacing;
                float feather = max(_BarFeather, fwidth(lightCoord));
                // Unity's cube front face has V=0 at its top edge.
                float upperFalloff = lerp(0.3, 1.0, smoothstep(0.08, 0.8, 1.0 - input.uv.y));
                float reflection = softStrip(lightDistance, _BarWidth, feather);
                reflection += softStrip(thinDistance, _BarWidth * 0.22, feather) * 0.65;
                reflection *= upperFalloff * smoothstep(0.0, 0.025, edge);

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
