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
        private static int mIdCounter = -1;

        public int Id { get; protected set; }

        private InteractionPointer[] mActivePointers = new InteractionPointer[10];

        public InteractionObject InteractionObject { get; private set; }

        public Camera InteractionCamera { get; private set; }

        public int PointersCount => mActivePointers.Where(c => c != null).Count();

        public SimpleGestureAnalizer(InteractionObject initiatorObject, Camera initiatorCamera)
        {
            InteractionObject = initiatorObject;
            InteractionCamera = initiatorCamera;

            Id = ++mIdCounter;
        }

        public void AddTrackingPointer(InteractionPointer pointer)
        {
            for (int i = 0; i < 10; i++)
            {
                if (mActivePointers[i] == null)
                {
                    mActivePointers[i] = pointer;
                    pointer.PositionChangeEvent += OnPointerUpdate;

                    //POINTER i DOWN
                    Debug.Log($"Gesture {Id} TestPointer down {i}");
                    break;
                }
            }
        }

        private void OnPointerUpdate(InteractionPointer pointer)
        {
            for (int i = 0; i < 10; i++)
            {
                if (mActivePointers[i] == pointer)
                {
                    //POINTER i UPDATE
                    Debug.Log($"Gesture {Id} TestPointer update {i}");
                    break;
                }
            }
        }

        public void RemoveTrackingPointer(InteractionPointer pointer)
        {
            for (int i = 0; i < 10; i++)
            {
                if (mActivePointers[i] == pointer)
                {
                    var p = mActivePointers[i];
                    p.PositionChangeEvent -= OnPointerUpdate;
                    mActivePointers[i] = null;
                    //POINTER i UP;
                    Debug.Log($"Gesture {Id} TestPointer up {i}");
                    break;
                }
            }
        }

        public void Update()
        {
            for(int i = 0; i< 10; i++)
                if (mActivePointers[i] != null)
                    mActivePointers[i].UpdateData(mActivePointers[i].Data);
        }
    }

    public class InputManager : SingletonMonobehaviour<InputManager>
    {
        private string mUnsopportedPointerMsg = "Unknown control device. Cannot read its position. check if control device is Pointer";

        /// <summary>
        /// This parameters determines how far should drag perform to be to start drag. If distance is less, press will be performed
        /// </summary>
        [SerializeField] private int mDragOrPressTriggerDistance = 0;

        /// <summary>
        /// Raise drags on update if true
        /// </summary>
        [SerializeField] private bool mRaiseDragOnUpdates = true;

        private InputDataContainer mInputData = new InputDataContainer();
        private Actions mActions;
        private List<Camera> mCameras = new List<Camera>();
        private CameraTracer mCameraTracer = new CameraTracer();

        public CameraTracer CameraTracer => mCameraTracer;

        public void RegisterCamera(Camera cam)
        {
            if (!mCameras.Contains(cam))
            {
                mCameras.Add(cam);
                if (!cam.TryGetComponent<InteractionObject>(out var interactionObject))
                    cam.gameObject.AddComponent<InteractionObject>();
                mCameras.Sort((cam1, cam2) => cam1.depth.CompareTo(cam2.depth));
            }
        }

        /// <summary>
        /// Create Actions and subscribe it's events
        /// </summary>
        private void OnEnable()
        {
            mActions = new Actions();
            mActions.GestureActions.Enable();

            mActions.GestureActions.Pointer.performed += OnPointerEvent;
            mActions.GestureActions.Pointer.canceled += OnPointerEvent;
        }

        private InputDevice mActiveDevice;
        private HashSet<int> mActiveTouches = new HashSet<int>();
        private bool mIsMouseOrPenInputStarted;

        private Dictionary<int, InteractionPointer> mActivePointers = new Dictionary<int, InteractionPointer>();
        private Dictionary<InteractionObject, SimpleGestureAnalizer> mActiveGestures = new Dictionary<InteractionObject, SimpleGestureAnalizer>();

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
                        if (mActiveTouches.Contains(data.InputId))
                        {
                            //POINTER DRAG
                            OnRawPointerDrag(data);
                        }
                        else
                        {
                            mActiveTouches.Add(data.InputId);
                            mActiveDevice = context.control.device;
                            //POINTER DOWN
                            OnRawPointerDown(data);
                        }
                    }
                    else
                    {
                        if (mActiveTouches.Contains(data.InputId))
                        {
                            mActiveTouches.Remove(data.InputId);
                            if (mActiveTouches.Count == 0)
                                mActiveDevice = null;
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
                        if (mIsMouseOrPenInputStarted)
                        {
                            //POINTER DRAG
                            OnRawPointerDrag(data);
                        }
                        else
                        {
                            mIsMouseOrPenInputStarted = true;
                            mActiveDevice = context.control.device;
                            //POINTER DOWN
                            OnRawPointerDown(data);
                        }
                    }
                    else
                    {
                        if (mIsMouseOrPenInputStarted)
                        {
                            mIsMouseOrPenInputStarted = false;
                            mActiveDevice = null;
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
            if (!mCameraTracer.IsOverUI(data.Position))
            {
                var goodCameras = GetTracebleCameras(data.Position);
                if (goodCameras.Count != 0)
                {
                    var camera = goodCameras[0];
                    if (mCameraTracer.TryTraceInteractionObject(camera, data.Position, out var interactionObject))
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
            var ray = mCameras[0].ScreenPointToRay(pos);
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
            return mActiveDevice == null || mActiveDevice.GetType() == context.control.device.GetType();
        }

        /// <summary>
        /// Unsubscribe from all events and dispose Actions
        /// </summary>
        private void OnDisable()
        {
            //mActions.GestureActions.PointerStart.performed -= OnPointerPerformed;
            //mActions.GestureActions.PointerStart.canceled -= OnPointerCanceled;
            //mActions.GestureActions.PointerMove.performed -= OnPointerMove;
            mActions.Dispose();
        }

        /// <summary>
        /// Called when pointer is down
        /// </summary>
        /// <param name="context"></param>
        private void OnPointerPerformed(InputAction.CallbackContext context)
        {
            if (TryReadPointerPosition(context, out var pointerPosition))
            {
                mInputData.StartInput();
                mInputData.PointerDownPosition = pointerPosition;
                mInputData.PointerCurrentPosition = pointerPosition;
                mInputData.PointerPreviousPosition = pointerPosition;

                //Pointer grab(object) and Pointer down (global) events
                //TextDebugger.Instance.LogColored($"Pointer down at {mInputData.PointerCurrentPosition}", Color.blue);
                var screenCoord = mInputData.PointerDownPosition;
                var args = new PointerInteractionEventArgs(screenCoord);
                var evt = InteractionEvents.PointerGrabStartEvent;
                if (!mCameraTracer.IsOverUI(screenCoord))
                {
                    var goodCameras = GetTracebleCameras(screenCoord);
                    if (goodCameras.Count != 0)
                    {
                        var goodCam = goodCameras[0];
                        mInputData.InteractionCamera = goodCam;
                        args.SetCamera(goodCam);

                        if (mCameraTracer.TryTraceInteractionObject(goodCam, screenCoord, out var interactionObject))
                        {
                            mInputData.InteractionObject = interactionObject;
                            interactionObject.RunEvent(evt, args);
                        }

                        RunOnCamera(goodCam, evt, args);
                    }
                }
            }
            else
            {
                Debug.LogError(mUnsopportedPointerMsg);
            }
        }

        private void RunOnCamera(Camera cam, InteractionEvent evt, InteractionEventArgs args)
        {
            if (cam != null && cam.TryGetComponent<InteractionObject>(out var cameraInteractionObject))
                cameraInteractionObject.RunEvent(evt, args);
        }

        private List<Camera> GetTracebleCameras(Vector2 position)
        {
            if (mCameras.Count == 0)
                Debug.LogWarning("No traceble cameras. Did you forget to add one?");

            return mCameras.Where(camera => mCameraTracer.CanBeTraced(camera, position)).ToList();
        }

        /// <summary>
        /// Called when pointer is up
        /// </summary>
        /// <param name="context"></param>
        private void OnPointerCanceled(InputAction.CallbackContext context)
        {
            if (TryReadPointerPosition(context, out var pointerPosition))
            {
                if (mInputData.IsPointerDownTriggered)
                {
                    mInputData.PointerPreviousPosition = mInputData.PointerCurrentPosition;
                    mInputData.PointerCurrentPosition = pointerPosition;

                    if (mInputData.IsDragTriggered)
                    {
                        //Drag end handling
                        //TextDebugger.Instance.LogColored($"Drag end at {mInputData.PointerCurrentPosition}", Color.red);
                        var args = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerPreviousPosition, mInputData.InteractionCamera);
                        var evt = InteractionEvents.PointerGrabEndEvent;
                        if (mInputData.InteractionObject != null)
                            mInputData.InteractionObject.RunEvent(evt, args);
                        RunOnCamera(mInputData.InteractionCamera, evt, args);
                    }
                    else
                    {
                        //Press event handling
                        //TextDebugger.Instance.LogColored($"Pointer press at {mInputData.PointerCurrentPosition}", Color.magenta);
                        var args = new PointerInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.InteractionCamera);
                        var evt = InteractionEvents.PointerPressEvent;
                        if (mInputData.InteractionObject != null)
                            mInputData.InteractionObject.RunEvent(evt, args);
                        RunOnCamera(mInputData.InteractionCamera, evt, args);
                    }

                    //Up/grab end event handling
                    //TextDebugger.Instance.Log($"Pointer up at {mInputData.PointerCurrentPosition}");
                    {
                        var args = new PointerInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.InteractionCamera);
                        var evt = InteractionEvents.PointerGrabEndEvent;
                        if (mInputData.InteractionObject != null)
                            mInputData.InteractionObject.RunEvent(evt, args);
                        RunOnCamera(mInputData.InteractionCamera, evt, args);
                    }
                }
            }
            else
            {
                Debug.LogError(mUnsopportedPointerMsg);
            }

            mInputData.StopInput();
        }

        /// <summary>
        /// Called when pointer have changed its position
        /// </summary>
        /// <param name="context"></param>
        private void OnPointerMove(InputAction.CallbackContext context)
        {
            var currentPosition = context.ReadValue<Vector2>();

            if (mInputData.IsPointerDownTriggered)
            {
                mInputData.PointerPreviousPosition = mInputData.PointerCurrentPosition;
                mInputData.PointerCurrentPosition = currentPosition;

                if (!mInputData.IsDragTriggered && mInputData.PointerTotalDelta.magnitude > mDragOrPressTriggerDistance)
                {
                    //Drag start handling
                    //TextDebugger.Instance.LogColored($"Drag start at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}, Total delta magnitude {mInputData.PointerTotalDelta.magnitude}", Color.green);
                    mInputData.TriggerDrag();
                    var args = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerDownPosition, mInputData.InteractionCamera);
                    var evt = InteractionEvents.PointerDragStartEvent;
                    if (mInputData.InteractionObject != null)
                        mInputData.InteractionObject.RunEvent(evt, args);

                    RunOnCamera(mInputData.InteractionCamera, evt, args);
                }
                else if (mInputData.IsDragTriggered)
                {
                    //Drag handling
                    //TextDebugger.Instance.LogColored($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}", Color.yellow);
                    var args = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerPreviousPosition, mInputData.InteractionCamera);
                    var evt = InteractionEvents.PointerDragEvent;
                    if (mInputData.InteractionObject != null)
                        mInputData.InteractionObject.RunEvent(evt, args);

                    RunOnCamera(mInputData.InteractionCamera, evt, args);
                }
            }

            //Pointer move handling
            {
                if (!mCameraTracer.IsOverUI(currentPosition))
                {
                    var cameras = GetTracebleCameras(currentPosition);
                    if (cameras.Count != 0)
                    {
                        var cam = cameras[0];
                        var evt = InteractionEvents.PointerMoveEvent;
                        var args = new PointerInteractionEventArgs(currentPosition, cam);

                        if (mCameraTracer.TryTraceInteractionObject(cam, currentPosition, out var moveInteractionObj))
                            moveInteractionObj.RunEvent(evt, args);

                        RunOnCamera(cam, evt, args);
                    }
                }
            }
        }

        private void Update()
        {
            //Handle optional drag 
            if (mRaiseDragOnUpdates && mInputData.IsPointerDownTriggered && mInputData.IsDragTriggered)
            {
                //TextDebugger.Instance.Log($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, currend delta: {mInputData.PointerCurrentDelta}");
                mInputData.PointerPreviousPosition = mInputData.PointerCurrentPosition;
                var args = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerPreviousPosition, mInputData.InteractionCamera);
                var evt = InteractionEvents.PointerDragEvent;
                if (mInputData.InteractionObject != null)
                    mInputData.InteractionObject.RunEvent(evt, args);

                RunOnCamera(mInputData.InteractionCamera, evt, args);
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