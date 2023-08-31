using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

namespace UTools.Outline
{
    internal class OutlinePass : ScriptableRenderPass
    {
        private readonly Material m_MaskRenderObjectMat;
        private readonly Material m_BlitOutlineMaterial;
        private RTHandle m_CameraTex;
        private RTHandle m_Depth;
        private OutlineRenderFeature m_Feature;

        private readonly List<ShaderTagId> m_ShaderTagIdList = new();

        private RTHandle m_MaskTex;
        private RTHandle m_TempTexA;
        private RTHandle m_TempTexB;

        public OutlinePass(Material renderObjectMaskMaterial, Material blitOutlineMaterial)
        {
            m_MaskRenderObjectMat = renderObjectMaskMaterial;
            m_BlitOutlineMaterial = blitOutlineMaterial;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        }

        public void Setup(OutlineRenderFeature feature, RTHandle colorHandle)
        {
            m_Feature = feature;
            m_CameraTex = colorHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //Color buffers
            var camColorDesc = renderingData.cameraData.cameraTargetDescriptor;
            camColorDesc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref m_MaskTex, Vector2.one, camColorDesc, wrapMode: TextureWrapMode.Clamp, name: "MaskColor");
            RenderingUtils.ReAllocateIfNeeded(ref m_TempTexA, Vector2.one, camColorDesc, wrapMode: TextureWrapMode.Clamp, name: "TempColor1");
            RenderingUtils.ReAllocateIfNeeded(ref m_TempTexB, Vector2.one, camColorDesc, wrapMode: TextureWrapMode.Clamp, name: "TempColor2");

            //Depth buffer
            var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
            depthDesc.depthBufferBits = 32;
            RenderingUtils.ReAllocateIfNeeded(ref m_Depth, Vector2.one, camColorDesc);

            //Configure
            ConfigureTarget(m_MaskTex, m_Depth);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_MaskRenderObjectMat == null)
                return;

            //First we draw the mask to render target
            CommandBuffer cmd = CommandBufferPool.Get();
            var desc = new RendererListDesc(m_ShaderTagIdList.ToArray(), renderingData.cullResults, renderingData.cameraData.camera)
            {
                overrideMaterial = m_MaskRenderObjectMat,
                sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags,
                renderQueueRange = RenderQueueRange.all,
                layerMask = m_Feature.Settings.LayerMask,
            };
            var rendererList = context.CreateRendererList(desc);
            cmd.DrawRendererList(rendererList);

            cmd.SetGlobalTexture("_InitialImage", m_TempTexA);
            cmd.SetGlobalTexture("_MaskTexture", m_MaskTex);
            cmd.SetGlobalFloatArray("_GaussSamples", OutlineHelpers.GetGaussSamples(16));
            cmd.SetGlobalFloat("_Width", m_Feature.Settings.Width);
            cmd.SetGlobalColor("_OutlineColor", m_Feature.Settings.Color);

            Blitter.BlitCameraTexture(cmd, m_CameraTex, m_TempTexA);
            Blitter.BlitCameraTexture(cmd, m_MaskTex, m_TempTexB, m_BlitOutlineMaterial, 0);
            Blitter.BlitCameraTexture(cmd, m_TempTexB, m_CameraTex, m_BlitOutlineMaterial, 1);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Cleanup()
        {
            CoreUtils.Destroy(m_MaskTex);
            CoreUtils.Destroy(m_TempTexA);
            CoreUtils.Destroy(m_TempTexB);
            CoreUtils.Destroy(m_Depth);
        }
    }
}