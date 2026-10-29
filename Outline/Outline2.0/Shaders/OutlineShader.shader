Shader "Hidden/OutlineShader"
{
    Properties
    {
        // The stencil test of the composite (OutlineSettingsData); Always by default.
        [HideInInspector] _StencilComp("Stencil Compare", Int) = 8
        [HideInInspector] _StencilRef("Stencil Reference", Int) = 0
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Int) = 255
    }

    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float4 _OutlineColor = float4(1, 1, 1, 1);
        int _OutlineWidth = 5;
        float _SpreadSamples = 32;

        // One side of the blur as pair taps read with bilinear filtering (OutlineHelpers.BlurTaps).
        float2 _BlurDirection;
        float _BlurCenterWeight;
        float _BlurWeights[16];
        float _BlurOffsets[16];
        int _BlurTapCount;

        TEXTURE2D_X(_MaskTexture);

        float SampleIntensity(float2 uv)
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).r;
        }

        // Spreads the mask: a texel takes the glow where the mask is within the outline width around it. Reads the mask
        // at the camera's resolution whatever the size of the glow buffer it writes.
        float4 FragmentSpread(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float width = _OutlineWidth * _BlitTexture_TexelSize.x;
            float height = _OutlineWidth * _BlitTexture_TexelSize.y;

            const float TAU = 6.28318530;
            float steps = _SpreadSamples;

            float intensity = 0;
            [loop]
            for (int i = 0; i < steps; i++)
            {
                float angle = TAU * i / steps;
                float2 offset = float2(cos(angle) * width, sin(angle) * height);
                intensity = max(step(0.001, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv + offset).r),
                                intensity);
            }
            return float4(intensity, intensity, intensity, 1);
        }

        // Gauss blur along _BlurDirection.
        // NOTE: Unroll directive is needed to make the method function on platforms like WebGL 1.0 where loops are not supported.
        float4 FragmentBlur(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float2 texelStep = _BlurDirection * _BlitTexture_TexelSize.xy;

            float intensity = _BlurCenterWeight * SampleIntensity(uv);
            [unroll(16)]
            for (int t = 0; t < _BlurTapCount; ++t)
            {
                float2 offset = texelStep * _BlurOffsets[t];
                intensity += _BlurWeights[t] * (SampleIntensity(uv + offset) + SampleIntensity(uv - offset));
            }

            intensity = clamp(intensity, 0, 1);
            return float4(intensity, intensity, intensity, 1);
        }

        // The glow outside the mask, laid over the camera colour by the blend of the pass: the colour becomes
        // lerp(colour, _OutlineColor, glow), as if the camera colour were read and mixed here.
        float4 FragmentComposite(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
            float intensity = SampleIntensity(uv);
            float outside = 1 - SAMPLE_TEXTURE2D_X(_MaskTexture, sampler_PointClamp, uv).r;
            return float4(_OutlineColor.rgb, min(outside, intensity * _OutlineColor.a));
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
            Name "Spread"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentSpread
            ENDHLSL
        }

        Pass
        {
            Name "Blur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentBlur
            ENDHLSL
        }

        //Composite pass: blends the glow over the camera colour. The alpha of the target goes the same way towards 1,
        //which is lerp(alpha, _OutlineColor.a, glow) for the opaque outline colours the feature is used with.
        Pass
        {
            Name "Composite"
            ZTest Always
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                Comp [_StencilComp]
            }
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentComposite
            ENDHLSL
        }
    }
}
