Shader "WaterSurface"
{
    Properties 
    {
        // Water Colours
        _DeepColour ("Deep Colour", Color) = (0.16, 0.22, 0.28, 0.9)
        _ShallowColour ("Shallow Colour", Color) = (0.43, 0.91, 1.0, 0.7)
        _FoamColour ("Foam Colour", Color) = (0.98, 0.98, 0.95, 1.0)

        // Wave Properties
        _WaveHeight ("Wave Height", Range(0.0, 0.5)) = 0.08
        _WaveFrequency ("Wave Frequency", Range(0.5, 10.0)) = 3.0
        _WaveSpeed ("Wave Speed", Range(0.0, 5.0)) = 1.8

        _FoamThreshold ("Foam Threshold", Range(0.5, 1.0)) = 0.8
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            uniform float4 _DeepColour;
            uniform float4 _ShallowColour;
            uniform float4 _FoamColour;
            uniform float _WaveHeight;
            uniform float _WaveFrequency;
            uniform float _WaveSpeed;
            uniform float _FoamThreshold;

            struct vertIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float heightFactor : TEXCOORD1;
            };

            // Vertex Shader
            vertOut vert(vertIn v)
            {
                vertOut o;

                // Use three waves travelling in different directions across x-z plane
                // Project the coordinate onto direction vector using dot product
                // Evaluate the sin 
                // Displace the y value by the weighted sum

                float2 dir1 = normalize(float2(1.0, 0.35));
                float wave1 = sin(dot(v.vertex.xz, dir1) * _WaveFrequency + _Time.y * _WaveSpeed);  

                float2 dir2 = normalize(float2(-0.4, 1.0));
                float wave2 = sin(dot(v.vertex.xz, dir2) * (_WaveFrequency * 1.7) + _Time.y * (_WaveSpeed * 1.3) + 1.7);

                float2 dir3 = normalize(float2(0.65, -0.75));
                float wave3 = sin(dot(v.vertex.xz, dir3) * (_WaveFrequency * 0.45) + _Time.y * (_WaveSpeed * 0.6) + 4.2);

                // Weight the layers so the big slow wave dominates the shape
                // The smaller waves just add texture on top, rather than all fighting equally.
                float combinedWave = wave1 * 0.5 + wave2 * 0.3 + wave3 * 0.2;
                float displacement = combinedWave * _WaveHeight;

                // Displace vertex along y-axis
                v.vertex.y += displacement;

                // Transform vertex from object space into clip space
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // Pass the wave factor (-1 : 1 scaled to 0 : 1) to the fragment shader
                o.heightFactor = combinedWave * 0.5 + 0.5;

                return o;
            }

            // Fragment Shader
            fixed4 frag(vertOut v) : SV_Target 
            {
                // Blend between deep colour and shallow colour
                fixed4 finalColour = lerp(_DeepColour, _ShallowColour, v.heightFactor);
                
                float foamAmount = smoothstep(_FoamThreshold, 1.0, v.heightFactor);
                finalColour.rgb = lerp(finalColour.rgb, _FoamColour.rgb, foamAmount * _FoamColour.a);

                return finalColour;
            }
            ENDCG
        }
    }
}