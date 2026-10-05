// Based on SolidColorShader for transparent liquid volume
Shader "WaterBody"
{
    Properties
    {
        _Colour ("Colour", Color) = (0.08, 0.14, 0.22, 0.75) // Dark navy with 75% opacity
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

            uniform float4 _Colour;

            struct vertIn
            {
                float4 vertex : POSITION;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
            };

            vertOut vert(vertIn v)
            {
                vertOut o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag(vertOut v) : SV_Target
            {
                return _Colour;
            }
            ENDCG
        }
    }
}