using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ViJTools
{
    public class InputManager : SingletonMonobehaviour<InputManager>
    {
        /// <summary>
        /// This parameters determines how far should drag perform to start drag. If distance is less, press will be performed
        /// </summary>
        [SerializeField] private int m_DragOrPressTriggerDistance = 0;

        /// <summary>
        /// Raise drags on update if true
        /// </summary>
        [SerializeField] private bool m_RaiseDragOnUpdates = true;

        private InputActions m_Actions;
        private List<Camera> m_Cameras = new List<Camera>();
        private CameraTracer m_CameraTracer = new CameraTracer();

        private InputDevice m_ActiveDevice;
        private HashSet<int> m_ActiveTouches = new HashSet<int>();
        private bool m_IsMouseOrPenInputStarted;
        private Dictionary<InteractionObject, PointerGestureAnalizer> mActiveGestures = new Dictionary<InteractionObject, PointerGestureAnalizer>();

        public CameraTracer CameraTracer => m_CameraTracer;

        public int DragOrPressTriggerDistance => m_DragOrPressTriggerDistance;

        public IEnumerable<PointerGestureAnalizer> ActiveGestures => mActiveGestures.Values;

        public void RegisterCamera(Camera cam)
        {
            if (!m_Cameras.Contains(cam))
            {
                m_Cameras.Add(cam);
                if (!cam.TryGetComponent<InteractionObject>(out var interactionObject))
                    cam.gameObject.AddComponent<InteractionObject>();
                m_Cameras.Sort((cam1, cam2) => cam1.depth.CompareTo(cam2.depth));
            }
        }

        /// <summary>
        /// Create Actions and subscribe it's events
        /// </summary>
        private void OnEnable()
        {
            m_Actions = new InputActions();
            m_Actions.GestureActions.Enable();

            m_Actions.GestureActions.Pointer.performed += OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled += OnPointerEvent;
        }

        /// <summary>
        /// Unsubscribe from all events and dispose Actions
        /// </summary>
        private void OnDisable()
        {
            m_Actions.Disable();

            m_Actions.GestureActions.Pointer.performed -= OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled -= OnPointerEvent;
        }

        private void OnPointerEvent(InputAction.CallbackContext context)
        {
            //I DO NOT ALLOW TO MIX DIFFERENT DEVICES IN THIS COMBINATION OF GESTURES
            if (CanBeHandled(context))
            {
                var data = context.ReadValue<PointerInput>();
                if (context.control.device is Touchscreen)
                {
                    //Check if new contact
                    if (data.Contact)
                    {
                        if (m_ActiveTouches.Contains(data.InputId))
                        {
                            //POINTER DRAG
                            OnProcessedPointerUpdate(data);
                        }
                        else
                        {
                            m_ActiveTouches.Add(data.InputId);
                            m_ActiveDevice = context.control.device;
                            //POINTER DOWN
                            OnProcessedPointerDown(data);
                        }
                    }
                    else
                    {
                        if (m_ActiveTouches.Contains(data.InputId))
                        {
                            m_ActiveTouches.Remove(data.InputId);
                            if (m_ActiveTouches.Count == 0)
                                m_ActiveDevice = null;
                            //POINTER UP
                            OnProcessedPointerUp(data);
                        }
                    }
                }

                var isMouse = context.control.device is Mouse;
                var isPen = context.control.device is Pen;

                if (isMouse || isPen)
                {
                    //We do not have ID in the beginning. So we fill it here
                    var id = isMouse ? Helpers.LeftMouseInputId : Helpers.PenInputId;
                    data.InputId = id;

                    if (data.Contact)
                    {
                        if (m_IsMouseOrPenInputStarted)
                        {
                            //POINTER DRAG
                            OnProcessedPointerUpdate(data);
                        }
                        else
                        {
                            m_IsMouseOrPenInputStarted = true;
                            m_ActiveDevice = context.control.device;
                            //POINTER DOWN
                            OnProcessedPointerDown(data);
                        }
                    }
                    else
                    {
                        if (m_IsMouseOrPenInputStarted)
                        {
                            m_IsMouseOrPenInputStarted = false;
                            m_ActiveDevice = null;
                            //POINTER UP
                            OnProcessedPointerUp(data);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Cant be handled");
            }
        }

        /// <summary>
        /// Gestures start here 
        /// </summary>
        /// <param name="data"></param>
        private void OnProcessedPointerDown(PointerInput data)
        {
            if (!m_CameraTracer.IsOverUI(data.Position))
            {
                var goodCameras = GetTracebleCameras(data.Position);
                if (goodCameras.Count != 0)
                {
                    var camera = goodCameras[0];
                    if (m_CameraTracer.TryTraceInteractionObject(camera, data.Position, out var interactionObject))
                    {
                        if (!mActiveGestures.TryGetValue(interactionObject, out var gesture))
                        {
                            gesture = new PointerGestureAnalizer(interactionObject, camera);
                            mActiveGestures.Add(interactionObject, gesture);
                        }

                        gesture?.CreatePointer(data.InputId, data.Position);
                    }
                    else
                    {
                        //TODO: CAMERA INTERACTION CAN BE HERE
                    }
                }
            }
        }

        private void OnProcessedPointerUpdate(PointerInput data)
        {
            //Just update data. everything should be inside subscribtions
            foreach (var gesture in mActiveGestures.Values)
            {
                if (gesture.TryGetPointer(data.InputId, out var pointer))
                    pointer.UpdateDataAndRaise(data.Position);
            }
        }

        private void OnProcessedPointerUp(PointerInput data)
        {
            var toRemoveList = new List<InteractionObject>();
            foreach (var gestureKeyVal in mActiveGestures)
            {
                var gesture = gestureKeyVal.Value;
                if (gesture.HasPointer(data.InputId))
                {
                    gesture.RemovePointer(data.InputId, data.Position);
                    if (gesture.PointersCount == 0)
                        toRemoveList.Add(gestureKeyVal.Key);
                }
            }
            foreach (var gesture in toRemoveList)
                mActiveGestures.Remove(gesture);
        }

        /// <summary>
        /// Checks if action can be handled from this device. We do not allow to use pen/mouse and touch at the same time
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool CanBeHandled(InputAction.CallbackContext context)
        {
            return m_ActiveDevice == null || m_ActiveDevice.GetType() == context.control.device.GetType();
        }

        private List<Camera> GetTracebleCameras(Vector2 position)
        {
            if (m_Cameras.Count == 0)
                Debug.LogWarning("No traceble cameras. Did you forget to add one?");

            return m_Cameras.Where(camera => m_CameraTracer.CanBeTraced(camera, position)).ToList();
        }

        public static void RunEventOnCamera(Camera cam, InteractionEvent evt, InteractionEventArgs args)
        {
            if (cam != null && cam.TryGetComponent<InteractionObject>(out var cameraInteractionObject))
                cameraInteractionObject.RunEvent(evt, args);
        }

        public static void Set2dPos(Camera cam, Transform t, Vector2 pos)
        {
            var ray = cam.ScreenPointToRay(pos);
            var plane = new Plane(Vector3.back, 0);
            plane.Raycast(ray, out var enter);
            var point3d = ray.GetPoint(enter);
            t.transform.position = point3d;
        }
    }
}