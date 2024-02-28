using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UTools.Outline
{
    [Serializable]
    public class OutlineSettingsData : IDisposable
    {
        [SerializeField] private LayerMask m_LayerMask = -1;
        [SerializeField] [Range(1, 32)] private int m_OutlineWidth = 16;
        [SerializeField] [Range(1, 32)] private int m_BlurWidth = 16;
        [SerializeField] private Color m_Color = Color.white;

        private Material m_MaskMaterial;
        private Material m_OutlineMaterial;

        public LayerMask LayerMask => m_LayerMask;

        public int OutlineWidth => m_OutlineWidth;

        public int BlurWidth => m_BlurWidth;

        public Color Color => m_Color;

        public Material MaskMaterial => m_MaskMaterial;

        public Material OutlineMaterial => m_OutlineMaterial;

        public bool Init(Shader maskShader, Shader outlineShader)
        {
            if (maskShader == null || outlineShader == null)
                return false;

            m_MaskMaterial = CoreUtils.CreateEngineMaterial(maskShader);
            m_OutlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
            return true;
        }

        public void Dispose()
        {
            if (m_MaskMaterial != null)
            {
                CoreUtils.Destroy(m_MaskMaterial);
                m_MaskMaterial = null;
            }

            if (m_OutlineMaterial != null)
            {
                CoreUtils.Destroy(m_OutlineMaterial);
                m_OutlineMaterial = null;
            }
        }

        public float[] GetGaussSamples() => OutlineHelpers.GetGaussSamples(m_BlurWidth);
    }

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