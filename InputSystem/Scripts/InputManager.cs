using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ViJTools
{
    public class InteractionPointer
    {
        public int ID { get; private set; }
        public Vector2 PointerStartPosition { get; private set; }
        public Vector2 PointerPosition { get; private set; }
        public Vector2 PointerPrevPosition { get; private set; }
        public bool IsPrimary { get; private set; }

        public event Action<InteractionPointer> PositionChangeEvent;

        public PointerInput Data { get; private set; }

        public InteractionPointer(PointerInput data, bool isPrimary)
        {
            PointerStartPosition = data.Position;
            PointerPosition = data.Position;
            PointerPrevPosition = data.Position;
            IsPrimary = isPrimary;
            ID = data.InputId;
        }

        public void UpdateData(PointerInput newData)
        {
            if (ID != newData.InputId)
                Debug.LogError("WRONG DATA");
            Data = newData;
            PointerPrevPosition = PointerPosition;
            PointerPosition = newData.Position;
            PositionChangeEvent?.Invoke(this);
        }
    }

    public class SimpleGestureAnalizer : DisposableObject
    {
        private static int m_IdCounter = -1;

        private InteractionPointer[] m_ActivePointers = new InteractionPointer[10];

        public Camera InteractionCamera { get; private set; }

        public int Id { get; protected set; }

        public InteractionObject InteractionObject { get; private set; }

        public int PointersCount { get; private set; }

        public SimpleGestureAnalizer(InteractionObject initiatorObject, Camera initiatorCamera)
        {
            InteractionObject = initiatorObject;
            InteractionCamera = initiatorCamera;

            Id = ++m_IdCounter;
        }

        public void AddTrackingPointer(InteractionPointer pointer)
        {
            var index = GetFirstFreePointerIndex();
            if (index == -1)
                return;

            PointersCount++;
            m_ActivePointers[index] = pointer;
            pointer.PositionChangeEvent += OnPointerUpdate;

            //POINTER i DOWN
            if (PointersCount == 1)
            {
                //Grab start
                var args = new PointerInteractionEventArgs(pointer.PointerPosition);
                InteractionObject.RunEvent(InteractionEvents.PointerGrabStartEvent, args);
            }
            else if (PointersCount == 2)
            {
                
            }

            Debug.Log($"Gesture {Id} TestPointer down {index}, position {pointer.PointerPosition}");
        }

        private void OnPointerUpdate(InteractionPointer pointer)
        {
            var index = Array.IndexOf(m_ActivePointers, pointer);
            if (index == -1)
                return;

            if (PointersCount == 1)
            {
                //Pointer drag
            }
            else if (PointersCount == 2)
            {
                //Double pointer drag
            }

            //POINTER i UPDATE
            Debug.Log($"Gesture {Id} TestPointer update {index}, position {pointer.PointerPosition}");
        }

        public void RemoveTrackingPointer(InteractionPointer pointer)
        {
            var index = Array.IndexOf(m_ActivePointers, pointer);
            if (index == -1)
                return;

            PointersCount--;
            var p = m_ActivePointers[index];
            p.PositionChangeEvent -= OnPointerUpdate;
            m_ActivePointers[index] = null;

            if (PointersCount == 0)
            {
                var args = new PointerInteractionEventArgs(pointer.PointerPosition);
                InteractionObject.RunEvent(InteractionEvents.PointerGrabEndEvent, args);
            }

            //POINTER i UP;
            Debug.Log($"Gesture {Id} TestPointer up {index}, position {pointer.PointerPosition}");
        }

        public void Update()
        {
            for (int i = 0; i < 10; i++)
                if (m_ActivePointers[i] != null)
                    m_ActivePointers[i].UpdateData(m_ActivePointers[i].Data);
        }

        private int GetFirstFreePointerIndex()
        {
            for (int i = 0; i < 10; i++)
                if (m_ActivePointers[i] == null)
                    return i;
            return -1;
        }
    }

    public class InputManager : SingletonMonobehaviour<InputManager>
    {
        private string m_UnsopportedPointerMsg = "Unknown control device. Cannot read its position. check if control device is Pointer";

        /// <summary>
        /// This parameters determines how far should drag perform to be to start drag. If distance is less, press will be performed
        /// </summary>
        [SerializeField] private int m_DragOrPressTriggerDistance = 0;

        /// <summary>
        /// Raise drags on update if true
        /// </summary>
        [SerializeField] private bool m_RaiseDragOnUpdates = true;

        private InputDataContainer m_InputData = new InputDataContainer();
        private Actions m_Actions;
        private List<Camera> m_Cameras = new List<Camera>();
        private CameraTracer m_CameraTracer = new CameraTracer();

        private InputDevice m_ActiveDevice;
        private HashSet<int> m_ActiveTouches = new HashSet<int>();
        private bool m_IsMouseOrPenInputStarted;

        private Dictionary<int, InteractionPointer> mActivePointers = new Dictionary<int, InteractionPointer>();
        private Dictionary<InteractionObject, SimpleGestureAnalizer> mActiveGestures = new Dictionary<InteractionObject, SimpleGestureAnalizer>();

        public CameraTracer CameraTracer => m_CameraTracer;

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
            m_Actions = new Actions();
            m_Actions.GestureActions.Enable();

            m_Actions.GestureActions.Pointer.performed += OnPointerEvent;
            m_Actions.GestureActions.Pointer.canceled += OnPointerEvent;
        }

        private void OnPointerEvent(InputAction.CallbackContext context)
        {
            //WE DO NOT ALLOW TO MIX DIFFERENT DEVICES IN THIS COMBINATION OF GESTURES
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
                            OnRawPointerDrag(data);
                        }
                        else
                        {
                            m_ActiveTouches.Add(data.InputId);
                            m_ActiveDevice = context.control.device;
                            //POINTER DOWN
                            OnRawPointerDown(data);
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
                            OnRawPointerUp(data);
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
                            OnRawPointerDrag(data);
                        }
                        else
                        {
                            m_IsMouseOrPenInputStarted = true;
                            m_ActiveDevice = context.control.device;
                            //POINTER DOWN
                            OnRawPointerDown(data);
                        }
                    }
                    else
                    {
                        if (m_IsMouseOrPenInputStarted)
                        {
                            m_IsMouseOrPenInputStarted = false;
                            m_ActiveDevice = null;
                            //POINTER UP
                            OnRawPointerUp(data);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Cant be handled");
            }
        }

        private void OnRawPointerDown(PointerInput data)
        {
            var interactionPointer = new InteractionPointer(data, mActivePointers.Count == 0);

            //TODO: INPUT INTERCEPTOR CAN BE HERE
            mActivePointers.Add(data.InputId, interactionPointer);
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

                            gesture = new SimpleGestureAnalizer(interactionObject, camera);
                            mActiveGestures.Add(interactionObject, gesture);
                        }

                        gesture?.AddTrackingPointer(interactionPointer);
                    }
                    else
                    {
                        //TODO: CAMERA INTERACTION CAN BE HERE
                    }
                }
            }
        }

        private void OnRawPointerDrag(PointerInput data)
        {
            //Just update data. everything should be inside subscribtions
            mActivePointers[data.InputId].UpdateData(data);
        }

        private void OnRawPointerUp(PointerInput data)
        {
            var pointer = mActivePointers[data.InputId];
            List<InteractionObject> toRemoveList = new List<InteractionObject>();
            foreach (var gesture in mActiveGestures)
            {
                gesture.Value.RemoveTrackingPointer(pointer);
                if (gesture.Value.PointersCount == 0)
                    toRemoveList.Add(gesture.Key);
            }
            foreach (var gesture in toRemoveList)
                mActiveGestures.Remove(gesture);

            mActivePointers.Remove(data.InputId);
        }

        private void Set2dPos(Vector2 pos, Transform t)
        {
            var ray = m_Cameras[0].ScreenPointToRay(pos);
            var plane = new Plane(Vector3.back, 0);
            plane.Raycast(ray, out var enter);
            var point3d = ray.GetPoint(enter);
            t.transform.position = point3d;
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

        /// <summary>
        /// Unsubscribe from all events and dispose Actions
        /// </summary>
        private void OnDisable()
        {
            //mActions.GestureActions.PointerStart.performed -= OnPointerPerformed;
            //mActions.GestureActions.PointerStart.canceled -= OnPointerCanceled;
            //mActions.GestureActions.PointerMove.performed -= OnPointerMove;
            m_Actions.Dispose();
        }

        /// <summary>
        /// Called when pointer is down
        /// </summary>
        /// <param name="context"></param>
        private void OnPointerPerformed(InputAction.CallbackContext context)
        {
            if (TryReadPointerPosition(context, out var pointerPosition))
            {
                m_InputData.StartInput();
                m_InputData.PointerDownPosition = pointerPosition;
                m_InputData.PointerCurrentPosition = pointerPosition;
                m_InputData.PointerPreviousPosition = pointerPosition;

                //Pointer grab(object) and Pointer down (global) events
                //TextDebugger.Instance.LogColored($"Pointer down at {mInputData.PointerCurrentPosition}", Color.blue);
                var screenCoord = m_InputData.PointerDownPosition;
                var args = new PointerInteractionEventArgs(screenCoord);
                var evt = InteractionEvents.PointerGrabStartEvent;
                if (!m_CameraTracer.IsOverUI(screenCoord))
                {
                    var goodCameras = GetTracebleCameras(screenCoord);
                    if (goodCameras.Count != 0)
                    {
                        var goodCam = goodCameras[0];
                        m_InputData.InteractionCamera = goodCam;
                        args.SetCamera(goodCam);

                        if (m_CameraTracer.TryTraceInteractionObject(goodCam, screenCoord, out var interactionObject))
                        {
                            m_InputData.InteractionObject = interactionObject;
                            interactionObject.RunEvent(evt, args);
                        }

                        RunOnCamera(goodCam, evt, args);
                    }
                }
            }
            else
            {
                Debug.LogError(m_UnsopportedPointerMsg);
            }
        }

        private void RunOnCamera(Camera cam, InteractionEvent evt, InteractionEventArgs args)
        {
            if (cam != null && cam.TryGetComponent<InteractionObject>(out var cameraInteractionObject))
                cameraInteractionObject.RunEvent(evt, args);
        }

        private List<Camera> GetTracebleCameras(Vector2 position)
        {
            if (m_Cameras.Count == 0)
                Debug.LogWarning("No traceble cameras. Did you forget to add one?");

            return m_Cameras.Where(camera => m_CameraTracer.CanBeTraced(camera, position)).ToList();
        }

        /// <summary>
        /// Called when pointer is up
        /// </summary>
        /// <param name="context"></param>
        private void OnPointerCanceled(InputAction.CallbackContext context)
        {
            if (TryReadPointerPosition(context, out var pointerPosition))
            {
                if (m_InputData.IsPointerDownTriggered)
                {
                    m_InputData.PointerPreviousPosition = m_InputData.PointerCurrentPosition;
                    m_InputData.PointerCurrentPosition = pointerPosition;

                    if (m_InputData.IsDragTriggered)
                    {
                        //Drag end handling
                        //TextDebugger.Instance.LogColored($"Drag end at {mInputData.PointerCurrentPosition}", Color.red);
                        var args = new PointerDragInteractionEventArgs(m_InputData.PointerCurrentPosition, m_InputData.PointerPreviousPosition, m_InputData.InteractionCamera);
                        var evt = InteractionEvents.PointerGrabEndEvent;
                        if (m_InputData.InteractionObject != null)
                            m_InputData.InteractionObject.RunEvent(evt, args);
                        RunOnCamera(m_InputData.InteractionCamera, evt, args);
                    }
                    else
                    {
                        //Press event handling
                        //TextDebugger.Instance.LogColored($"Pointer press at {mInputData.PointerCurrentPosition}", Color.magenta);
                        var args = new PointerInteractionEventArgs(m_InputData.PointerCurrentPosition, m_InputData.InteractionCamera);
                        var evt = InteractionEvents.PointerPressEvent;
                        if (m_InputData.InteractionObject != null)
                            m_InputData.InteractionObject.RunEvent(evt, args);
                        RunOnCamera(m_InputData.InteractionCamera, evt, args);
                    }

                    //Up/grab end event handling
                    //TextDebugger.Instance.Log($"Pointer up at {mInputData.PointerCurrentPosition}");
                    {
                        var args = new PointerInteractionEventArgs(m_InputData.PointerCurrentPosition, m_InputData.InteractionCamera);
                        var evt = InteractionEvents.PointerGrabEndEvent;
                        if (m_InputData.InteractionObject != null)
                            m_InputData.InteractionObject.RunEvent(evt, args);
                        RunOnCamera(m_InputData.InteractionCamera, evt, args);
                    }
                }
            }
            else
            {
                Debug.LogError(m_UnsopportedPointerMsg);
            }

            m_InputData.StopInput();
        }

        /// <summary>
        /// Called when pointer have changed its position
        /// </summary>
        /// <param name="context"></param>
        private void OnPointerMove(InputAction.CallbackContext context)
        {
            var currentPosition = context.ReadValue<Vector2>();

            if (m_InputData.IsPointerDownTriggered)
            {
                m_InputData.PointerPreviousPosition = m_InputData.PointerCurrentPosition;
                m_InputData.PointerCurrentPosition = currentPosition;

                if (!m_InputData.IsDragTriggered && m_InputData.PointerTotalDelta.magnitude > m_DragOrPressTriggerDistance)
                {
                    //Drag start handling
                    //TextDebugger.Instance.LogColored($"Drag start at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}, Total delta magnitude {mInputData.PointerTotalDelta.magnitude}", Color.green);
                    m_InputData.TriggerDrag();
                    var args = new PointerDragInteractionEventArgs(m_InputData.PointerCurrentPosition, m_InputData.PointerDownPosition, m_InputData.InteractionCamera);
                    var evt = InteractionEvents.PointerDragStartEvent;
                    if (m_InputData.InteractionObject != null)
                        m_InputData.InteractionObject.RunEvent(evt, args);

                    RunOnCamera(m_InputData.InteractionCamera, evt, args);
                }
                else if (m_InputData.IsDragTriggered)
                {
                    //Drag handling
                    //TextDebugger.Instance.LogColored($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}", Color.yellow);
                    var args = new PointerDragInteractionEventArgs(m_InputData.PointerCurrentPosition, m_InputData.PointerPreviousPosition, m_InputData.InteractionCamera);
                    var evt = InteractionEvents.PointerDragEvent;
                    if (m_InputData.InteractionObject != null)
                        m_InputData.InteractionObject.RunEvent(evt, args);

                    RunOnCamera(m_InputData.InteractionCamera, evt, args);
                }
            }

            //Pointer move handling
            {
                if (!m_CameraTracer.IsOverUI(currentPosition))
                {
                    var cameras = GetTracebleCameras(currentPosition);
                    if (cameras.Count != 0)
                    {
                        var cam = cameras[0];
                        var evt = InteractionEvents.PointerMoveEvent;
                        var args = new PointerInteractionEventArgs(currentPosition, cam);

                        if (m_CameraTracer.TryTraceInteractionObject(cam, currentPosition, out var moveInteractionObj))
                            moveInteractionObj.RunEvent(evt, args);

                        RunOnCamera(cam, evt, args);
                    }
                }
            }
        }

        private void Update()
        {
            //Handle optional drag 
            if (m_RaiseDragOnUpdates && m_InputData.IsPointerDownTriggered && m_InputData.IsDragTriggered)
            {
                //TextDebugger.Instance.Log($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, currend delta: {mInputData.PointerCurrentDelta}");
                m_InputData.PointerPreviousPosition = m_InputData.PointerCurrentPosition;
                var args = new PointerDragInteractionEventArgs(m_InputData.PointerCurrentPosition, m_InputData.PointerPreviousPosition, m_InputData.InteractionCamera);
                var evt = InteractionEvents.PointerDragEvent;
                if (m_InputData.InteractionObject != null)
                    m_InputData.InteractionObject.RunEvent(evt, args);

                RunOnCamera(m_InputData.InteractionCamera, evt, args);
            }
        }

        private bool TryReadPointerPosition(InputAction.CallbackContext context, out Vector2 pointerPosition)
        {
            if (context.control.device is Pointer pointer)
            {
                pointerPosition = pointer.position.ReadValue();
                return true;
            }

            //Default unsuccessfull return
            pointerPosition = Vector2.zero;
            return false;
        }
    }
}