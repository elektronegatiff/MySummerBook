Shader "ColoringBook/CrayonBrush"
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

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = rand(i);
                float b = rand(i + float2(1.0, 0.0));
                float c = rand(i + float2(0.0, 1.0));
                float d = rand(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
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
                
                // Crayon texture - streaky, uneven
                float n1 = noise(screenUV * 0.2);
                float n2 = noise(screenUV * 0.5 + 100.0);
                
                // Vary brightness like real crayon
                col.rgb *= 0.75 + n1 * 0.35;
                
                // Paper showing through - some areas more transparent
                col.a *= 0.8 + n2 * 0.2;
                
                // Brush shape with rough edges
                float2 center = IN.uv - 0.5;
                float dist = length(center);
                float edgeNoise = noise(IN.uv * 25.0) * 0.12;
                col.a *= 1.0 - smoothstep(0.0, 0.4 + edgeNoise, dist);
                
                return col;
            }
            ENDHLSL
        }
    }
}