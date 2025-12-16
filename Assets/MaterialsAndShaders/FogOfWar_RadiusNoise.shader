Shader "Custom/FogOfWar_RadiusNoiseTex_URP"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0,0,0,0.85)
        _FogTex ("Fog Texture (tile)", 2D) = "white" {}

        _Center   ("Center (World XZ)", Vector) = (0,0,0,0)
        _Radius   ("Visible Radius", Float) = 20
        _Softness ("Edge Softness", Float) = 8

        _TexScale ("Tex Scale", Float) = 0.02
        _TexStrength ("Tex Strength", Float) = 0.6
        _TexSpeed ("Tex Speed", Float) = 0.03

        _EdgeTexBoost ("Edge Tex Boost", Float) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "FogOfWar"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float4 _Center;
                float  _Radius;
                float  _Softness;

                float  _TexScale;
                float  _TexStrength;
                float  _TexSpeed;
                float  _EdgeTexBoost;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs v = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = v.positionCS;
                OUT.positionWS  = v.positionWS;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 p = IN.positionWS.xz;
                float2 c = _Center.xz;

                float dist = distance(p, c);

                // Базовая "маска видимости": 0 внутри, 1 снаружи
                float softness = max(_Softness, 0.0001);
                float edge = smoothstep(_Radius, _Radius + softness, dist);

                // Текстура тумана в world-space, чтобы не "плавала" по объекту
                float t = _Time.y * _TexSpeed;
                float2 uv = p * _TexScale + float2(t, t * 0.7);

                float fogTex = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, uv).r; // 0..1

                // Усиливаем текстуру ближе к границе, чтобы край был "дымный"
                // edge=0 (внутри) => boost почти 0, edge=1 (снаружи) => boost ~1
                float edgeBoost = lerp(0.0, 1.0, saturate(edge * _EdgeTexBoost));

                // Альфа тумана: базовый цвет * (1 + текстура) * edge
                float texMod = lerp(1.0, fogTex, _TexStrength);
                float alpha = _FogColor.a * edge * texMod;

                half4 col = _FogColor;
                col.a = saturate(alpha);

                return col;
            }
            ENDHLSL
        }
    }
}
