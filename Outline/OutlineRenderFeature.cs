using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace UTools.Outline
{
    [Serializable]
    public class OutlineSettings
    {
        [SerializeField] private Color m_Color = Color.white;
        [SerializeField] [Range(1, 32)] private int m_OutlineWidth = 16;
        [SerializeField] [Range(1, 32)] private int m_BlurWidth = 16;
        [SerializeField] private LayerMask m_LayerMask = -1;

        public Color Color => m_Color;
        public int OutlineWidth => m_OutlineWidth;
        public int BlurWidth => m_BlurWidth;
        public LayerMask LayerMask => m_LayerMask;
    }

    public class OutlineRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_RenderObjectsShader;
        [SerializeField] private Shader m_BlitShader;

        [SerializeField] private OutlineSettings m_Settings;

        public OutlineSettings Settings => m_Settings;

        private Material m_RenderObjectsMaterial;
        private Material m_BlitMaterial;
        private OutlinePass m_RenderPass;

        public override void Create()
        {
            if (m_RenderObjectsShader == null)
                return;
            if (m_BlitShader == null)
                return;

            m_RenderObjectsMaterial = CoreUtils.CreateEngineMaterial(m_RenderObjectsShader);
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(m_BlitShader);
            m_RenderPass = new OutlinePass(m_RenderObjectsMaterial, m_BlitMaterial);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (m_RenderPass == null)
                return;

            if (renderingData.cameraData.cameraType == CameraType.Game)
                m_RenderPass.Setup(this, renderer.cameraColorTargetHandle);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_RenderPass == null)
                return;

            if (renderingData.cameraData.cameraType == CameraType.Game)
                renderer.EnqueuePass(m_RenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_RenderObjectsMaterial);
            CoreUtils.Destroy(m_BlitMaterial);

            if (m_RenderPass != null)
                m_RenderPass.Cleanup();
        }
    }
}