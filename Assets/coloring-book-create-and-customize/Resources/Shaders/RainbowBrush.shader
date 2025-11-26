Shader "ColoringBook/RainbowBrush"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            half3 hsv2rgb(half3 c)
            {
                half3 rgb = clamp(abs(fmod(c.x * 6.0 + half3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
                return c.z * lerp(half3(1, 1, 1), rgb, c.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Screen position
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                screenUV *= _ScreenParams.xy;
                
                // Rainbow - hue changes with position and time
                float hue = frac((screenUV.x + screenUV.y) * 0.003 + _Time.y * 0.3);
                half3 rainbow = hsv2rgb(half3(hue, 1.0, 1.0));
                
                half4 col;
                col.rgb = rainbow;
                col.a = 1.0;
                
                // Brush shape
                float2 center = IN.uv - 0.5;
                float dist = length(center);
                col.a *= 1.0 - smoothstep(0.0, 0.5, dist);
                
                return col;
            }
            ENDHLSL
        }
    }
}