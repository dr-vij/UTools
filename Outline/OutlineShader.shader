Shader "Hidden/OutlineShader"
{
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _OutlineColor = float4(1, 1, 1, 1);
        float _Intensity = 1;
        float2 _BlitTexture_TexelSize; // = float2(0.001953125, 0.001953125);
        int _Width = 5;

        float _GaussSamples[32];
        TEXTURE2D_X(_InitialImage);
        SAMPLER(sampler_InitialImage);

        TEXTURE2D_X(_BackgroundTexture);
        SAMPLER(sampler_BackgroundTexture);

        SAMPLER(sampler_BlitTexture);

        TEXTURE2D_X(_MaskTexture);
        SAMPLER(sampler_MaskTexture);

        float ExpandPlus(float2 uv, float2 offset, int k)
        {
            return step(0.001, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + k * offset).r);
        }

        float ExpandMinus(float2 uv, float2 offset, int k)
        {
            return step(0.001, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - k * offset).r);
        }

        float Expand(float2 uv, float2 offset)
        {
            float intensity = 0;
            // Accumulates horizontal or vertical blur intensity for the specified texture position.
            // Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
            //
            // NOTE: Unroll directive is needed to make the method function on platforms like WebGL 1.0 where loops are not supported.
            // If maximum outline width is changed here, it should be changed in OutlineResources.MaxWidth as well.
            //
            [unroll(32)]
            for (int k = 1; k <= _Width; ++k)
            {
                intensity = step(0.001, ExpandPlus(uv, offset, k));
                intensity = max(step(0.001, ExpandMinus(uv, offset, k)), intensity);
            }

            intensity = max(ExpandPlus(uv, offset, 0), intensity);
            return intensity;
        }

        float CalcIntensityN0(float2 uv, float2 offset, int k)
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + k * offset).r * _GaussSamples[k];
        }

        float CalcIntensityN1(float2 uv, float2 offset, int k)
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - k * offset).r * _GaussSamples[k];
        }

        float CalcIntensity(float2 uv, float2 offset)
        {
            float intensity = 0;
            // Accumulates horizontal or vertical blur intensity for the specified texture position.
            // Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
            //
            // NOTE: Unroll directive is needed to make the method function on platforms like WebGL 1.0 where loops are not supported.
            // If maximum outline width is changed here, it should be changed in OutlineResources.MaxWidth as well.
            //
            [unroll(32)]
            for (int k = 1; k <= _Width; ++k)
            {
                intensity += CalcIntensityN0(uv, offset, k);
                intensity += CalcIntensityN1(uv, offset, k);
            }

            intensity += CalcIntensityN0(uv, offset, 0);
            return intensity;
        }

        float4 FragmentH(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float intensity = CalcIntensity(uv, float2(_BlitTexture_TexelSize.x, 0));
            return float4(intensity, intensity, intensity, 1);
        }

        float4 FragmentV(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float intensity = clamp(CalcIntensity(uv, float2(0, _BlitTexture_TexelSize.y)), 0, 1);
            float mask = 1 - SAMPLE_TEXTURE2D_X(_MaskTexture, sampler_MaskTexture, uv).r;
            float4 color = SAMPLE_TEXTURE2D_X(_InitialImage, sampler_InitialImage, uv);
            float glowMask = min(mask, intensity * _OutlineColor.a);
            float4 result = lerp(color, _OutlineColor, glowMask);
            return float4(result.rgb, 1);
        }
        ENDHLSL

        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Horizontal blur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentH
            ENDHLSL
        }

        Pass
        {
            Name "Vertical blur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentV
            ENDHLSL
        }
    }
}