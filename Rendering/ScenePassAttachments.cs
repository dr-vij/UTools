using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace UTools.Rendering
{
    /// <summary>
    /// Attaches a raster pass drawn between URP's own passes over the scene to the camera's colour and depth the way they
    /// attach them, so that it stays in their native render pass: depth read-write, and in XR the same foveated
    /// rasterization and multiview render regions. On Quest 3 a pass without the XR flags broke the render pass and the
    /// next one loaded its memoryless MSAA colour (an error every frame, a third of the frames lost); with a read-only
    /// depth it broke without the error, storing and reloading the eye buffers (a quarter of the frames lost).
    /// </summary>
    internal static class ScenePassAttachments
    {
        public static void Attach(IRasterRenderGraphBuilder builder, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            builder.SetRenderAttachment(resources.activeColorTexture, 0);
            if (resources.activeDepthTexture.IsValid())
                builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.ReadWrite);

            if (!cameraData.xr.enabled)
                return;
            // URP also foveates its passes into an intermediate texture where the platform allows it, which on Quest is
            // the default viewport; that flag is internal, and the camera of the headset always has the default viewport.
            builder.EnableFoveatedRasterization(cameraData.xr.supportsFoveatedRendering);
            if (!cameraData.xr.isQuadViewInnerPass)
                builder.SetExtendedFeatureFlags(ExtendedFeatureFlags.MultiviewRenderRegionsCompatible);
        }
    }
}
