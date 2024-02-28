Shader "Hidden/OutlineShader"
{
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _OutlineColor = float4(1, 1, 1, 1);
        int _BlurWidth = 5;
        int _OutlineWidth = 5;

        float _GaussSamples[32];

        TEXTURE2D_X(_InitialImage);
        SAMPLER(sampler_InitialImage);

        TEXTURE2D_X(_BackgroundTexture);
        SAMPLER(sampler_BackgroundTexture);

        SAMPLER(sampler_BlitTexture);

        TEXTURE2D_X(_MaskTexture);
        SAMPLER(sampler_MaskTexture);


        float OutlinePositive(float2 uv, float2 offset, int k)
        {
            return step(0.001, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + k * offset).r);
        }

        float OutlineNegative(float2 uv, float2 offset, int k)
        {
            return step(0.001, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - k * offset).r);
        }

        // Accumulates horizontal or vertical blur intensity for the specified texture position.
        // Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
        //
        // NOTE: Unroll directive is needed to make the method function on platforms like WebGL 1.0 where loops are not supported.
        // If maximum outline width is changed here, it should be changed in OutlineResources.MaxWidth as well.
        //
        float Outline(float2 uv, float2 offset)
        {
            float intensity = 0;
            [unroll(32)]
            for (int k = 1; k <= _OutlineWidth; ++k)
            {
                intensity = OutlinePositive(uv, offset, k);
                intensity = max(OutlineNegative(uv, offset, k), intensity);
            }

            intensity = max(OutlinePositive(uv, offset, 0), intensity);
            return intensity;
        }

        float CalcIntensityPositive(float2 uv, float2 offset, int k)
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + k * offset).r * _GaussSamples[k];
        }

        float CalcIntensityNegative(float2 uv, float2 offset, int k)
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - k * offset).r * _GaussSamples[k];
        }

        // Accumulates horizontal or vertical blur intensity for the specified texture position.
        // Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
        //
        // NOTE: Unroll directive is needed to make the method function on platforms like WebGL 1.0 where loops are not supported.
        // If maximum outline width is changed here, it should be changed in OutlineResources.MaxWidth as well.
        //
        float CalcIntensity(float2 uv, float2 offset)
        {
            float intensity = 0;
            [unroll(32)]
            for (int k = 1; k <= _BlurWidth; ++k)
            {
                intensity += CalcIntensityPositive(uv, offset, k);
                intensity += CalcIntensityNegative(uv, offset, k);
            }

            intensity += CalcIntensityPositive(uv, offset, 0);
            return intensity;
        }

        float4 OutlineFragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float width = _OutlineWidth * _BlitTexture_TexelSize.x;
            float height = _OutlineWidth * _BlitTexture_TexelSize.y;

            const float TAU = 6.28318530;
            const float steps = 32.0;

            float intensity = 0;
            for (int i = 0; i < steps; i++)
            {
                float angle = TAU * i / steps;
                float2 offset = float2(cos(angle) * width, sin(angle) * height);
                intensity = max(step(0.001, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + offset).r),
                                intensity);
            }
            return float4(intensity, intensity, intensity, 1);
        }

        float4 FragmentHorizontalBlur(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float intensity = clamp(CalcIntensity(uv, float2(_BlitTexture_TexelSize.x, 0)), 0, 1);
            return float4(intensity, intensity, intensity, 1);
        }

        float4 FragmentVerticalBlur(Varyings input) : SV_Target
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

        //Outline pass. does the outlining of the mask
        Pass
        {
            Name "Outline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment OutlineFragment
            ENDHLSL
        }
        
        //Horizontal blur pass. 
        Pass
        {
            Name "Horizontal blur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentHorizontalBlur
            ENDHLSL
        }

        Pass
        {
            Name "Vertical blur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentVerticalBlur
            ENDHLSL
        }
    }
}