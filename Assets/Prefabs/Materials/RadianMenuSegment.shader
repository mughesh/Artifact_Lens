// RadialMenuSegment.shader
Shader "UI/RadialMenuSegment"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Normal Color", Color) = (0.5,0.5,0.5,1)
        _HighlightColor ("Highlight Color", Color) = (0,0.5,1,1)
        _InnerRadius ("Inner Radius", Range(0,1)) = 0.6
        _EdgeSoftness ("Edge Softness", Range(0,0.5)) = 0.1
        _IsHighlighted ("Is Highlighted", Range(0,1)) = 0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _HighlightColor;
            float _InnerRadius;
            float _EdgeSoftness;
            float _IsHighlighted;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 delta = i.uv - center;
                float dist = length(delta);
                
                // Radial gradient
                float radialGrad = 1 - smoothstep(_InnerRadius - _EdgeSoftness, _InnerRadius + _EdgeSoftness, dist);
                
                // Outer edge softness
                float outerEdge = 1 - smoothstep(0.9 - _EdgeSoftness, 0.9 + _EdgeSoftness, dist);
                
                // Combine colors
                float4 col = lerp(_Color, _HighlightColor, _IsHighlighted);
                
                // Apply gradients
                col.a *= radialGrad * outerEdge;
                
                return col;
            }
            ENDCG
        }
    }
}