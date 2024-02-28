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
            private class MaskPassData
            {
                internal RendererListHandle RenderersList;
            }

            private class BlitPassData
            {
                internal TextureHandle SourceTexture;
            }

            private class OutlinePassData : BlitPassData
            {
                internal OutlineSettingsData OutlineSettings;
                internal TextureHandle InitialTexture;
                internal TextureHandle MaskTexture;
            }

            private static readonly MaterialPropertyBlock MaterialPropertyBlock = new();

            private static readonly int BlitTexturePropertyId = Shader.PropertyToID("_BlitTexture");
            private static readonly int BlitScaleBiasPropertyId = Shader.PropertyToID("_BlitScaleBias");
            private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
            private static readonly int BlurWidth = Shader.PropertyToID("_BlurWidth");
            private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
            private static readonly int GaussSamples = Shader.PropertyToID("_GaussSamples");
            private static readonly int MaskTexture = Shader.PropertyToID("_MaskTexture");
            private static readonly int InitialImage = Shader.PropertyToID("_InitialImage");

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

                var resourceData = frameData.Get<UniversalResourceData>();

                //Prepare textures to use in graph
                var cameraData = frameData.Get<UniversalCameraData>();
                var renderTextureDesc = cameraData.cameraTargetDescriptor;
                renderTextureDesc.depthBufferBits = 0;

                var maskTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTextureDesc, "MaskPassResult", false);
                var tempTextureA = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTextureDesc, "TempTextureA", false);
                var tempTextureB = UniversalRenderer.CreateRenderGraphTexture(renderGraph, renderTextureDesc, "TempTextureB", false);

                //Here we render the mask of our objects.
                using (var maskBuilder = renderGraph.AddRasterRenderPass<MaskPassData>(maskPassName, out var passData))
                {
                    //Prepare render list of affected objects
                    passData.RenderersList = PrepareRenderList(renderGraph, frameData);
                    maskBuilder.UseRendererList(passData.RenderersList);
                    //Prepare the buffer to render this objects to and set render function
                    maskBuilder.SetRenderAttachment(maskTexture, 0);
                    maskBuilder.SetRenderFunc(static (MaskPassData data, RasterGraphContext context) => { context.cmd.DrawRendererList(data.RenderersList); });
                }

                //here we copy this mask and outline it
                using (var outlineBuilder = renderGraph.AddRasterRenderPass<OutlinePassData>(outlinePassName, out var blitPassData))
                {
                    blitPassData.SourceTexture = maskTexture;
                    blitPassData.OutlineSettings = m_OutlineSettings;

                    outlineBuilder.SetRenderAttachment(tempTextureA, 0);
                    outlineBuilder.UseTexture(blitPassData.SourceTexture);

                    outlineBuilder.SetRenderFunc(static (OutlinePassData data, RasterGraphContext context) =>
                    {
                        var settings = data.OutlineSettings;

                        MaterialPropertyBlock.Clear();
                        MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                        MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                        MaterialPropertyBlock.SetFloat(OutlineWidth, settings.OutlineWidth);

                        context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, 0, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                    });
                }

                //here we blur the outline horizontally
                using (var horizontalBlurBuilder = renderGraph.AddRasterRenderPass<OutlinePassData>(horizontalPassName, out var horizontalBlurPassData))
                {
                    horizontalBlurPassData.SourceTexture = tempTextureA;
                    horizontalBlurPassData.OutlineSettings = m_OutlineSettings;

                    horizontalBlurBuilder.UseTexture(horizontalBlurPassData.SourceTexture);
                    horizontalBlurBuilder.SetRenderAttachment(tempTextureB, 0);

                    horizontalBlurBuilder.SetRenderFunc(static (OutlinePassData data, RasterGraphContext context) =>
                    {
                        var settings = data.OutlineSettings;

                        MaterialPropertyBlock.Clear();
                        MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                        MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                        MaterialPropertyBlock.SetFloat(BlurWidth, settings.BlurWidth);
                        MaterialPropertyBlock.SetFloatArray(GaussSamples, settings.GetGaussSamples());

                        context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, 1, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                    });
                }

                //here we blur the outline vertically, and return the result
                using (var horizontalBlurBuilder = renderGraph.AddRasterRenderPass<OutlinePassData>(verticalPassName, out var horizontalBlurPassData))
                {
                    horizontalBlurPassData.SourceTexture = tempTextureB;
                    horizontalBlurPassData.MaskTexture = maskTexture;
                    horizontalBlurPassData.InitialTexture = resourceData.activeColorTexture;
                    horizontalBlurPassData.OutlineSettings = m_OutlineSettings;

                    horizontalBlurBuilder.UseTexture(horizontalBlurPassData.SourceTexture);
                    horizontalBlurBuilder.UseTexture(horizontalBlurPassData.MaskTexture);
                    horizontalBlurBuilder.UseTexture(horizontalBlurPassData.InitialTexture);
                    horizontalBlurBuilder.SetRenderAttachment(tempTextureA, 0);

                    horizontalBlurBuilder.SetRenderFunc(static (OutlinePassData data, RasterGraphContext context) =>
                    {
                        var settings = data.OutlineSettings;

                        MaterialPropertyBlock.Clear();
                        MaterialPropertyBlock.SetTexture(BlitTexturePropertyId, data.SourceTexture);
                        MaterialPropertyBlock.SetVector(BlitScaleBiasPropertyId, new Vector4(1, 1, 0, 0));
                        MaterialPropertyBlock.SetFloat(BlurWidth, settings.BlurWidth);
                        MaterialPropertyBlock.SetFloatArray(GaussSamples, settings.GetGaussSamples());

                        MaterialPropertyBlock.SetTexture(InitialImage, data.InitialTexture);
                        MaterialPropertyBlock.SetTexture(MaskTexture, data.MaskTexture);
                        MaterialPropertyBlock.SetColor(OutlineColor, settings.Color);

                        context.cmd.DrawProcedural(Matrix4x4.identity, settings.OutlineMaterial, 2, MeshTopology.Triangles, 3, 1, MaterialPropertyBlock);
                    });
                }

                using (var blitBuilder = renderGraph.AddRasterRenderPass<BlitPassData>("Copy active color", out var blitPassData))
                {
                    blitBuilder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    blitPassData.SourceTexture = tempTextureA;
                    blitBuilder.UseTexture(blitPassData.SourceTexture);
                    blitBuilder.SetRenderFunc(static (BlitPassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, data.SourceTexture, new Vector4(1, 1, 0, 0), 0, false);
                    });
                }
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