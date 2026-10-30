using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UTools.Rendering
{
    /// <summary>
    /// Sets a global shader float at a point of the frame, for the draws that follow until it is set again: a shader
    /// tells by it which passes it is drawn in. Two of them bracket a group of passes - one before, one after, at the same
    /// event in the renderer's list - where a renderer's state cannot reach the draws themselves (UI Toolkit draws its
    /// world panels with a state of its own). The pass draws nothing; it attaches the scene's targets like the passes
    /// around it (ScenePassAttachments) so that it stays in their native render pass.
    /// </summary>
    public sealed class ShaderGlobalFloatFeature : ScriptableRendererFeature
    {
        [Tooltip("The global shader property set, by name.")]
        [SerializeField] private string m_Property;
        [SerializeField] private float m_Value;
        [SerializeField] private RenderPassEvent m_Event = RenderPassEvent.AfterRenderingOpaques;

        private SetPass m_Pass;

        public override void Create() => m_Pass = new SetPass(this) { renderPassEvent = m_Event };

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!string.IsNullOrEmpty(m_Property))
                renderer.EnqueuePass(m_Pass);
        }

        private class PassData
        {
            internal int Id;
            internal float Value;
        }

        private sealed class SetPass : ScriptableRenderPass
        {
            private readonly ShaderGlobalFloatFeature m_Feature;

            public SetPass(ShaderGlobalFloatFeature feature) => m_Feature = feature;

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>("Shader Global " + m_Feature.m_Property, out var data);
                data.Id = Shader.PropertyToID(m_Feature.m_Property);
                data.Value = m_Feature.m_Value;
                ScenePassAttachments.Attach(builder, frameData);
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (PassData passData, RasterGraphContext context) => context.cmd.SetGlobalFloat(passData.Id, passData.Value));
            }
        }
    }
}
