Shader "PocketPlanet/PlanetSurface"
{
    Properties
    {
        [NoScaleOffset] _BiomeMap ("Biome Map", 2D) = "black" {}
        [NoScaleOffset] _Palette ("Palette", 2D) = "white" {}
        _BiomeCount ("Biome Count", Float) = 1
        _Mood ("Mood", Range(-1, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PlanetSurface"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 disc : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 disc : TEXCOORD1;
            };

            TEXTURE2D(_BiomeMap);
            TEXTURE2D(_Palette);
            SAMPLER(sampler_PointClamp);

            CBUFFER_START(UnityPerMaterial)
                float _BiomeCount;
                float _Mood;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.disc = input.disc;
                return output;
            }

            half4 PaletteColor(float u, float row)
            {
                return SAMPLE_TEXTURE2D(_Palette, sampler_PointClamp, float2(u, (row + 0.5) / 3.0));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float distanceFromCenter = length(input.disc);
                float edge = max(fwidth(distanceFromCenter), 1e-4);
                float alpha = 1.0 - smoothstep(1.0 - edge, 1.0, distanceFromCenter);
                clip(alpha - 0.001);

                float biomeIndex = round(SAMPLE_TEXTURE2D(_BiomeMap, sampler_PointClamp, input.uv).r * 255.0);
                clip(254.5 - biomeIndex);

                float u = (biomeIndex + 0.5) / _BiomeCount;
                half4 doom = PaletteColor(u, 0);
                half4 normal = PaletteColor(u, 1);
                half4 hallowed = PaletteColor(u, 2);
                half4 color = _Mood < 0 ? lerp(normal, doom, -_Mood) : lerp(normal, hallowed, _Mood);
                return half4(color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
