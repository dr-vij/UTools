
#if OUTLINE_URP
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UTools.Outline
{
    public partial class OutlineRenderGraphFeature
    {
        // The passes of OutlineShader.
        private const int SpreadPass = 0;
        private const int BlurPass = 1;
        private const int CompositeShaderPass = 2;

        // The drawing state shared by the passes. A nested class, not fields of the feature: the feature is a
        // ScriptableObject, and Unity creates no object while one is constructed.
        private static class Shared
        {
            internal static readonly MaterialPropertyBlock PropertyBlock = new();

            internal static readonly int BlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");
            internal static readonly int BlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");
            internal static readonly int BlitTexelSizePropertyId = Shader.PropertyToID("_BlitTexture_TexelSize");
            internal static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
            internal static readonly int SpreadSamplesId = Shader.PropertyToID("_SpreadSamples");
            internal static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
            internal static readonly int MaskTextureId = Shader.PropertyToID("_MaskTexture");
            internal static readonly int BlurDirectionId = Shader.PropertyToID("_BlurDirection");
            internal static readonly int BlurCenterWeightId = Shader.PropertyToID("_BlurCenterWeight");
            internal static readonly int BlurWeightsId = Shader.PropertyToID("_BlurWeights");
            internal static readonly int BlurOffsetsId = Shader.PropertyToID("_BlurOffsets");
            internal static readonly int BlurTapCountId = Shader.PropertyToID("_BlurTapCount");
        }

        /// <summary>The mask and the glow of a frame, handed from the glow pass to the composite.</summary>
        private class OutlineFrameData : ContextItem
        {
            internal TextureHandle Mask;
            internal TextureHandle Glow;

            public override void Reset()
            {
                Mask = TextureHandle.nullHandle;
                Glow = TextureHandle.nullHandle;
            }
        }

        private class PassData
        {
            internal RendererListHandle RenderersList;

            internal OutlineSettingsData OutlineSettings;
            internal int ShaderPass;
            internal Vector2 BlurDirection;
            // Set explicitly: a texture of a property block does not set it, and the value a blit of URP left in the
            // global would follow wherever in the frame the pass runs.
            internal Vector4 SourceTexelSize;

            internal TextureHandle SourceTexture;
            internal TextureHandle MaskTexture;
        }

        /// <summary>
        /// The mask of the outlined objects at the camera's resolution, then the glow spread from it and blurred in
        /// buffers scaled by OutlineSettingsData.ResolutionScale. Reads nothing of the scene.
        /// </summary>
        private class GlowPass : ScriptableRenderPass
        {
            private static readonly List<ShaderTagId> ShaderTagIds = new()
            {
                new("UniversalForwardOnly"),
                new("UniversalForward"),
                new("SRPDefaultUnlit"), //LEGACY
                new("LightweightForward") //LEGACY
            };

            private readonly OutlineSettingsData m_OutlineSettings;

            public GlowPass(OutlineSettingsData settings)
            {
                m_OutlineSettings = settings;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var cameraData = frameData.Get<UniversalCameraData>();

                // Single-channel buffers without antialiasing: the mask and the glow hold one intensity, and the
                // camera's MSAA (4x on Quest) would make every full-screen pass over them four times as costly.
                var maskDesc = cameraData.cameraTargetDescriptor;
                maskDesc.depthBufferBits = 0;
                maskDesc.msaaSamples = 1;
                maskDesc.graphicsFormat = GraphicsFormat.R8_UNorm;
                var glowDesc = maskDesc;
                glowDesc.width = Mathf.Max(1, Mathf.RoundToInt(maskDesc.width * m_OutlineSettings.ResolutionScale));
                glowDesc.height = Mathf.Max(1, Mathf.RoundToInt(maskDesc.height * m_OutlineSettings.ResolutionScale));

                var mask = UniversalRenderer.CreateRenderGraphTexture(renderGraph, maskDesc, "OutlineMask", false);
                var glowA = UniversalRenderer.CreateRenderGraphTexture(renderGraph, glowDesc, "OutlineGlowA", false, FilterMode.Bilinear);
                var glowB = UniversalRenderer.CreateRenderGraphTexture(renderGraph, glowDesc, "OutlineGlowB", false, FilterMode.Bilinear);

                RenderMask(renderGraph, frameData, mask);
                RenderFullScreen(renderGraph, "Outline Spread", SpreadPass, mask, TexelSize(maskDesc), glowA, Vector2.zero);
                RenderFullScreen(renderGraph, "Outline Blur Horizontal", BlurPass, glowA, TexelSize(glowDesc), glowB, Vector2.right);
                RenderFullScreen(renderGraph, "Outline Blur Vertical", BlurPass, glowB, TexelSize(glowDesc), glowA, Vector2.up);

                var outline = frameData.Create<OutlineFrameData>();
                outline.Mask = mask;
                outline.Glow = glowA;
            }

            private static Vector4 TexelSize(RenderTextureDescriptor desc) =>
                new(1f / desc.width, 1f / desc.height, desc.width, desc.height);

            private void RenderFullScreen(RenderGraph renderGraph, string passName, int shaderPass, TextureHandle source,
                Vector4 sourceTexelSize, TextureHandle target, Vector2 blurDirection)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData);
                passData.SourceTexture = source;
                passData.OutlineSettings = m_OutlineSettings;
                passData.ShaderPass = shaderPass;
                passData.BlurDirection = blurDirection;
                passData.SourceTexelSize = sourceTexelSize;

                builder.UseTexture(source);
                builder.SetRenderAttachment(target, 0);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    Shared.PropertyBlock.Clear();
                    Shared.PropertyBlock.SetTexture(Shared.BlitTexturePropertyId, data.SourceTexture);
                    Shared.PropertyBlock.SetVector(Shared.BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    Shared.PropertyBlock.SetVector(Shared.BlitTexelSizePropertyId, data.SourceTexelSize);
                    Shared.PropertyBlock.SetFloat(Shared.OutlineWidthId, settings.OutlineWidth);
                    Shared.PropertyBlock.SetFloat(Shared.SpreadSamplesId, settings.SpreadSamples);
                    if (data.ShaderPass == BlurPass)
                    {
                        var taps = settings.GetBlurTaps();
                        Shared.PropertyBlock.SetVector(Shared.BlurDirectionId, data.BlurDirection);
                        Shared.PropertyBlock.SetFloat(Shared.BlurCenterWeightId, taps.CenterWeight);
                        Shared.PropertyBlock.SetFloatArray(Shared.BlurWeightsId, taps.Weights);
                        Shared.PropertyBlock.SetFloatArray(Shared.BlurOffsetsId, taps.Offsets);
                        Shared.PropertyBlock.SetInt(Shared.BlurTapCountId, taps.Count);
                    }

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, data.ShaderPass, MeshTopology.Triangles, 3, 1, Shared.PropertyBlock);
                });
            }

            private void RenderMask(RenderGraph renderGraph, ContextContainer frameData, TextureHandle maskTexture)
            {
                using var maskBuilder = renderGraph.AddRasterRenderPass<PassData>("Outline Mask", out var passData);
                //Prepare render list of affected objects
                passData.RenderersList = PrepareRenderList(renderGraph, frameData);
                maskBuilder.UseRendererList(passData.RenderersList);
                //Prepare the buffer to render this objects to and set render function
                maskBuilder.SetRenderAttachment(maskTexture, 0);
                maskBuilder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(true, true, Color.clear);
                    context.cmd.DrawRendererList(data.RenderersList);
                });
            }

            private RendererListHandle PrepareRenderList(RenderGraph renderGraph, ContextContainer frameData)
            {
                // Access the relevant frame data from the Universal Render Pipeline
                var universalRenderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();

                var sortingCriteria = cameraData.defaultOpaqueSortFlags;
                var filterSettings = new FilteringSettings(RenderQueueRange.opaque, m_OutlineSettings.LayerMask);

                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(ShaderTagIds, universalRenderingData, cameraData, lightData, sortingCriteria);
                drawSettings.overrideMaterial = m_OutlineSettings.MaskMaterial;
                var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
                return renderGraph.CreateRendererList(param);
            }
        }

        /// <summary>
        /// The glow laid over the camera colour by blending, outside the mask. The camera colour is never read, and the
        /// pass attaches the camera's targets like the scene's passes before it (ScenePassAttachments), so URP can merge
        /// it into their render pass instead of storing and reloading the colour.
        /// </summary>
        private class CompositePass : ScriptableRenderPass
        {
            private readonly OutlineSettingsData m_OutlineSettings;

            public CompositePass(OutlineSettingsData settings)
            {
                m_OutlineSettings = settings;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (!frameData.Contains<OutlineFrameData>())
                    return;
                var outline = frameData.Get<OutlineFrameData>();

                using var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Composite", out var passData);
                passData.SourceTexture = outline.Glow;
                passData.MaskTexture = outline.Mask;
                passData.OutlineSettings = m_OutlineSettings;

                builder.UseTexture(outline.Glow);
                builder.UseTexture(outline.Mask);
                Rendering.ScenePassAttachments.Attach(builder, frameData);

                // Draws with the outline material, not with Blitter's default one: on the way into Play Mode the editor can
                // render the Game view before Blitter's statics are reset, and the pipeline created by that render then runs
                // the whole session with them null.
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    Shared.PropertyBlock.Clear();
                    Shared.PropertyBlock.SetTexture(Shared.BlitTexturePropertyId, data.SourceTexture);
                    Shared.PropertyBlock.SetVector(Shared.BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    Shared.PropertyBlock.SetTexture(Shared.MaskTextureId, data.MaskTexture);
                    Shared.PropertyBlock.SetColor(Shared.OutlineColorId, settings.Color);

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, CompositeShaderPass, MeshTopology.Triangles, 3, 1, Shared.PropertyBlock);
                });
            }
        }
    }
}

#endif
