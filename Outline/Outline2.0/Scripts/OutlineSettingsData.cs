using System;
using UnityEngine;
using UnityEngine.Rendering;

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

        public Material MaskMaterial => m_MaskMaterial ? m_MaskMaterial : throw new Exception($"{nameof(m_MaskMaterial)} must not be null");

        public Material OutlineMaterial => m_OutlineMaterial ? m_OutlineMaterial : throw new Exception($"{nameof(m_OutlineMaterial)} must not be null");

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
}