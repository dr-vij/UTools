using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UTools.Editor
{
    public enum SceneViewPlane
    {
        XPlane,
        YPlane,
        ZPlane
    }

    public enum PlaneType
    {
        CameraPlane = 0,
        WolrdPlane = 1,
        ObjectPlane = 2
    }

    public class SceneCameraEditor : EditorWindow
    {
        private static bool m_IsInitialized = false;

        [SerializeField] private SceneViewSettings m_Settings;
        [SerializeField] private bool m_IsScenviewOverriderEnabled = false;

        private double m_MouseDownTimeWithAlt;
        private double m_MouseDownClickTimeWithAlt = 0.3f;
        private double m_MouseClickDistance = 10f;
        private Vector2 m_MouseDownTimeWithAltPosition;

        //Rotation visualizer
        private Vector3 m_RotateAroundPoint;
        private bool m_DragWasStarted = false;

        //Zoom point visualizer
        private Vector3 m_ZoomTarget;
        private double m_ZoomStartedTime;
        private double m_ZoomTime = 1f;

        [MenuItem("Tools/UTools/SceneView Camera Extension")]
        public static void ShowWindow()
        {
            GetWindow<SceneCameraEditor>("SceneView Settings").Show();
        }

        private void UpdateInitializationState(bool isMustBeEnabled)
        {
            if (isMustBeEnabled)
            {
                if (!m_IsInitialized)
                {
                    SceneView.duringSceneGui += AfterGUI;
                    m_IsInitialized = true;
                }
            }
            else
            {
                if (m_IsInitialized)
                {
                    SceneView.duringSceneGui -= AfterGUI;
                    m_IsInitialized = false;
                }
            }
        }

        private void OnGUI()
        {
            //Plugin on off
            if (!m_IsInitialized)
            {
                if (GUILayout.Button("Enable plugin"))
                    m_IsScenviewOverriderEnabled = true;
            }
            else
            {
                if (GUILayout.Button("Disable plugin"))
                    m_IsScenviewOverriderEnabled = false;
            }

            UpdateInitializationState(m_IsScenviewOverriderEnabled);
            if (!m_IsInitialized)
                return;
            //-------------

            EditorGUILayout.LabelField("Scroll sensevity");
            m_Settings.ScrollSensetivity = EditorGUILayout.Slider(m_Settings.ScrollSensetivity, 0f, 1f);

            EditorGUILayout.LabelField("Near clip");
            m_Settings.NearClip = Mathf.Clamp(m_Settings.NearClip, 1e-6f, float.MaxValue);
            m_Settings.NearClip = EditorGUILayout.FloatField(m_Settings.NearClip);

            EditorGUILayout.LabelField("Far clip");
            m_Settings.FarClip = Mathf.Clamp(m_Settings.FarClip, m_Settings.NearClip, 1e6f);
            m_Settings.FarClip = EditorGUILayout.FloatField(m_Settings.FarClip);

            RefreshCameraSettings(SceneView.lastActiveSceneView);
        }

        protected void OnEnable()
        {
            m_Settings ??= new SceneViewSettings();
            var data = EditorPrefs.GetString(nameof(SceneViewSettings), JsonUtility.ToJson(m_Settings, false));
            if (data != null)
                JsonUtility.FromJsonOverwrite(data, m_Settings);
        }

        protected void OnDisable()
        {
            var data = JsonUtility.ToJson(m_Settings, false);
            EditorPrefs.SetString(nameof(SceneViewSettings), data);
        }

        //Teleport pivot without any scaling, just change the view target. (we move pivot to the new place and change size to make same zoom)
        private void SetCameraPlaneToPosition2d(SceneView sceneView, Vector2 position2d)
        {
            var vectorTowardsCamera = sceneView.rotation * Vector3.back;
            GetNearestPlane(sceneView, position2d, out var planeTouched);
            var cameraOldPosition = sceneView.pivot + vectorTowardsCamera * sceneView.cameraDistance;
            var rayFromOldCameraPosition = new Ray(cameraOldPosition, -vectorTowardsCamera);
            planeTouched.Raycast(rayFromOldCameraPosition, out var newEnter);

            var newPivotPosition = rayFromOldCameraPosition.GetPoint(newEnter);

            var beforeDistance = sceneView.cameraDistance;
            var afterDistance = Vector3.Distance(newPivotPosition, cameraOldPosition);
            var newSize = (afterDistance * sceneView.size) / beforeDistance;
            sceneView.pivot = newPivotPosition;
            if (!sceneView.orthographic)
                sceneView.size = newSize;
        }

        private void DrawCubeIfNeeded(float size)
        {
            if (EditorApplication.timeSinceStartup - m_ZoomStartedTime < m_ZoomTime)
                Handles.DrawWireCube(m_ZoomTarget, Vector3.one * size);
        }

        private void StartVisualZoom(Vector3 position)
        {
            m_ZoomTarget = position;
            m_ZoomStartedTime = EditorApplication.timeSinceStartup;
        }

        private void StopVisualZoom() => m_ZoomStartedTime = 0;

        private void AfterGUI(SceneView sceneView)
        {
            //Drawing handles:
            var cubeSize = sceneView.cameraDistance / 45f;
            if (m_DragWasStarted)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireCube(m_RotateAroundPoint, Vector3.one * cubeSize);
            }

            DrawCubeIfNeeded(cubeSize);
            //----------------

            var e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (e.button == 0 && e.alt)
                    {
                        if (!m_DragWasStarted)
                        {
                            m_DragWasStarted = true;
                            SetCameraPlaneToPosition2d(sceneView, e.mousePosition);
                            //now get the point to rotate around:s
                            GetNearestPlane(sceneView, e.mousePosition, out _, out m_RotateAroundPoint);
                        }

                        Quaternion rotation = sceneView.rotation;
                        var newPivot = RotatePoint(sceneView.pivot, m_RotateAroundPoint, Quaternion.AngleAxis(e.delta.y * .003f * Mathf.Rad2Deg, rotation * Vector3.right));
                        newPivot = RotatePoint(newPivot, m_RotateAroundPoint, Quaternion.AngleAxis(e.delta.x * .003f * Mathf.Rad2Deg, Vector3.up));
                        sceneView.pivot = newPivot;
                        rotation = Quaternion.AngleAxis(e.delta.y * 0.18f, rotation * Vector3.right) * rotation;
                        rotation = Quaternion.AngleAxis(e.delta.x * 0.18f, Vector3.up) * rotation;
                        sceneView.rotation = rotation;
                        e.Use();
                    }

                    StopVisualZoom();
                    break;
                case EventType.MouseDown:
                    //Change point of interest
                    if (e.button == 0 && e.alt)
                    {
                        m_MouseDownTimeWithAlt = EditorApplication.timeSinceStartup;
                        m_MouseDownTimeWithAltPosition = e.mousePosition;
                    }

                    StopVisualZoom();
                    break;
                case EventType.MouseUp:
                    //Change point of interest
                    if (e.button == 0 && e.alt)
                    {
                        if (EditorApplication.timeSinceStartup - m_MouseDownTimeWithAlt <= m_MouseDownClickTimeWithAlt &&
                            (m_MouseDownTimeWithAltPosition - e.mousePosition).magnitude < m_MouseClickDistance)
                        {
                            SetCameraPlaneToPosition2d(sceneView, e.mousePosition);
                        }

                        m_DragWasStarted = false;
                    }

                    StopVisualZoom();
                    break;
                case EventType.ScrollWheel:
                    e.Use();
                    var zoomModifyer = e.delta.y > 0 ? 1 + m_Settings.ScrollSensetivity : 1 / (1 + m_Settings.ScrollSensetivity);
                    float targetSize;
                    Vector3 targetPivot;
                    var vectorTowardsCamera = sceneView.rotation * Vector3.back;
                    var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                    //Move to planes or colliders
                    if (e.alt)
                    {
                        SetCameraPlaneToPosition2d(sceneView, e.mousePosition);

                        // Movement algorithm:
                        // we move camera to point on plane, then we check were the pivot would be in this case.
                        GetNearestPlane(sceneView, e.mousePosition, out var planeTouched);
                        planeTouched.Raycast(ray, out var enter);
                        var zoomTargetPosition = ray.GetPoint(enter);
                        var cameraBeforeCameraZoomDelta = sceneView.pivot + vectorTowardsCamera * sceneView.cameraDistance - zoomTargetPosition;
                        var oldDirectionLength = cameraBeforeCameraZoomDelta.magnitude;
                        var newDirectionLength = oldDirectionLength * zoomModifyer;
                        var newCameraPosition = zoomTargetPosition + cameraBeforeCameraZoomDelta.normalized * newDirectionLength;
                        var newCameraPositionRay = new Ray(newCameraPosition, -vectorTowardsCamera);
                        planeTouched.Raycast(newCameraPositionRay, out var newPivotIntersection);
                        targetPivot = newCameraPositionRay.GetPoint(newPivotIntersection);
                        targetSize = sceneView.size * zoomModifyer;
                        StartVisualZoom(zoomTargetPosition);
                    }
                    else
                    {
                        targetPivot = sceneView.pivot;
                        targetSize = sceneView.size * zoomModifyer;
                        StopVisualZoom();
                    }

                    sceneView.LookAt(targetPivot, sceneView.rotation, targetSize, sceneView.orthographic, false);
                    break;
            }

            RefreshCameraSettings(sceneView);
        }

        private void RefreshCameraSettings(SceneView sv)
        {
            var settings = sv.cameraSettings;
            settings.dynamicClip = false;
            settings.nearClip = m_Settings.NearClip;
            settings.farClip = m_Settings.FarClip;
            sv.cameraSettings = settings;
            sv.Repaint();
        }

        private PlaneType GetNearestPlane(SceneView sv, Vector2 mousePosition, out Plane plane)
        {
            return GetNearestPlane(sv, mousePosition, out plane, out _);
        }

        private PlaneType GetNearestPlane(SceneView sv, Vector2 mousePosition, out Plane plane, out Vector3 position3d)
        {
            var vectorTowardsCamera = sv.rotation * Vector3.back;
            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            //Default returned plane
            var planeType = PlaneType.CameraPlane;
            plane = new Plane(vectorTowardsCamera, sv.pivot);
            plane.Raycast(ray, out var defaultEnter);
            position3d = ray.GetPoint(defaultEnter);

            //First we try to raycast any meshrenderer
            var go = EditorMeshIntersections.GetObjectUnder2dPosition(mousePosition);
            MeshFilter filter = null;
            if (go != null)
                filter = go.GetComponent<MeshFilter>();
            if (filter != null && EditorMeshIntersections.RaycastMeshFilter(ray, filter, out var hit))
            {
                plane = new Plane(vectorTowardsCamera, hit.point);
                planeType = PlaneType.ObjectPlane;
                position3d = hit.point;
            }
            //Then we try to fly to any world plane
            else
            {
                bool wolrdPlaneFound = false;
                float nearestDistance = float.PositiveInfinity;
                foreach (var worldPlane in m_Settings.ZoomPlanes)
                {
                    if (worldPlane.Raycast(ray, out var worldPlaneEnter))
                    {
                        wolrdPlaneFound |= true;
                        planeType = PlaneType.WolrdPlane;
                        if (worldPlaneEnter < nearestDistance)
                        {
                            nearestDistance = worldPlaneEnter;
                            plane = worldPlane;
                            position3d = ray.GetPoint(worldPlaneEnter);
                        }
                    }
                }
            }

            return planeType;
        }

        private Vector3 RotatePoint(Vector3 initialPoint, Vector3 pivot, Quaternion rotation)
        {
            var direction = initialPoint - pivot;
            var rotatedDirection = rotation * direction;
            return rotatedDirection + pivot;
        }
    }

    static class SceneViewHelpers
    {
        public static Vector3 VectorTowardsCamera(this SceneView sv) => sv.rotation * Vector3.back;

        public static Vector3 VectorFromCamera(this SceneView sv) => -VectorTowardsCamera(sv);

        public static void DebugSceneView(this SceneView sv) => Debug.Log($"Pivot: {sv.pivot}, Rotation: {sv.rotation}, Size: {sv.size}, Ortho: {sv.orthographic}");
    }
}