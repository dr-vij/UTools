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
        [Tooltip("The size of the outline's blur buffers relative to the camera target, per axis. Below 1 the glow is " +
                 "spread and blurred at that resolution and stretched back: the same look on screen for a fraction of " +
                 "the cost, for the GPUs of headsets.")]
        [SerializeField] [Range(0.25f, 1f)] private float m_ResolutionScale = 1f;
        [Tooltip("The points on the outline width's circle the glow is spread from. Fewer cost less; with the glow " +
                 "buffers scaled down the circle is shorter in their texels and needs fewer points to stay closed.")]
        [SerializeField] [Range(8, 32)] private int m_SpreadSamples = 32;

        private Material m_MaskMaterial;
        private Material m_OutlineMaterial;

        public LayerMask LayerMask => m_LayerMask;

        public int OutlineWidth => m_OutlineWidth;

        public int BlurWidth => m_BlurWidth;

        public Color Color => m_Color;

        public float ResolutionScale => m_ResolutionScale;

        public int SpreadSamples => m_SpreadSamples;

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

        /// <summary>The blur taps for the blur buffers: the blur width scaled with them keeps its size on screen.</summary>
        public OutlineHelpers.BlurTaps GetBlurTaps() =>
            OutlineHelpers.GetBlurTaps(Mathf.Max(1, Mathf.RoundToInt(m_BlurWidth * m_ResolutionScale)));
    }
}
