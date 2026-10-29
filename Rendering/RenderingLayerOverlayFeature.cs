using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UTools.Rendering
{
    /// <summary>
    /// Draws the opaque renderers of a rendering layer once more over the scene with one material (RenderingLayerOverlay
    /// marks them), over the camera's depth: an additive material lights them up where they are seen. Nothing is drawn
    /// while no overlay is on. The pass reads nothing of the camera colour and attaches the targets as the scene's passes
    /// do (ScenePassAttachments), so URP keeps it in the scene's render pass.
    /// </summary>
    public sealed class RenderingLayerOverlayFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderingLayerMask m_RenderingLayer;
        [SerializeField] private Material m_Material;
        [SerializeField] private RenderPassEvent m_Event = RenderPassEvent.AfterRenderingOpaques;
        [Tooltip("The stencil test of the overlay: drawn only where the stencil of the camera's target, masked by the read " +
                 "mask, compares to the reference. Always - everywhere.")]
        [SerializeField] private CompareFunction m_StencilCompare = CompareFunction.Always;
        [SerializeField] [Range(0, 255)] private int m_StencilReference;
        [SerializeField] [Range(0, 255)] private int m_StencilReadMask = 255;

        private OverlayPass m_Pass;

        public override void Create() => m_Pass = new OverlayPass(this) { renderPassEvent = m_Event };

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (RenderingLayerOverlay.Any && m_Material != null)
                renderer.EnqueuePass(m_Pass);
        }

        private class PassData
        {
            internal RendererListHandle Renderers;
        }

        private sealed class OverlayPass : ScriptableRenderPass
        {
            private static readonly List<ShaderTagId> ShaderTagIds = new()
            {
                new("UniversalForwardOnly"),
                new("UniversalForward"),
                new("SRPDefaultUnlit"),
            };

            private readonly RenderingLayerOverlayFeature m_Feature;

            public OverlayPass(RenderingLayerOverlayFeature feature) => m_Feature = feature;

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var renderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();

                var drawing = RenderingUtils.CreateDrawingSettings(ShaderTagIds, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
                drawing.overrideMaterial = m_Feature.m_Material;
                drawing.overrideMaterialPassIndex = 0;
                var filtering = new FilteringSettings(RenderQueueRange.opaque, -1, m_Feature.m_RenderingLayer);

                var list = new RendererListParams(renderingData.cullResults, drawing, filtering);
                if (m_Feature.m_StencilCompare != CompareFunction.Always)
                {
                    var stencil = StencilState.defaultValue;
                    stencil.enabled = true;
                    stencil.SetCompareFunction(m_Feature.m_StencilCompare);
                    stencil.readMask = (byte)m_Feature.m_StencilReadMask;
                    stencil.writeMask = 0;
                    var block = new RenderStateBlock(RenderStateMask.Stencil) { stencilReference = m_Feature.m_StencilReference, stencilState = stencil };
                    // One state for every pass of the renderers: a tag value of none matches them all.
                    list.tagValues = new NativeArray<ShaderTagId>(new[] { ShaderTagId.none }, Allocator.Temp);
                    list.stateBlocks = new NativeArray<RenderStateBlock>(new[] { block }, Allocator.Temp);
                    list.isPassTagName = false;
                }

                using var builder = renderGraph.AddRasterRenderPass<PassData>("Rendering Layer Overlay", out var data);
                data.Renderers = renderGraph.CreateRendererList(list);
                builder.UseRendererList(data.Renderers);
                ScenePassAttachments.Attach(builder, frameData);
                builder.SetRenderFunc(static (PassData passData, RasterGraphContext context) => context.cmd.DrawRendererList(passData.Renderers));
            }
        }
    }
}
