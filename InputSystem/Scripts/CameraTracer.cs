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
        private Collider[] m_Colliders;
        private int m_CurrentHitCount;
        private int m_RaycastCapacity;
        private List<RaycastResult> m_RaycastResults = new();
        private PointerEventData m_EventData;
        private EventSystem m_EventDataSystem;
        private readonly List<PointerHit> m_Traced = new();
        private readonly List<float> m_Distances = new();
        private readonly List<Vector3> m_MeshVertices = new();
        private readonly List<int> m_MeshTriangles = new();
        private readonly HashSet<Mesh> m_UnreadableMeshes = new();

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
                m_Colliders = new Collider[m_RaycastCapacity];
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
            TraceCamera3D(position, camera);
            CollectHits(results);
        }

        /// <summary>
        /// Searches all interaction objects along a ray of the world - a hand, a controller - the nearest first, each
        /// of them once, with the point where the ray met it. Of two objects met at one distance, a child comes before
        /// its parent.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <param name="mask">The layers to trace, LayerSettings.RaycastMask for all the pointers see</param>
        /// <param name="results">Is cleared first</param>
        public void TraceInteractionObjects(Ray ray, float maxDistance, int mask, List<PointerHit> results)
        {
            Trace(ray, maxDistance, mask);
            CollectHits(results);
        }

        /// <summary>
        /// Searches all interaction objects within reach of a point of the world - a pinch of a hand - the nearest
        /// first, each of them once, with the point of its collider nearest to the point. Of two objects at one
        /// distance, a child comes before its parent.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="radius"></param>
        /// <param name="mask">The layers to search, LayerSettings.RaycastMask for all the pointers see</param>
        /// <param name="results">Is cleared first</param>
        public void OverlapInteractionObjects(Vector3 point, float radius, int mask, List<PointerHit> results)
        {
            var count = Physics.OverlapSphereNonAlloc(point, radius, m_Colliders, mask);
            if (count == m_Colliders.Length)
                Debug.LogWarning("Hit capacity limit reached, I recommend you to increase mRaycastHitCapacity");

            results.Clear();
            m_Distances.Clear();
            var sqrRadius = radius * radius;
            for (var i = 0; i < count; i++)
            {
                // The cheap tests first: the exact distance to a non-convex mesh walks its triangles.
                var collider = m_Colliders[i];
                if (!TryGetNewTarget(results, collider, out var target) || collider.bounds.SqrDistance(point) > sqrRadius)
                    continue;

                var closest = ClosestPoint(collider, point);
                Insert(results, new PointerHit(target, closest), Vector3.Distance(point, closest));
            }

            PutChildrenFirst(results);
        }

        private void CollectHits(List<PointerHit> results)
        {
            results.Clear();
            m_Distances.Clear();
            for (int i = 0; i < m_CurrentHitCount; i++)
            {
                var hit = m_Hits[i];
                if (TryGetNewTarget(results, hit.collider, out var target))
                    Insert(results, new PointerHit(target, hit.point), hit.distance);
            }

            PutChildrenFirst(results);
        }

        // The interaction object of a collider, unless the collider is ignored or its object is already among the results:
        // the first collider of an object met puts it in.
        private static bool TryGetNewTarget(List<PointerHit> results, Collider collider, out InteractionObjectBase target)
        {
            target = null;
            return !collider.transform.HasInteractionIgnorer() && collider.transform.TryFindInteractionObject(out target) && !Contains(results, target);
        }

        // Of two objects at one distance, a child comes before its parent.
        private void PutChildrenFirst(List<PointerHit> results)
        {
            for (int i = 1; i < results.Count; i++)
            {
                if (m_Distances[i] - m_Distances[i - 1] > TieDistance || !results[i].Target.transform.IsChildOf(results[i - 1].Target.transform))
                    continue;

                (results[i - 1], results[i]) = (results[i], results[i - 1]);
                (m_Distances[i - 1], m_Distances[i]) = (m_Distances[i], m_Distances[i - 1]);
            }
        }

        // The hits of a ray come sorted; the overlap's do not: each is put after the ones nearer than it.
        private void Insert(List<PointerHit> results, PointerHit hit, float distance)
        {
            var index = m_Distances.Count;
            while (index > 0 && m_Distances[index - 1] > distance)
                index--;
            results.Insert(index, hit);
            m_Distances.Insert(index, distance);
        }

        // Physics answers the closest point only for primitives and convex meshes. A mesh that is not convex - the tube of
        // a cable sagging between its plugs - is measured by its own triangles: its bounds would put every point inside
        // the box of the sag at distance zero, ahead of anything really nearer. PhysX could measure it only against a
        // probe collider (ComputePenetration), and a probe has to be enabled in the scene, where every other query would
        // meet it; the triangles of a mesh within reach of a point are few - a cable's tube has a few hundred - and the
        // walk is exact.
        private Vector3 ClosestPoint(Collider collider, Vector3 point)
        {
            if (collider is MeshCollider { convex: false } mesh)
                return ClosestPointOnMesh(mesh, point);
            return collider.ClosestPoint(point);
        }

        private Vector3 ClosestPointOnMesh(MeshCollider collider, Vector3 point)
        {
            var mesh = collider.sharedMesh;
            if (mesh == null)
                return collider.bounds.ClosestPoint(point);

            // A mesh imported without Read/Write has no triangles to walk in a player: it is measured by its bounds, and
            // said so once, for the mesh to be made readable.
            if (!mesh.isReadable)
            {
                if (m_UnreadableMeshes.Add(mesh))
                    Debug.LogError($"The mesh {mesh.name} of the non-convex collider {collider.name} is not readable: its distance is " +
                                   "measured by its bounds. Enable Read/Write on its import.", collider);
                return collider.bounds.ClosestPoint(point);
            }

            // In the world, not in the mesh's own space: a scale that is not uniform changes the distances.
            mesh.GetVertices(m_MeshVertices);
            var toWorld = collider.transform.localToWorldMatrix;
            for (var i = 0; i < m_MeshVertices.Count; i++)
                m_MeshVertices[i] = toWorld.MultiplyPoint3x4(m_MeshVertices[i]);

            var closest = collider.bounds.ClosestPoint(point);
            var closestSqrDistance = float.PositiveInfinity;
            for (var subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
            {
                if (mesh.GetTopology(subMesh) != MeshTopology.Triangles)
                    continue;

                mesh.GetTriangles(m_MeshTriangles, subMesh);
                for (var t = 0; t + 2 < m_MeshTriangles.Count; t += 3)
                {
                    var candidate = ClosestPointOnTriangle(point, m_MeshVertices[m_MeshTriangles[t]], m_MeshVertices[m_MeshTriangles[t + 1]],
                        m_MeshVertices[m_MeshTriangles[t + 2]]);
                    var sqrDistance = (candidate - point).sqrMagnitude;
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closest = candidate;
                    }
                }
            }

            return closest;
        }

        // The point of the triangle abc nearest to p: a corner, a point of an edge or of the face, by the region of p
        // (Ericson, Real-Time Collision Detection, 5.1.5).
        private static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            var ap = p - a;
            var d1 = Vector3.Dot(ab, ap);
            var d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0f && d2 <= 0f)
                return a;

            var bp = p - b;
            var d3 = Vector3.Dot(ab, bp);
            var d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0f && d4 <= d3)
                return b;

            var vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f)
                return a + ab * (d1 / (d1 - d3));

            var cp = p - c;
            var d5 = Vector3.Dot(ab, cp);
            var d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0f && d5 <= d6)
                return c;

            var vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f)
                return a + ac * (d2 / (d2 - d6));

            var va = d3 * d6 - d5 * d4;
            if (va <= 0f && d4 - d3 >= 0f && d5 - d6 >= 0f)
                return b + (c - b) * ((d4 - d3) / (d4 - d3 + (d5 - d6)));

            // A triangle without area has no face: the regions of its corners and edges above hold every point but
            // those along it, for which its corner stands.
            var area = va + vb + vc;
            if (area <= 0f)
                return a;

            return a + ab * (vb / area) + ac * (vc / area);
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
            Trace(ray, rayDistance, mask);
        }

        private void Trace(Ray ray, float maxDistance, int mask)
        {
            m_CurrentHitCount = Physics.RaycastNonAlloc(ray, m_Hits, maxDistance, mask);
            if (m_CurrentHitCount == m_RaycastCapacity)
                Debug.LogWarning("Hit capacity limit reached, I recommend you to increase mRaycastHitCapacity");

            Array.Sort(m_Hits, 0, m_CurrentHitCount, HitDistanceComparer);
        }
    }
}