Shader "ColoringBook/GlitterBrush"
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

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
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
                half4 col = _Color;
                
                // Screen position
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                screenUV *= _ScreenParams.xy;
                
                // Glitter grid - small cells
                float2 glitterID = floor(screenUV / 3.0);
                float glitterRand = rand(glitterID);
                
                // Animated sparkle
                float time = _Time.y * 8.0 + glitterRand * 50.0;
                float sparkle = pow(sin(time) * 0.5 + 0.5, 6.0);
                
                // Only show glitter on some cells (15%)
                float showGlitter = step(0.85, glitterRand);
                
                // Add bright white/yellow sparkles
                half3 sparkleColor = lerp(half3(1, 1, 1), half3(1, 1, 0.7), glitterRand);
                col.rgb += showGlitter * sparkle * sparkleColor * 1.2;
                
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