using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ViJTools
{
    public class CameraTracer
    {
        public readonly static IComparer<RaycastHit> HitDistanceComparer = new FuncComparer<RaycastHit>((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

        //SETTINGS
        private int mRaycastCapacity;

        //BUFFER DATA
        private RaycastHit[] mHits;
        private int mCurrentHitCount;

        public int RaycastCapacity
        {
            get => mRaycastCapacity;
            set
            {
                mRaycastCapacity = value;
                RefreshCapacityArray();
            }
        }

        #region LAYERS/MASKS

        private int mRaycastLayerMask;

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
        /// <param name="layers"></param>
        public void AddMask(int mask) => RaycastMask |= mask;

        /// <summary>
        /// Removes bits from raycast mask
        /// </summary>
        /// <param name="mask"></param>
        public void RemoveMask(int mask) => RaycastMask &= ~mask;

        #endregion

        public CameraTracer(int raycastCapacity)
        {
            RaycastCapacity = raycastCapacity;
        }

        private void RefreshCapacityArray()
        {
            Array.Resize(ref mHits, mRaycastCapacity);
        }

        public void TraceCamera(Vector2 position, Camera camera)
        {
            var mask = mRaycastLayerMask & camera.cullingMask & ~Physics.IgnoreRaycastLayer;
            var ray = camera.ScreenPointToRay(position, Camera.MonoOrStereoscopicEye.Mono);
            var raycastLimitPlane = new Plane(-camera.transform.forward, camera.transform.position + camera.transform.forward * camera.farClipPlane);
            raycastLimitPlane.Raycast(ray, out var rayDistance);
            mCurrentHitCount = Physics.RaycastNonAlloc(ray, mHits, rayDistance, mask);
            if (mCurrentHitCount == mRaycastCapacity)
                Debug.LogWarning("Hit capacity limit reached, I recommend you to increase mRaycastHitCapacity");

            Array.Sort(mHits, 0, mCurrentHitCount, HitDistanceComparer);
        }

        public bool IsOverUI(Vector2 pos)
        {
            var eventData = new PointerEventData(EventSystem.current) { position = pos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
    }
}