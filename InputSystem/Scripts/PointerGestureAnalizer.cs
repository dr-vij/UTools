using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ViJTools
{
    public class PointerGestureAnalizer : DisposableObject
    {
        private static int m_IdCounter = -1;

        private bool mPressInterrupted = false;
        private bool mOnePointerDragStarted = false;
        private bool mTwoPointerDragStarted = false;

        protected Dictionary<int, InteractionPointer> m_Pointers = new Dictionary<int, InteractionPointer>();

        public Camera InteractionCamera { get; private set; }

        public int GestureId { get; private set; }

        public InteractionObject InteractionObject { get; private set; }

        public int PointersCount => m_Pointers.Count;

        public IEnumerable<InteractionPointer> Pointers => m_Pointers.Values;

        public PointerGestureAnalizer(InteractionObject interactionObject, Camera interactionCamera)
        {
            InteractionObject = interactionObject;
            InteractionCamera = interactionCamera;

            GestureId = ++m_IdCounter;
        }

        public bool HasPointer(int pointerId) => m_Pointers.ContainsKey(pointerId);

        public bool TryGetPointer(int pointerId, out InteractionPointer pointer) => m_Pointers.TryGetValue(pointerId, out pointer);

        public void CreatePointer(int pointerId, Vector2 position)
        {
            var pointer = new InteractionPointer(pointerId, position);
            ResetPointersPositionExceptOne(pointer);

            switch (PointersCount)
            {
                case 0:
                    //Grab start and reset all gesture flags
                    var args = new PointerInteractionEventArgs(pointer.CurrentPosition, InteractionCamera);
                    InteractionObject.RunEvent(InteractionEvents.PointerGrabStartEvent, args);
                    ResetDragFlagsAndPointerPositions();
                    break;
                case 1:
                    CheckIfDragEnd();
                    ResetDragFlagsAndPointerPositions(true);
                    break;
                case 2:
                    CheckIfDragEnd();
                    ResetDragFlagsAndPointerPositions(true);
                    break;
                default:
                    ResetDragFlagsAndPointerPositions(true);
                    break;
            }

            pointer.PointerUpdateEvent += OnPointerUpdate;
            m_Pointers.Add(pointerId, pointer);
            //Debug.Log($"Gesture {GestureId} TestPointer down {pointerId}, position {pointer.CurrentPosition}");
        }

        public void RemovePointer(int pointerId, Vector2 position)
        {
            var pointer = m_Pointers[pointerId];
            ResetPointersPositionExceptOne(pointer);
            if (pointer.CurrentPosition != position)
                pointer.UpdateDataAndRaise(position);

            switch (PointersCount)
            {
                case 1:
                    if (!mPressInterrupted && !mOnePointerDragStarted && !mTwoPointerDragStarted)
                    {
                        var pressArgs = new PointerInteractionEventArgs(pointer.CurrentPosition, InteractionCamera);
                        InteractionObject.RunEvent(InteractionEvents.PointerPressEvent, pressArgs);
                    }
                    CheckIfDragEnd();
                    var args = new PointerInteractionEventArgs(pointer.CurrentPosition, InteractionCamera);
                    InteractionObject.RunEvent(InteractionEvents.PointerGrabEndEvent, args);
                    ResetDragFlagsAndPointerPositions();
                    break;
                case 2:
                case 3:
                    CheckIfDragEnd();
                    ResetDragFlagsAndPointerPositions(true);
                    break;
                default:
                    break;
            }

            pointer.PointerUpdateEvent -= OnPointerUpdate;
            m_Pointers.Remove(pointerId);
            //POINTER i UP;
            //Debug.Log($"Gesture {GestureId} TestPointer up {pointerId}, position {pointer.CurrentPosition}");
        }

        private void OnPointerUpdate(InteractionPointer pointer)
        {
            ResetPointersPositionExceptOne(pointer);
            int triggerDistance = InputManager.Instance.DragOrPressTriggerDistance;

            switch (m_Pointers.Count)
            {
                case 1:
                    if (mOnePointerDragStarted)
                    {
                        //Pointer drag here
                        var dragArgs = new PointerDragInteractionEventArgs(pointer.CurrentPosition, pointer.PrevPosition, InteractionCamera);
                        InteractionObject.RunEvent(InteractionEvents.PointerDragEvent, dragArgs);
                    }
                    else if (Vector2.Distance(pointer.CurrentPosition, pointer.TrackStartPosition) > triggerDistance)
                    {
                        //pointer drag detection here
                        var dragStartArgs = new PointerDragInteractionEventArgs(pointer.CurrentPosition, pointer.TrackStartPosition, InteractionCamera);
                        InteractionObject.RunEvent(InteractionEvents.PointerDragStartEvent, dragStartArgs);
                        mOnePointerDragStarted = true;
                    }
                    break;
                case 2:
                    (var pointer1, var pointer2) = GetTwoPointers();
                    if (mTwoPointerDragStarted)
                    {
                        var args = new TwoPointersDragInteractionEventArgs(pointer1.CurrentPosition, pointer1.PrevPosition, pointer2.CurrentPosition, pointer2.PrevPosition, InteractionCamera);
                        InteractionObject.RunEvent(InteractionEvents.TwoPointersDragEvent, args);
                    }
                    else if (Vector2.Distance(pointer1.CurrentPosition, pointer1.TrackStartPosition) > triggerDistance || Vector2.Distance(pointer2.CurrentPosition, pointer2.TrackStartPosition) > triggerDistance)
                    {
                        var args = new TwoPointersDragInteractionEventArgs(pointer1.CurrentPosition, pointer1.PrevPosition, pointer2.CurrentPosition, pointer2.PrevPosition, InteractionCamera);
                        InteractionObject.RunEvent(InteractionEvents.TwoPointersDragStartEvent, args);
                        mTwoPointerDragStarted = true;
                    }
                    break;
                default:
                    break;
            }

            //POINTER i UPDATE
            //Debug.Log($"Gesture {GestureId} TestPointer update {pointer.ID}, position {pointer.CurrentPosition}");
        }

        public void UpdateAllPointers()
        {
            foreach (var pointer in m_Pointers.Values)
                pointer.UpdateDataAndRaise(pointer.CurrentPosition);
        }

        private void CheckIfDragEnd()
        {
            if (mOnePointerDragStarted)
            {
                //Pointer drag end
                var firstPointer = m_Pointers.First().Value;
                var dragArgs = new PointerDragInteractionEventArgs(firstPointer.CurrentPosition, firstPointer.PrevPosition, InteractionCamera);
                InteractionObject.RunEvent(InteractionEvents.PointerDragEndEvent, dragArgs);
                mOnePointerDragStarted = false;
            }
            else if (mTwoPointerDragStarted)
            {
                //Two pointers drag end
                (var firstPointer, var secondPointer) = GetTwoPointers();
                var twoPointersDragEndArgs = new TwoPointersDragInteractionEventArgs(firstPointer.CurrentPosition, firstPointer.PrevPosition, secondPointer.CurrentPosition, secondPointer.PrevPosition, InteractionCamera);
                InteractionObject.RunEvent(InteractionEvents.TwoPointersDragEndEvent, twoPointersDragEndArgs);
                mTwoPointerDragStarted = false;
            }
        }

        private (InteractionPointer pointer1, InteractionPointer pointer2) GetTwoPointers()
        {
            var firstPointer = m_Pointers.ElementAtOrDefault(0).Value;
            var secondPointer = m_Pointers.ElementAtOrDefault(1).Value;
            return (firstPointer.ID < secondPointer.ID) ? (firstPointer, secondPointer) : (secondPointer, firstPointer);
        }

        private void ResetPointersPositionExceptOne(InteractionPointer excluded)
        {
            foreach (var pointer in m_Pointers.Values)
                if (pointer != excluded)
                    pointer.ResetLastPosition();
        }

        private void ResetDragFlagsAndPointerPositions(bool pressIsInterrupted = false)
        {
            foreach (var p in m_Pointers.Values)
                p.ResetPointerTracking();

            mPressInterrupted = pressIsInterrupted;
            mOnePointerDragStarted = false;
            mTwoPointerDragStarted = false;
        }
    }
}