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

        private CustomRenderPass m_ScriptablePass;

        public override void Create()
        {
            m_OutlineSettings.Init(m_MaskShader, m_OutlineShader);

            m_ScriptablePass = new CustomRenderPass(m_OutlineSettings)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        protected override void Dispose(bool disposing)
        {
            m_OutlineSettings.Dispose();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
#endif