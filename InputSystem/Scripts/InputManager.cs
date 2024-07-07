using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UTools.Input
{
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        public delegate void ScrollHandler(Vector2 scrollDelta, Vector2 pointerPosition);
        
        /// <summary>
        /// This parameters determines how far should drag perform to start drag. If distance is less, press will be performed
        /// </summary>
        [SerializeField] private int m_DragOrPressTriggerDistance;

        [SerializeField] private bool m_UpdateCausesGestureUpdates = false;

        private InputActions m_Actions;
        private readonly List<Camera> m_Cameras = new();
        private readonly CameraTracer m_CameraTracer = new();
        private readonly HashSet<int> m_ActiveTouches = new();
        private InputDevice m_ActiveDevice;
        private bool m_IsMouseOrPenInputStarted;
        private readonly GesturesContainer m_GesturesContainer = new();
        private Action<int> m_MouseDownEvent;
        private Action<int> m_MouseUpEvent;
        private event ScrollHandler m_GlobalScrollEvent;

        private Vector2 m_MousePosition;

        private readonly HashSet<int> m_PressedButtons = new();

        public int PressedButtonsCount => m_PressedButtons.Count;

        public CameraTracer CameraTracer => m_CameraTracer;

        public int DragOrPressTriggerDistance => m_DragOrPressTriggerDistance;

        public bool GetActualObjectForPointerEventArgs(PointerInteractionEventArgs args, out InteractionObjectBase interactionObject)
        {
            return m_CameraTracer.TryTraceInteractionObject(args.InteractionCamera, args.PointerPosition, out interactionObject);
        }

        public void RegisterCamera(Camera cam)
        {
            if (!m_Cameras.Contains(cam))
            {
                m_Cameras.Add(cam);
                if (!cam.TryGetComponent<InteractionObjectBase>(out _))
                    Debug.LogError("No interaction object for the camera");
                m_Cameras.Sort((cam1, cam2) => cam1.depth.CompareTo(cam2.depth));
            }
            else
            {
                Debug.LogWarning("Camera already registered");
            }
        }

        public IDisposable SubscribeGlobalMouseDown(Action<int> btnDownHandler)
        {
            m_MouseDownEvent += btnDownHandler;
            return new DisposeAction(() => m_MouseDownEvent -= btnDownHandler);
        }

        public IDisposable SubscribeGlobalMouseUp(Action<int> btnUpHandler)
        {
            m_MouseUpEvent += btnUpHandler;
            return new DisposeAction(() => m_MouseUpEvent -= btnUpHandler);
        }

        public IDisposable SubscribeGlobalScrollDelta(ScrollHandler scrollHandler)
        {
            m_GlobalScrollEvent += scrollHandler;
            return new DisposeAction(() => m_GlobalScrollEvent -= scrollHandler);
        }

        /// <summary>
        /// Create Actions and subscribe it's events
        /// </summary>
        private void OnEnable()
        {
            m_Actions = new InputActions();
            m_Actions.GestureActions.Enable();

            //Unified pointer gesture events. mouse + pen + touch
            m_Actions.GestureActions.Pointer.performed += OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled += OnPointerEvent;

            //Mouse gesture events
            m_Actions.GestureActions.MouseLeft.started += OnLeftMouseButtonDownEvent;
            m_Actions.GestureActions.MouseLeft.canceled += OnLeftMouseButtonUpEvent;
            m_Actions.GestureActions.MouseRight.started += OnRightMouseButtonDownEvent;
            m_Actions.GestureActions.MouseRight.canceled += OnRightMouseButtonUpEvent;
            m_Actions.GestureActions.MouseMiddle.started += OnMiddleMouseButtonDownEvent;
            m_Actions.GestureActions.MouseMiddle.canceled += OnMiddleMouseButtonUpEvent;

            m_Actions.GestureActions.MousePosition.performed += OnMousePositionEvent;

            m_Actions.GestureActions.MouseScroll.performed += OnMouseScrollEvent;
        }

        /// <summary>
        /// Unsubscribe from all events and dispose Actions
        /// </summary>
        private void OnDisable()
        {
            m_Actions.Disable();

            //Unified pointer gesture events. mouse + pen + touch
            m_Actions.GestureActions.Pointer.performed -= OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled -= OnPointerEvent;

            //Mouse gesture events
            m_Actions.GestureActions.MouseLeft.started -= OnLeftMouseButtonDownEvent;
            m_Actions.GestureActions.MouseLeft.canceled -= OnLeftMouseButtonUpEvent;
            m_Actions.GestureActions.MouseRight.started -= OnRightMouseButtonDownEvent;
            m_Actions.GestureActions.MouseRight.canceled -= OnRightMouseButtonUpEvent;
            m_Actions.GestureActions.MouseMiddle.started -= OnMiddleMouseButtonDownEvent;
            m_Actions.GestureActions.MouseMiddle.canceled -= OnMiddleMouseButtonUpEvent;

            m_Actions.GestureActions.MousePosition.performed -= OnMousePositionEvent;

            m_Actions.GestureActions.MouseScroll.performed -= OnMouseScrollEvent;
        }

        #region Mouse Gestures

        private void OnLeftMouseButtonDownEvent(InputAction.CallbackContext context) =>
            OnMouseDownPerformed(context, Helpers.LeftMouseInputId);

        private void OnLeftMouseButtonUpEvent(InputAction.CallbackContext context) =>
            OnMouseUpPerformed(context, Helpers.LeftMouseInputId);

        private void OnRightMouseButtonDownEvent(InputAction.CallbackContext context) =>
            OnMouseDownPerformed(context, Helpers.RightMouseInputId);

        private void OnRightMouseButtonUpEvent(InputAction.CallbackContext context) =>
            OnMouseUpPerformed(context, Helpers.RightMouseInputId);

        private void OnMiddleMouseButtonDownEvent(InputAction.CallbackContext context) =>
            OnMouseDownPerformed(context, Helpers.MiddleMouseInputId);

        private void OnMiddleMouseButtonUpEvent(InputAction.CallbackContext context) =>
            OnMouseUpPerformed(context, Helpers.MiddleMouseInputId);

        private void OnMousePositionEvent(InputAction.CallbackContext context)
        {
            m_MousePosition = context.ReadValue<Vector2>();
            foreach (var mouseAnalyzer in m_GesturesContainer.MouseGestureAnalyzers)
                mouseAnalyzer.UpdateMousePosition(m_MousePosition);
        }

        private void OnMouseDownPerformed(InputAction.CallbackContext context, int buttonIndex)
        {
            if (!CanBeHandled(context))
                return;

            // Debug.Log($"Mouse down {buttonIndex}");

            m_PressedButtons.Add(buttonIndex);
            m_ActiveDevice = context.control.device;
            if (TryGetOrCreateGestureAtPosition(m_MousePosition, out var analyzer) && analyzer is IMouseGestureAnalyzer mouseGestureAnalyzer)
            {
                mouseGestureAnalyzer.UpdateMousePosition(m_MousePosition);
                mouseGestureAnalyzer.MouseButtonDown(buttonIndex);
            }

            m_MouseDownEvent?.Invoke(buttonIndex);
        }

        private void OnMouseUpPerformed(InputAction.CallbackContext context, int buttonIndex)
        {
            if (!CanBeHandled(context))
                return;

            // Debug.Log($"Mouse up {buttonIndex}");

            m_PressedButtons.Remove(buttonIndex);
            if (m_PressedButtons.Count == 0)
                m_ActiveDevice = null;

            foreach (var mouseAnalyzer in m_GesturesContainer.MouseGestureAnalyzers)
                mouseAnalyzer.MouseButtonUp(buttonIndex);

            m_GesturesContainer.RemoveUnusedGestures();
            m_MouseUpEvent?.Invoke(buttonIndex);
        }

        private void OnMouseScrollEvent(InputAction.CallbackContext context)
        {
            if (!CanBeHandled(context))
                return;

            var scrollData = context.ReadValue<Vector2>();
            if (!m_CameraTracer.IsOverUI(m_MousePosition))
                m_GlobalScrollEvent?.Invoke(scrollData, m_MousePosition);
        }

        #endregion

        #region Pointer Gestures

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
            if (TryGetOrCreateGestureAtPosition(data.Position, out var analyzer) && analyzer is IPointerGestureAnalyzer pointerGestureAnalyzer)
                pointerGestureAnalyzer.CreatePointer(data.InputId, data.Position);
        }

        /// <summary>
        /// Just creates a gesture analyzer for an object under given position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="analyzer"></param>
        /// <returns></returns>
        private bool TryGetOrCreateGestureAtPosition(Vector2 position, out IGestureAnalyzer analyzer)
        {
            if (!m_CameraTracer.IsOverUI(position))
            {
                var goodCameras = GetTraceableCameras(position);
                if (goodCameras.Count != 0)
                {
                    var cam = goodCameras[0];
                    var cameraInteractionObj = cam.GetComponent<InteractionObjectBase>();
                    var interactionObjectFound = m_CameraTracer.TryTraceInteractionObject(cam, position, out var interactionObject);
                    if (interactionObjectFound && TryGetOrCreateGestureAnalyzer(interactionObject, cam, out analyzer))
                        return true;
                    if (TryGetOrCreateGestureAnalyzer(cameraInteractionObj, cam, out analyzer))
                        return true;
                }
            }

            analyzer = null;
            return false;
        }

        private void OnProcessedPointerUpdate(PointerInput data)
        {
            //Just update data. everything should be inside subscriptions
            foreach (var gesture in m_GesturesContainer.PointerGestureAnalyzers)
            {
                if (gesture.TryGetPointer(data.InputId, out var pointer))
                    pointer.UpdatePositionAndRaise(data.Position);
            }
        }

        private void OnProcessedPointerUp(PointerInput data)
        {
            foreach (var gesture in m_GesturesContainer.PointerGestureAnalyzers)
            {
                if (gesture.HasPointer(data.InputId))
                    gesture.RemovePointer(data.InputId, data.Position);
            }

            m_GesturesContainer.RemoveUnusedGestures();
        }

        #endregion

        #region Utils

        private bool TryGetOrCreateGestureAnalyzer(InteractionObjectBase interactionObj, Camera cam, out IGestureAnalyzer gestureAnalyzer)
        {
            gestureAnalyzer = null;
            if (interactionObj == null)
                return false;

            if (!m_GesturesContainer.TryGetGestureAnalyzer(interactionObj, out gestureAnalyzer))
            {
                //check if we already have solo interaction and prevent creating new gesture analyzers
                if (m_GesturesContainer.AllGestures.Any(gestureAnalyzer => gestureAnalyzer.IsSoloGesture))
                    return false;
                //check if this object is solo interaction and prevent its gesture analyzer creation if another gestures exist
                var analyzer = interactionObj.CreateAnalyzer(cam);
                if (analyzer.IsSoloGesture && m_GesturesContainer.HasGestures)
                    return false;

                gestureAnalyzer = analyzer;
                m_GesturesContainer.AddGesture(gestureAnalyzer);
                return true;
            }

            return true;
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

        private List<Camera> GetTraceableCameras(Vector2 position)
        {
            if (m_Cameras.Count == 0)
                Debug.LogWarning("No traceable cameras. Did you forget to add one?");

            return m_Cameras.Where(cam => m_CameraTracer.CanBeTraced(cam, position)).ToList();
        }

        public static void RunEventOnCamera(Camera cam, InteractionEvent evt, InteractionEventArgs args)
        {
            if (cam != null && cam.TryGetComponent<InteractionObjectBase>(out var cameraInteractionObject))
                cameraInteractionObject.RunEvent(evt, args);
        }

        #endregion

        private void Update()
        {
            if (m_UpdateCausesGestureUpdates)
            {
                foreach (var gesture in m_GesturesContainer.AllGestures)
                    gesture.UpdateAnalyzer();
            }
        }
    }
}