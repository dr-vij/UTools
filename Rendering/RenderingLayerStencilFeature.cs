using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UTools.Rendering
{
    /// <summary>
    /// Writes a stencil value wherever the renderers of a rendering layer are seen (RenderingLayerOverlay marks them),
    /// their colour and depth left untouched: a later pass that draws only where the stencil says so draws them too.
    /// On the headset what a hand holds clears so, before the board's passes, the stencil mark of what lies beside the
    /// window of the wall, which those passes leave undrawn: a held device is drawn wherever the hand brings it. The renderers are drawn with their own
    /// materials under one render state - no colour written, no depth written, the depth test as they are seen. Nothing
    /// is drawn while no overlay of the layer is on; the pass attaches the targets as the scene's passes do
    /// (ScenePassAttachments).
    /// </summary>
    public sealed class RenderingLayerStencilFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderingLayerMask m_RenderingLayer;
        [SerializeField] private RenderPassEvent m_Event = RenderPassEvent.AfterRenderingOpaques;
        [Tooltip("The value written where the renderers are, within the write mask.")]
        [SerializeField] [Range(0, 255)] private int m_StencilReference = 1;
        [SerializeField] [Range(0, 255)] private int m_StencilWriteMask = 1;

        private StencilPass m_Pass;

        public override void Create() => m_Pass = new StencilPass(this) { renderPassEvent = m_Event };

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (RenderingLayerOverlay.IsAnyOn(m_RenderingLayer.value))
                renderer.EnqueuePass(m_Pass);
        }

        private class PassData
        {
            internal RendererListHandle Renderers;
        }

        private sealed class StencilPass : ScriptableRenderPass
        {
            private static readonly List<ShaderTagId> ShaderTagIds = new()
            {
                new("UniversalForwardOnly"),
                new("UniversalForward"),
                new("SRPDefaultUnlit"),
            };

            private readonly RenderingLayerStencilFeature m_Feature;

            public StencilPass(RenderingLayerStencilFeature feature) => m_Feature = feature;

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var renderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();

                var drawing = RenderingUtils.CreateDrawingSettings(ShaderTagIds, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
                var filtering = new FilteringSettings(RenderQueueRange.all, -1, m_Feature.m_RenderingLayer);

                var stencil = StencilState.defaultValue;
                stencil.enabled = true;
                stencil.SetCompareFunction(CompareFunction.Always);
                stencil.SetPassOperation(StencilOp.Replace);
                stencil.readMask = 255;
                stencil.writeMask = (byte)m_Feature.m_StencilWriteMask;
                var blend = BlendState.defaultValue;
                blend.blendState0 = new RenderTargetBlendState((ColorWriteMask)0);
                var block = new RenderStateBlock(RenderStateMask.Stencil | RenderStateMask.Blend | RenderStateMask.Depth)
                {
                    stencilReference = m_Feature.m_StencilReference,
                    stencilState = stencil,
                    blendState = blend,
                    depthState = new DepthState(false, CompareFunction.LessEqual),
                };

                var list = new RendererListParams(renderingData.cullResults, drawing, filtering)
                {
                    // One state for every pass of the renderers: a tag value of none matches them all.
                    tagValues = new NativeArray<ShaderTagId>(new[] { ShaderTagId.none }, Allocator.Temp),
                    stateBlocks = new NativeArray<RenderStateBlock>(new[] { block }, Allocator.Temp),
                    isPassTagName = false,
                };

                using var builder = renderGraph.AddRasterRenderPass<PassData>("Rendering Layer Stencil", out var data);
                data.Renderers = renderGraph.CreateRendererList(list);
                builder.UseRendererList(data.Renderers);
                ScenePassAttachments.Attach(builder, frameData);
                builder.SetRenderFunc(static (PassData passData, RasterGraphContext context) => context.cmd.DrawRendererList(passData.Renderers));
            }
        }
    }
}
