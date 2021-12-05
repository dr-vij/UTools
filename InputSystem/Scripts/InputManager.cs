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
        /// This parameters decides how far should drag perform to call Drag. If distance is less, press will be performed
        /// </summary>
        [SerializeField] private int mDragOrPressTriggerDistance = 0;
        [SerializeField] private bool mRaiseDragOnUpdates = true;

        private InputDataContainer mInputData = new InputDataContainer();
        private Actions mActions;
        private List<Camera> mCameras = new List<Camera>();
        private CameraTracer mCameraTracer = new CameraTracer();

        public CameraTracer CameraTracer => mCameraTracer;

        public void RegisterCamera(Camera cam)
        {
            if (!mCameras.Contains(cam))
                mCameras.Add(cam);
            //We sort cameras by depth for now.
            mCameras.Sort((cam1, cam2) => cam1.depth.CompareTo(cam2.depth));
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
                if (!mCameraTracer.IsOverUI(screenCoord))
                {
                    foreach(var camera in mCameras)
                    {
                        if (mCameraTracer.CanBeTraced(camera, screenCoord))
                        {
                            if (mInputData.InteractionCamera == null)
                                mInputData.InteractionCamera = camera;

                            //TODO: Think if we really need to break after first successfull trace.
                            if (mCameraTracer.TryTraceInteractionObject(camera, screenCoord, out var interactionObject))
                            {
                                mInputData.InteractionCamera = camera;
                                mInputData.InteractionObject = interactionObject;
                                args.SetCamera(camera);
                                interactionObject.RunEvent(ObjectInteractionEvents.ObjectPointerGrabStartEvent, args);
                                break;
                            }
                            break;
                        }
                    }
                }
                //TODO: Raise global pointer down here
                //------------------------------------
            }
            else
            {
                Debug.LogError(mUnsopportedPointerMsg);
            }
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
                        var dragArgs = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerPreviousPosition, mInputData.InteractionCamera);
                        if (mInputData.InteractionObject != null)
                            mInputData.InteractionObject.RunEvent(ObjectInteractionEvents.ObjectPointerDragEndEvent, dragArgs);
                        //TODO: global drag end here;
                    }
                    else
                    {
                        //Press event handling
                        //TextDebugger.Instance.LogColored($"Pointer press at {mInputData.PointerCurrentPosition}", Color.magenta);
                        var pressArgs = new PointerInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.InteractionCamera);
                        if (mInputData.InteractionObject != null)
                            mInputData.InteractionObject.RunEvent(ObjectInteractionEvents.ObjectPointerPressEvent, pressArgs);
                        //TODO: global press here
                    }

                    //Up/grab end event handling
                    //TextDebugger.Instance.Log($"Pointer up at {mInputData.PointerCurrentPosition}");
                    var upArgs = new PointerInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.InteractionCamera);
                    if (mInputData.InteractionObject != null)
                        mInputData.InteractionObject.RunEvent(ObjectInteractionEvents.ObjectPointerGrabEndEvent, upArgs);
                    //TODO: global up here
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
                    var dragStartArgs = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerDownPosition, mInputData.InteractionCamera);
                    if (mInputData.InteractionObject != null)
                        mInputData.InteractionObject.RunEvent(ObjectInteractionEvents.ObjectPointerDragStartEvent, dragStartArgs);
                    //TODO: global drag start here

                }
                else if (mInputData.IsDragTriggered)
                {
                    //Drag handling
                    //TextDebugger.Instance.LogColored($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}", Color.yellow);
                    var dragArgs = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerPreviousPosition, mInputData.InteractionCamera);
                    if (mInputData.InteractionObject != null)
                        mInputData.InteractionObject.RunEvent(ObjectInteractionEvents.ObjectPointerDragEvent, dragArgs);
                    //TODO: global drag here
                }
            }

            //TODO: global Pointer Move
            //TextDebugger.Instance.Log($"Pointer Move at: {currentPosition}");
        }

        private void Update()
        {
            if (mRaiseDragOnUpdates && mInputData.IsPointerDownTriggered && mInputData.IsDragTriggered)
            {
                //TextDebugger.Instance.Log($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, currend delta: {mInputData.PointerCurrentDelta}");
                mInputData.PointerPreviousPosition = mInputData.PointerCurrentPosition;
                var dragArgs = new PointerDragInteractionEventArgs(mInputData.PointerCurrentPosition, mInputData.PointerPreviousPosition, mInputData.InteractionCamera);
                if (mInputData.InteractionObject!=null)
                    mInputData.InteractionObject.RunEvent(ObjectInteractionEvents.ObjectPointerDragEvent, dragArgs);

                //TODO: global drag here
            }
        }

        #region Helpers

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

        #endregion
    }
}