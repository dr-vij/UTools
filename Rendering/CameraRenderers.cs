using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UTools.Rendering
{
    /// <summary>The renderer a camera draws with, for a caller without a reference to the render pipeline assembly.</summary>
    public static class CameraRenderers
    {
        /// <summary>
        /// Makes the camera draw with the renderer at this index of the pipeline asset's renderer list; -1 - the asset's
        /// default one.
        /// </summary>
        public static void Use(Camera camera, int rendererIndex) => camera.GetUniversalAdditionalCameraData().SetRenderer(rendererIndex);
    }
}
