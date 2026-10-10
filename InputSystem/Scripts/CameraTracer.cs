using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UTools.Input
{
    public class CameraTracer
    {
        private static readonly IComparer<RaycastHit> HitDistanceComparer = new FuncComparer<RaycastHit>((hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

        //Hits closer to each other than this are at one distance: which of them comes first is not left to rounding.
        private const float TieDistance = 1e-5f;

        private RaycastHit[] m_Hits;
        private int m_CurrentHitCount;
        private int m_RaycastCapacity;
        private List<RaycastResult> m_RaycastResults = new();
        private PointerEventData m_EventData;
        private EventSystem m_EventDataSystem;
        private readonly List<PointerHit> m_Traced = new();
        private readonly List<float> m_Distances = new();

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
            TraceInteractionObjects(camera, position, m_Traced);
            result = m_Traced.Count != 0 ? m_Traced[0].Target : null;
            return result != null;
        }

        /// <summary>
        /// Searches all interaction objects under position, the nearest to camera first, each of them once, with the
        /// point where the ray met it. Of two objects met at one distance, a child comes before its parent.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="position"></param>
        /// <param name="results">Is cleared first</param>
        public void TraceInteractionObjects(Camera camera, Vector2 position, List<PointerHit> results)
        {
            results.Clear();
            m_Distances.Clear();
            TraceCamera3D(position, camera);
            for (int i = 0; i < m_CurrentHitCount; i++)
            {
                var hit = m_Hits[i];
                if (hit.collider.transform.HasInteractionIgnorer() || !hit.collider.transform.TryFindInteractionObject(out var interactionObject))
                    continue;
                if (Contains(results, interactionObject))
                    continue;

                results.Add(new PointerHit(interactionObject, hit.point));
                m_Distances.Add(hit.distance);
            }

            for (int i = 1; i < results.Count; i++)
            {
                if (m_Distances[i] - m_Distances[i - 1] > TieDistance || !results[i].Target.transform.IsChildOf(results[i - 1].Target.transform))
                    continue;

                (results[i - 1], results[i]) = (results[i], results[i - 1]);
                (m_Distances[i - 1], m_Distances[i]) = (m_Distances[i], m_Distances[i - 1]);
            }
        }

        private static bool Contains(List<PointerHit> hits, InteractionObjectBase interactionObject)
        {
            foreach (var hit in hits)
            {
                if (hit.Target == interactionObject)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if there is UI under given screen pos
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsOverUI(Vector2 pos)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            var mask = LayerSettings.RaycastMask & ~Physics.IgnoreRaycastLayer;
            if (m_EventData == null || m_EventDataSystem != eventSystem)
            {
                m_EventData = new PointerEventData(eventSystem);
                m_EventDataSystem = eventSystem;
            }

            m_EventData.position = pos;
            m_RaycastResults.Clear();
            eventSystem.RaycastAll(m_EventData, m_RaycastResults);
            foreach (var result in m_RaycastResults)
            {
                if ((1 << result.gameObject.layer & mask) != 0 && result.module is GraphicRaycaster or PanelRaycaster)
                    return true;
            }

            return false;
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