using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UTools.Input
{
    public class CameraTracer
    {
        private static readonly IComparer<RaycastHit> HitDistanceComparer = new FuncComparer<RaycastHit>((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

        private RaycastHit[] m_Hits;
        private int m_CurrentHitCount;
        private int m_RaycastCapacity;
        private List<RaycastResult> m_RaycastResults = new();

        public int RaycastCapacity
        {
            get => m_RaycastCapacity;
            set
            {
                m_RaycastCapacity = value;
                if (m_Hits == null)
                    m_Hits = new RaycastHit[m_RaycastCapacity];
                else if (m_RaycastCapacity != m_Hits.Length)
                    Array.Resize(ref m_Hits, m_RaycastCapacity);
            }
        }

        public LayerSettingsContainer LayerSettings { get; } = new();

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
        public bool TryTraceInteractionObject(Camera camera, Vector2 position, out InteractionObjectBase result)
        {
            TraceCamera3D(position, camera);
            for (int i = 0; i < m_CurrentHitCount; i++)
                if (!m_Hits[i].collider.transform.HasInteractionIgnorer() && m_Hits[i].collider.transform.TryFindInteractionObject(out result))
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
            var mask = LayerSettings.RaycastMask & ~Physics.IgnoreRaycastLayer;
            var eventData = new PointerEventData(EventSystem.current) { position = pos };
            m_RaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, m_RaycastResults);
            var result = m_RaycastResults.Count != 0 && m_RaycastResults.Any(c => (1 << c.gameObject.layer & mask) != 0 && c.module is GraphicRaycaster or PanelRaycaster);
            // Debug.Log( $"IsOverUI: {result}");
            return result;
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
            var trans = camera.transform;
            var forward = trans.forward;
            var raycastLimitPlane = new Plane(-forward, trans.position + forward * camera.farClipPlane);
            raycastLimitPlane.Raycast(ray, out var rayDistance);
            m_CurrentHitCount = Physics.RaycastNonAlloc(ray, m_Hits, rayDistance, mask);
            if (m_CurrentHitCount == m_RaycastCapacity)
                Debug.LogWarning("Hit capacity limit reached, I recommend you to increase mRaycastHitCapacity");

            Array.Sort(m_Hits, 0, m_CurrentHitCount, HitDistanceComparer);
        }
    }
}