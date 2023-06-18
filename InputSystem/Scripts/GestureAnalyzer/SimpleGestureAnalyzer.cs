using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTools.Input
{
    public class SimpleGestureAnalyzer : IPointerGestureAnalyzer, IMouseGestureAnalyzer
    {
        private static int m_IdCounter = -1;

        //Pointer data
        private bool m_PressInterrupted;
        private bool m_OnePointerDragStarted;
        private bool m_TwoPointerDragStarted;

        private readonly Dictionary<int, InteractionPointer> m_Pointers = new();

        //Mouse data
        private Vector2 m_MousePosition;
        private InteractionPointer m_MousePointer;
        private bool m_MouseDragStarted;
        private int m_PressedMouseButton = int.MaxValue;

        public int GestureId { get; private set; }

        public bool IsSoloGesture { get; }

        public Camera InteractionCamera { get; private set; }

        public InteractionObjectBase InteractionObjectBase { get; private set; }

        public int PointersCount => m_Pointers.Count;

        public IEnumerable<InteractionPointer> Pointers => m_Pointers.Values;

        public SimpleGestureAnalyzer(InteractionObjectBase interactionObjectBase, Camera interactionCamera, bool isSolo)
        {
            InteractionObjectBase = interactionObjectBase;
            InteractionCamera = interactionCamera;
            IsSoloGesture = isSolo;
            GestureId = ++m_IdCounter;
        }

        public bool CanBeRemoved => PointersCount == 0 && m_PressedMouseButton == int.MaxValue;

        #region Pointer Logic

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
                    InteractionObjectBase.RunEvent(PointerInteractionEvents.PointerGrabEvent, args);
                    ResetDragFlagsAndPointerPositions();
                    break;
                case 1:
                    EndExistingPointerDrags();
                    ResetDragFlagsAndPointerPositions(true);
                    break;
                case 2:
                    EndExistingPointerDrags();
                    ResetDragFlagsAndPointerPositions(true);
                    break;
                default:
                    ResetDragFlagsAndPointerPositions(true);
                    break;
            }

            pointer.OnPointerUpdateEvent += OnPointerUpdate;
            m_Pointers.Add(pointerId, pointer);
        }

        public void RemovePointer(int pointerId, Vector2 position)
        {
            var pointer = m_Pointers[pointerId];
            ResetPointersPositionExceptOne(pointer);
            if (pointer.CurrentPosition != position)
                pointer.UpdatePositionAndRaise(position);

            switch (PointersCount)
            {
                case 1:
                    if (!m_PressInterrupted && !m_OnePointerDragStarted && !m_TwoPointerDragStarted)
                    {
                        var pressArgs = new PointerInteractionEventArgs(pointer.CurrentPosition, InteractionCamera);
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.PointerPressEvent, pressArgs);
                    }

                    EndExistingPointerDrags();
                    var args = new PointerInteractionEventArgs(pointer.CurrentPosition, InteractionCamera);
                    InteractionObjectBase.RunEvent(PointerInteractionEvents.PointerReleaseEvent, args);
                    ResetDragFlagsAndPointerPositions();
                    break;
                case 2:
                case 3:
                    EndExistingPointerDrags();
                    ResetDragFlagsAndPointerPositions(true);
                    break;
                default:
                    break;
            }

            pointer.OnPointerUpdateEvent -= OnPointerUpdate;
            m_Pointers.Remove(pointerId);
        }

        private void OnPointerUpdate(InteractionPointer pointer)
        {
            ResetPointersPositionExceptOne(pointer);
            int triggerDistance = InputManager.Instance.DragOrPressTriggerDistance;

            switch (m_Pointers.Count)
            {
                case 1:
                    if (m_OnePointerDragStarted)
                    {
                        //Pointer drag here
                        var dragArgs = new PointerDragInteractionEventArgs(pointer.CurrentPosition, pointer.PreviousPosition, InteractionCamera);
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.PointerDragEvent, dragArgs);
                    }
                    else if (Vector2.Distance(pointer.CurrentPosition, pointer.TrackStartPosition) > triggerDistance)
                    {
                        //pointer drag detection here
                        var dragStartArgs = new PointerDragInteractionEventArgs(pointer.CurrentPosition, pointer.TrackStartPosition, InteractionCamera);
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.PointerDragStartEvent, dragStartArgs);
                        m_OnePointerDragStarted = true;
                    }

                    break;
                case 2:
                    var (pointer1, pointer2) = GetTwoPointers();
                    if (m_TwoPointerDragStarted)
                    {
                        var args = new TwoPointersDragInteractionEventArgs(pointer1.CurrentPosition, pointer1.PreviousPosition, pointer2.CurrentPosition,
                            pointer2.PreviousPosition, InteractionCamera);
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.TwoPointersDragEvent, args);
                    }
                    else if (Vector2.Distance(pointer1.CurrentPosition, pointer1.TrackStartPosition) > triggerDistance ||
                             Vector2.Distance(pointer2.CurrentPosition, pointer2.TrackStartPosition) > triggerDistance)
                    {
                        var args = new TwoPointersDragInteractionEventArgs(pointer1.CurrentPosition, pointer1.PreviousPosition, pointer2.CurrentPosition,
                            pointer2.PreviousPosition, InteractionCamera);
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.TwoPointersDragStartEvent, args);
                        m_TwoPointerDragStarted = true;
                    }

                    break;
                default:
                    break;
            }
        }

        private void EndExistingPointerDrags()
        {
            if (m_OnePointerDragStarted)
            {
                //Pointer drag end
                var firstPointer = m_Pointers.First().Value;
                var dragArgs = new PointerDragInteractionEventArgs(firstPointer.CurrentPosition, firstPointer.PreviousPosition, InteractionCamera);
                InteractionObjectBase.RunEvent(PointerInteractionEvents.PointerDragEndEvent, dragArgs);
                m_OnePointerDragStarted = false;
            }
            else if (m_TwoPointerDragStarted)
            {
                //Two pointers drag end
                var (firstPointer, secondPointer) = GetTwoPointers();
                var twoPointersDragEndArgs = new TwoPointersDragInteractionEventArgs(firstPointer.CurrentPosition, firstPointer.PreviousPosition,
                    secondPointer.CurrentPosition, secondPointer.PreviousPosition, InteractionCamera);
                InteractionObjectBase.RunEvent(PointerInteractionEvents.TwoPointersDragEndEvent, twoPointersDragEndArgs);
                m_TwoPointerDragStarted = false;
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
                    pointer.ResetLastPositionSilently();
        }

        private void ResetDragFlagsAndPointerPositions(bool pressIsInterrupted = false)
        {
            foreach (var p in m_Pointers.Values)
                p.ResetPointerTrackingSilently();

            m_PressInterrupted = pressIsInterrupted;
            m_OnePointerDragStarted = false;
            m_TwoPointerDragStarted = false;
        }

        #endregion

        public void UpdateAnalyzer()
        {
            foreach (var pointer in m_Pointers.Values)
                pointer.UpdatePositionAndRaise(pointer.CurrentPosition);
            m_MousePointer?.UpdatePositionAndRaise(m_MousePosition);
        }

        public void UpdateMousePosition(Vector2 position)
        {
            m_MousePosition = position;
            m_MousePointer?.UpdatePositionAndRaise(position);
        }

        public void MouseButtonDown(int button)
        {
            if (m_PressedMouseButton == int.MaxValue)
            {
                // Debug.Log($"MouseButtonDown {button}");

                m_PressedMouseButton = button;
                m_MousePointer = new InteractionPointer(button, m_MousePosition);
                m_MousePointer.OnPointerUpdateEvent += OnMousePositionChanged;

                var mouseDownArgs = new MouseInteractionEventArgs(m_MousePosition, button, InteractionCamera);
                InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseGrabEvent, mouseDownArgs);
                switch (m_PressedMouseButton)
                {
                    case Helpers.LeftMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseLeftGrabEvent, mouseDownArgs);
                        break;
                    case Helpers.MiddleMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseMiddleGrabEvent, mouseDownArgs);
                        break;
                    case Helpers.RightMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseRightGrabEvent, mouseDownArgs);
                        break;
                }
            }
        }

        public void MouseButtonUp(int button)
        {
            if (m_PressedMouseButton == button)
            {
                // Debug.Log($"MouseButtonUp {button}");

                if (m_MouseDragStarted)
                {
                    var mouseDragEndArgs = new MouseDragInteractionEventArgs(m_MousePointer.CurrentPosition, m_MousePointer.TrackStartPosition, button, InteractionCamera);
                    InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseDragEndEvent, mouseDragEndArgs);
                    switch (m_PressedMouseButton)
                    {
                        case Helpers.LeftMouseInputId:
                            InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseLeftDragEndEvent, mouseDragEndArgs);
                            break;
                        case Helpers.MiddleMouseInputId:
                            InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseMiddleDragEndEvent, mouseDragEndArgs);
                            break;
                        case Helpers.RightMouseInputId:
                            InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseRightDragEndEvent, mouseDragEndArgs);
                            break;
                    }
                }
                else
                {
                    var mousePressArgs = new MouseInteractionEventArgs(m_MousePointer.CurrentPosition, button, InteractionCamera);
                    InteractionObjectBase.RunEvent(PointerInteractionEvents.MousePressEvent, mousePressArgs);
                    switch (m_PressedMouseButton)
                    {
                        case Helpers.LeftMouseInputId:
                            InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseLeftPressEvent, mousePressArgs);
                            break;
                        case Helpers.MiddleMouseInputId:
                            InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseMiddlePressEvent, mousePressArgs);
                            break;
                        case Helpers.RightMouseInputId:
                            InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseRightPressEvent, mousePressArgs);
                            break;
                    }
                }

                var mouseUpArgs = new MouseInteractionEventArgs(m_MousePosition, button, InteractionCamera);
                InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseReleaseEvent, mouseUpArgs);
                switch (m_PressedMouseButton)
                {
                    case Helpers.LeftMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseLeftReleaseEvent, mouseUpArgs);
                        break;
                    case Helpers.MiddleMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseMiddleReleaseEvent, mouseUpArgs);
                        break;
                    case Helpers.RightMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseRightReleaseEvent, mouseUpArgs);
                        break;
                }

                m_MouseDragStarted = false;
                m_PressedMouseButton = int.MaxValue;
                m_MousePointer.OnPointerUpdateEvent -= OnMousePositionChanged;
                m_MousePointer = null;
            }
        }

        private void OnMousePositionChanged(InteractionPointer mousePointer)
        {
            // Debug.Log("Mouse position changed");
            int triggerDistance = InputManager.Instance.DragOrPressTriggerDistance;
            if (m_MouseDragStarted)
            {
                //Pointer drag here
                var dragArgs = new MouseDragInteractionEventArgs(mousePointer.CurrentPosition, mousePointer.PreviousPosition, m_PressedMouseButton, InteractionCamera);
                InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseDragPerformEvent, dragArgs);
                switch (m_PressedMouseButton)
                {
                    case Helpers.LeftMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseLeftDragPerformEvent, dragArgs);
                        break;
                    case Helpers.MiddleMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseMiddleDragPerformEvent, dragArgs);
                        break;
                    case Helpers.RightMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseRightDragPerformEvent, dragArgs);
                        break;
                }
            }
            else if (Vector2.Distance(mousePointer.CurrentPosition, mousePointer.TrackStartPosition) > triggerDistance)
            {
                //pointer drag detection here
                m_MouseDragStarted = true;
                var dragStartArgs =
                    new MouseDragInteractionEventArgs(mousePointer.CurrentPosition, mousePointer.PreviousPosition, m_PressedMouseButton, InteractionCamera);
                InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseDragStartEvent, dragStartArgs);

                switch (m_PressedMouseButton)
                {
                    case Helpers.LeftMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseLeftDragStartEvent, dragStartArgs);
                        break;
                    case Helpers.MiddleMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseMiddleDragStartEvent, dragStartArgs);
                        break;
                    case Helpers.RightMouseInputId:
                        InteractionObjectBase.RunEvent(PointerInteractionEvents.MouseRightDragStartEvent, dragStartArgs);
                        break;
                }
            }
        }
    }
}