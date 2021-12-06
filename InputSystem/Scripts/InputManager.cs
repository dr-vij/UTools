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
            mActions.GestureActions.PointerStart.performed += OnPointerPerformed;
            mActions.GestureActions.PointerStart.canceled += OnPointerCanceled;
            mActions.GestureActions.PointerMove.performed += OnPointerMove;
        }

        private void Awake()
        {
            TextDebugger.Instance.Log(Screen.dpi);
        }

        /// <summary>
        /// Unsubscribe from all events and dispose Actions
        /// </summary>
        private void OnDisable()
        {
            mActions.GestureActions.PointerStart.performed -= OnPointerPerformed;
            mActions.GestureActions.PointerStart.canceled -= OnPointerCanceled;
            mActions.GestureActions.PointerMove.performed -= OnPointerMove;
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