using UnityEngine;

namespace UTools.Input
{
    public class LayerSettingsContainer
    {
        /// <summary>
        /// Raycast mask, it is used to filter raycasts with camera
        /// The default value is -1 that means all layers
        /// </summary>
        public int RaycastMask { get; private set; } = -1;

        /// <summary>
        /// Sets layers to raycast mask
        /// </summary>
        /// <param name="layers"></param>
        public void SetLayers(params string[] layers) => RaycastMask = LayerMask.GetMask(layers);

        /// <summary>
        /// Adds layers to raycast mask
        /// </summary>
        /// <param name="layers"></param>
        public void AddLayers(params string[] layers) => AddMask(LayerMask.GetMask(layers));

        /// <summary>
        /// Removes layers from raycast mask
        /// </summary>
        /// <param name="layers"></param>
        public void RemoveLayers(params string[] layers) => RemoveMask(LayerMask.GetMask(layers));

        /// <summary>
        /// Sets layers to raycast mask
        /// </summary>
        /// <param name="layers"></param>
        public void SetLayers(params int[] layers)
        {
            RaycastMask = 0;
            foreach (var layer in layers)
                AddMask(1 << layer);
        }

        /// <summary>
        /// Adds layers to raycast mask
        /// </summary>
        /// <param name="layers"></param>
        public void AddLayers(params int[] layers)
        {
            foreach (var layer in layers)
                AddMask(1 << layer);
        }

        /// <summary>
        /// Removes layers from raycast mask
        /// </summary>
        /// <param name="layers"></param>
        public void RemoveLayers(params int[] layers)
        {
            foreach (var layer in layers)
                RemoveMask(1 << layer);
        }

        /// <summary>
        /// Adds bits to raycast mask
        /// </summary>
        /// <param name="mask"></param>
        public void AddMask(int mask) => RaycastMask |= mask;

        /// <summary>
        /// Removes bits from raycast mask
        /// </summary>
        /// <param name="mask"></param>
        public void RemoveMask(int mask) => RaycastMask &= ~mask;

        public int GetMaskForCamera(Camera camera)
        {
            return RaycastMask & camera.cullingMask & ~Physics.IgnoreRaycastLayer;
        }
    }
}