
#if OUTLINE_URP
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UTools.Outline
{
    public partial class OutlineRenderGraphFeature
    {
        private class CustomRenderPass : ScriptableRenderPass
        {
            private class PassData
            {
                internal RendererListHandle RenderersList;

                internal OutlineSettingsData OutlineSettings;
                
                //Textures For Blit
                internal TextureHandle SourceTexture;
                internal TextureHandle InitialTexture;
                internal TextureHandle MaskTexture;
            }

            private static readonly MaterialPropertyBlock MaterialPropertyBlock = new();

            private static readonly int BlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");
            private static readonly int BlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");
            private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
            private static readonly int BlurWidthId = Shader.PropertyToID("_BlurWidth");
            private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
            private static readonly int GaussSamplesId = Shader.PropertyToID("_GaussSamples");
            private static readonly int MaskTextureId = Shader.PropertyToID("_MaskTexture");
            private static readonly int InitialImageId = Shader.PropertyToID("_InitialImage");

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
                const string maskPassName = "Prepare Mask Pass";
                const string outlinePassName = "Blit Outline Pass";
                const string horizontalPassName = "Blit Horizontal Blur Pass";
                const string verticalPassName = "Blit Vertical Blur Final Pass";
                
                const string maskTextureName = "MaskPassResult";
                const string tempTextureAName = "TempTextureA";
                const string tempTextureBName = "TempTextureB";

                var resourceData = frameData.Get<UniversalResourceData>();

                //Prepare textures to use in graph
                var cameraData = frameData.Get<UniversalCameraData>();
                var renderTextureDesc = cameraData.cameraTargetDescriptor;
                renderTextureDesc.depthBufferBits = 0;

                var maskTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTextureDesc, maskTextureName, false);
                var tempTextureA = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTextureDesc, tempTextureAName, false);
                var tempTextureB = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTextureDesc, tempTextureBName, false);

                RenderMask(renderGraph, frameData, maskPassName, maskTexture);
                RenderOutline(renderGraph, outlinePassName, maskTexture, tempTextureA);
                RenderHorizontalBlur(renderGraph, horizontalPassName, tempTextureA, tempTextureB);
                RenderVerticalBlurAndMerge(renderGraph, verticalPassName, tempTextureB, maskTexture, resourceData, tempTextureA);
                RenderCopyBack(renderGraph, resourceData, tempTextureA);
            }

            private static void RenderCopyBack(RenderGraph renderGraph, UniversalResourceData resourceData, TextureHandle tempTextureA)
            {
                using var blitBuilder = renderGraph.AddRasterRenderPass<PassData>("Copy active color", out var blitPassData);
                blitBuilder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                blitPassData.SourceTexture = tempTextureA;
                blitBuilder.UseTexture(blitPassData.SourceTexture);
                blitBuilder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.SourceTexture, new Vector4(1, 1, 0, 0), 0, false);
                });
            }

            private void RenderVerticalBlurAndMerge(RenderGraph renderGraph, string verticalPassName, TextureHandle sourceWithPreviousPass, TextureHandle maskTexture,
                UniversalResourceData resourceData, TextureHandle renderTarget)
            {
                using var horizontalBlurBuilder = renderGraph.AddRasterRenderPass<PassData>(verticalPassName, out var horizontalBlurPassData);
                horizontalBlurPassData.SourceTexture = sourceWithPreviousPass;
                horizontalBlurPassData.MaskTexture = maskTexture;
                horizontalBlurPassData.InitialTexture = resourceData.activeColorTexture;
                horizontalBlurPassData.OutlineSettings = m_OutlineSettings;

                horizontalBlurBuilder.UseTexture(horizontalBlurPassData.SourceTexture);
                horizontalBlurBuilder.UseTexture(horizontalBlurPassData.MaskTexture);
                horizontalBlurBuilder.UseTexture(horizontalBlurPassData.InitialTexture);
                horizontalBlurBuilder.SetRenderAttachment(renderTarget, 0);

                horizontalBlurBuilder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    MaterialPropertyBlock.Clear();
                    MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                    MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    MaterialPropertyBlock.SetFloat(BlurWidthId, settings.BlurWidth);
                    MaterialPropertyBlock.SetFloatArray(GaussSamplesId, settings.GetGaussSamples());

                    MaterialPropertyBlock.SetTexture(InitialImageId, data.InitialTexture);
                    MaterialPropertyBlock.SetTexture(MaskTextureId, data.MaskTexture);
                    MaterialPropertyBlock.SetColor(OutlineColorId, settings.Color);

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, 2, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                });
            }

            private void RenderHorizontalBlur(RenderGraph renderGraph, string horizontalPassName, TextureHandle tempTextureA, TextureHandle tempTextureB)
            {
                using var horizontalBlurBuilder = renderGraph.AddRasterRenderPass<PassData>(horizontalPassName, out var horizontalBlurPassData);
                horizontalBlurPassData.SourceTexture = tempTextureA;
                horizontalBlurPassData.OutlineSettings = m_OutlineSettings;

                horizontalBlurBuilder.UseTexture(horizontalBlurPassData.SourceTexture);
                horizontalBlurBuilder.SetRenderAttachment(tempTextureB, 0);

                horizontalBlurBuilder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    MaterialPropertyBlock.Clear();
                    MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                    MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    MaterialPropertyBlock.SetFloat(BlurWidthId, settings.BlurWidth);
                    MaterialPropertyBlock.SetFloatArray(GaussSamplesId, settings.GetGaussSamples());

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, 1, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                });
            }

            private void RenderOutline(RenderGraph renderGraph, string outlinePassName, TextureHandle maskTexture, TextureHandle tempTextureA)
            {
                using var outlineBuilder = renderGraph.AddRasterRenderPass<PassData>(outlinePassName, out var blitPassData);
                blitPassData.SourceTexture = maskTexture;
                blitPassData.OutlineSettings = m_OutlineSettings;

                outlineBuilder.SetRenderAttachment(tempTextureA, 0);
                outlineBuilder.UseTexture(blitPassData.SourceTexture);

                outlineBuilder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    var settings = data.OutlineSettings;

                    MaterialPropertyBlock.Clear();
                    MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                    MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                    MaterialPropertyBlock.SetFloat(OutlineWidthId, settings.OutlineWidth);

                    context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, 0, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                });
            }

            private void RenderMask(RenderGraph renderGraph, ContextContainer frameData, string maskPassName, TextureHandle maskTexture)
            {
                using var maskBuilder = renderGraph.AddRasterRenderPass<PassData>(maskPassName, out var passData);
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