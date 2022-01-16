using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ViJTools
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
        /// <param name="layers"></param>
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

    public class CameraTracer
    {
        public readonly static IComparer<RaycastHit> HitDistanceComparer = new FuncComparer<RaycastHit>((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

        private RaycastHit[] mHits;

        private int mCurrentHitCount;

        private int mRaycastCapacity;

        private LayerSettingsContainer mLayerSettings = new LayerSettingsContainer();

        public int RaycastCapacity
        {
            get => mRaycastCapacity;
            set
            {
                mRaycastCapacity = value;
                if (mHits == null)
                    mHits = new RaycastHit[mRaycastCapacity];
                else if (mRaycastCapacity != mHits.Length)
                    Array.Resize(ref mHits, mRaycastCapacity);
            }
        }

        public LayerSettingsContainer LayerSettings => mLayerSettings;

        /// <summary>
        /// The capacity of the tracer is the maximum count of hits it can find. Increase number if you have a lot of colliders under pointers
        /// </summary>
        /// <param name="raycastCapacity"></param>
        public CameraTracer(int raycastCapacity = 100)
        {
            RaycastCapacity = raycastCapacity;
        }

        /// <summary>
        /// Searches nearest to camera interaction object under position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryTraceInteractionObject(Camera camera, Vector2 position, out InteractionObject result)
        {
            TraceCamera3D(position, camera);
            for (int i = 0; i < mCurrentHitCount; i++)
                if (!mHits[i].transform.HasInteractionIgnorer() && mHits[i].transform.TryFindInteractionObject(out result))
                    return true;

            result = null;
            return false;
        }

        /// <summary>
        /// Checks if there is UI under given screen pos
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsOverUI(Vector2 pos)
        {
            var eventData = new PointerEventData(EventSystem.current) { position = pos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        /// <summary>
        /// Checks if screen coord is inside camera rect and camera is enabled
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        public bool CanBeTraced(Camera camera, Vector2 coord)
        {
            return camera.pixelRect.Contains(coord) && camera.enabled && camera.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Traces camera and saves result to current mHits. The count of hits is under mCurrentHitCount
        /// </summary>
        /// <param name="position"></param>
        /// <param name="camera"></param>
        private void TraceCamera3D(Vector2 position, Camera camera)
        {
            var mask = LayerSettings.GetMaskForCamera(camera);
            var ray = camera.ScreenPointToRay(position, Camera.MonoOrStereoscopicEye.Mono);
            var raycastLimitPlane = new Plane(-camera.transform.forward, camera.transform.position + camera.transform.forward * camera.farClipPlane);
            raycastLimitPlane.Raycast(ray, out var rayDistance);
            mCurrentHitCount = Physics.RaycastNonAlloc(ray, mHits, rayDistance, mask);
            if (mCurrentHitCount == mRaycastCapacity)
                Debug.LogWarning("Hit capacity limit reached, I recommend you to increase mRaycastHitCapacity");

            Array.Sort(mHits, 0, mCurrentHitCount, HitDistanceComparer);
        }
    }
}