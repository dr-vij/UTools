using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ViJTools
{
    public class InputManager : MonoBehaviour
    {
        private string mUnsopportedPointerMsg = "Unknown control device.Cannot read its position.check if control device is Pointer";

        /// <summary>
        /// This parameters decides how far should drag perform to call Drag. If distance is less, press will be performed
        /// </summary>
        [SerializeField] private int mDragOrPressTriggerDistance = 0;
        [SerializeField] private bool mRaiseDragOnUpdates = true;

        private InputDataContainer mInputData = new InputDataContainer();
        private Actions mActions;
        private List<Camera> mCameras = new List<Camera>();

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

                //TODO: PointerDown here and capture dragged object
                //TextDebugger.Instance.LogColored($"Pointer down at {mInputData.PointerCurrentPosition}", Color.blue);
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


                    if (!mInputData.IsDragTriggered)
                    {
                        //TODO: Press here
                        TextDebugger.Instance.LogColored($"Pointer press at {mInputData.PointerCurrentPosition}", Color.magenta);
                    }
                    else
                    {
                        //TODO: Drag end here
                        TextDebugger.Instance.LogColored($"Drag end at {mInputData.PointerCurrentPosition}", Color.red);
                    }

                    //TODO: Pointer up here
                    //TextDebugger.Instance.Log($"Pointer up at {mInputData.PointerCurrentPosition}");
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
                    mInputData.TriggerDrag();
                    //TODO: Drag start here
                    TextDebugger.Instance.LogColored($"Drag start at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}, Total delta magnitude {mInputData.PointerTotalDelta.magnitude}", Color.green);

                }
                else if (mInputData.IsDragTriggered)
                {
                    //TODO: Drag here
                    TextDebugger.Instance.LogColored($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, current delta: {mInputData.PointerCurrentDelta}", Color.yellow);
                }
            }

            //TODO: Pointer Move can be here ???
            //TextDebugger.Instance.Log($"Pointer Move at: {currentPosition}");
        }

        private void Update()
        {
            if (mRaiseDragOnUpdates && mInputData.IsPointerDownTriggered && mInputData.IsDragTriggered)
            {
                //TODO: Drag here
                TextDebugger.Instance.Log($"Drag performed at: {mInputData.PointerCurrentPosition}, prev position: {mInputData.PointerPreviousPosition}, currend delta: {mInputData.PointerCurrentDelta}");
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