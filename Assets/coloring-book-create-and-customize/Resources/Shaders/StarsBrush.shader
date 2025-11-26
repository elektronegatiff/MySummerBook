Shader "ColoringBook/StarsBrush"
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
                float2 worldXY : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            float drawStar(float2 uv)
            {
                uv = (uv - 0.5) * 2.0;
                
                float angle = atan2(uv.y, uv.x);
                float r = length(uv);
                
                float rays = cos(angle * 5.0) * 0.5 + 0.5;
                rays = pow(rays, 3.0);
                
                float starShape = rays * 0.6;
                
                return 1.0 - smoothstep(starShape * 0.8, starShape, r);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.worldXY = IN.positionOS.xy;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = _Color;
                
                // BÜYÜK YILDIZLAR - 15 yerine 3
                float2 scaledPos = IN.worldXY * 3.0;
                
                float2 gridID = floor(scaledPos);
                float2 gridUV = frac(scaledPos);
                
                float cellRand = rand(gridID);
                
                // %40 hücrede yýldýz
                if (cellRand > 0.6)
                {
                    float2 starUV = gridUV;
                    float star = drawStar(starUV);
                    
                    float twinkle = sin(_Time.y * 4.0 + cellRand * 20.0) * 0.3 + 0.7;
                    
                    half3 starColor = half3(1.0, 1.0, 0.6) * twinkle * 1.2;
                    
                    col.rgb = lerp(col.rgb, starColor, star);
                    col.a = max(col.a, star);
                }
                
                float2 center = IN.uv - 0.5;
                float dist = length(center);
                float brush = 1.0 - smoothstep(0.3, 0.5, dist);
                col.a *= brush;
                
                return col;
            }
            ENDHLSL
        }
    }
    
    FallBack "Sprites/Default"
}