
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
        /// <summary>
        /// The mask of the outlined objects at the camera's resolution; the glow spread from it and blurred in buffers
        /// scaled by OutlineSettingsData.ResolutionScale; the glow laid over the camera colour by blending, outside the
        /// mask. The camera colour is never read, so the pass needs no intermediate texture of its own.
        /// </summary>
        private class CustomRenderPass : ScriptableRenderPass
        {
            // The passes of OutlineShader.
            private const int SpreadPass = 0;
            private const int BlurPass = 1;
            private const int CompositePass = 2;

            private class PassData
            {
                internal RendererListHandle RenderersList;

                internal OutlineSettingsData OutlineSettings;
                internal int ShaderPass;
                internal Vector2 BlurDirection;

                internal TextureHandle SourceTexture;
                internal TextureHandle MaskTexture;
            }

            private static readonly MaterialPropertyBlock MaterialPropertyBlock = new();

            private static readonly int BlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");
            private static readonly int BlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");
            private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
            private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
            private static readonly int MaskTextureId = Shader.PropertyToID("_MaskTexture");
            private static readonly int BlurDirectionId = Shader.PropertyToID("_BlurDirection");
            private static readonly int BlurCenterWeightId = Shader.PropertyToID("_BlurCenterWeight");
            private static readonly int BlurWeightsId = Shader.PropertyToID("_BlurWeights");
            private static readonly int BlurOffsetsId = Shader.PropertyToID("_BlurOffsets");
            private static readonly int BlurTapCountId = Shader.PropertyToID("_BlurTapCount");

            private readonly OutlineSettingsData m_OutlineSettings;

            private static readonly List<ShaderTagId> ShaderTagIds = new()
            {
                new("UniversalForwardOnly"),
                new("UniversalForward"),
                new("SRPDefaultUnlit"), //LEGACY
                new("LightweightForward") //LEGACY
            };

            public CustomRenderPass(OutlineSettingsData settings)
            {
                m_OutlineSettings = settings;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
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
                RenderFullScreen(renderGraph, "Outline Spread", SpreadPass, mask, glowA, Vector2.zero);
                RenderFullScreen(renderGraph, "Outline Blur Horizontal", BlurPass, glowA, glowB, Vector2.right);
                RenderFullScreen(renderGraph, "Outline Blur Vertical", BlurPass, glowB, glowA, Vector2.up);
                RenderComposite(renderGraph, glowA, mask, resourceData.activeColorTexture);
            }

            private void RenderFullScreen(RenderGraph renderGraph, string passName, int shaderPass, TextureHandle source,
                TextureHandle target, Vector2 blurDirection)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData);
                passData.SourceTexture = source;
                passData.OutlineSettings = m_OutlineSettings;
                passData.ShaderPass = shaderPass;
                passData.BlurDirection = blurDirection;

                builder.UseTexture(source);
                builder.SetRenderAttachment(target, 0);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    MaterialPropertyBlock.Clear();
                    MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                    MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    MaterialPropertyBlock.SetFloat(OutlineWidthId, settings.OutlineWidth);
                    if (data.ShaderPass == BlurPass)
                    {
                        var taps = settings.GetBlurTaps();
                        MaterialPropertyBlock.SetVector(BlurDirectionId, data.BlurDirection);
                        MaterialPropertyBlock.SetFloat(BlurCenterWeightId, taps.CenterWeight);
                        MaterialPropertyBlock.SetFloatArray(BlurWeightsId, taps.Weights);
                        MaterialPropertyBlock.SetFloatArray(BlurOffsetsId, taps.Offsets);
                        MaterialPropertyBlock.SetInt(BlurTapCountId, taps.Count);
                    }

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, data.ShaderPass, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                });
            }

            private void RenderComposite(RenderGraph renderGraph, TextureHandle glow, TextureHandle mask, TextureHandle cameraColor)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Composite", out var passData);
                passData.SourceTexture = glow;
                passData.MaskTexture = mask;
                passData.OutlineSettings = m_OutlineSettings;

                builder.UseTexture(glow);
                builder.UseTexture(mask);
                builder.SetRenderAttachment(cameraColor, 0);

                // Draws with the outline material, not with Blitter's default one: on the way into Play Mode the editor can
                // render the Game view before Blitter's statics are reset, and the pipeline created by that render then runs
                // the whole session with them null.
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    MaterialPropertyBlock.Clear();
                    MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                    MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    MaterialPropertyBlock.SetTexture(MaskTextureId, data.MaskTexture);
                    MaterialPropertyBlock.SetColor(OutlineColorId, settings.Color);

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, CompositePass, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
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
    }
}

#endif
