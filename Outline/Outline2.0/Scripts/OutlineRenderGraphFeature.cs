#if OUTLINE_URP
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UTools.Outline
{
    public partial class OutlineRenderGraphFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_MaskShader;
        [SerializeField] private Shader m_OutlineShader;

        [SerializeField] private OutlineSettingsData m_OutlineSettings = new();

        private GlowPass m_GlowPass;
        private CompositePass m_CompositePass;

        public override void Create()
        {
            m_OutlineSettings.Init(m_MaskShader, m_OutlineShader);

            // The glow depends only on the outlined objects, so it is made before the scene is drawn; the composite comes
            // last, and with nothing between them that reads the camera colour URP keeps it in the scene's render pass.
            m_GlowPass = new GlowPass(m_OutlineSettings) { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques };
            m_CompositePass = new CompositePass(m_OutlineSettings) { renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing };
        }

        protected override void Dispose(bool disposing)
        {
            m_OutlineSettings.Dispose();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Nothing to outline (OutlineTarget), or a render that must not carry the outline - a picture of the scene -
            // holds it off (OutlineSuspension).
            if (!OutlineTarget.Any || OutlineSuspension.IsSuspended)
                return;
            renderer.EnqueuePass(m_GlowPass);
            renderer.EnqueuePass(m_CompositePass);
        }
    }
}
#endif
