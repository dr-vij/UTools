using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    /// <summary>
    /// The basic interaction event. Maybe it should be generic?
    /// </summary>
    public class ObjectInteractionEvent
    {
        public static ulong mCounter = 0;

        private ulong mId;

        public ObjectInteractionEvent() => mId = ++mCounter;

        public override int GetHashCode() => mId.GetHashCode();
    }

    public class BroadcastInteractionEvent
    {
        public static ulong mCounter = 0;

        private ulong mId;

        public BroadcastInteractionEvent() => mId = ++mCounter;

        public override int GetHashCode() => mId.GetHashCode();
    }

    /// <summary>
    /// Global interaction events are 
    /// </summary>
    public static class BroadcastInteractionEvents
    {
        /// <summary>
        /// Raised when pointer changes its position
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerMoveEvent = new BroadcastInteractionEvent();

        /// <summary>
        /// Raised when pointer is down anywhere
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerDownEvent = new BroadcastInteractionEvent();

        /// <summary>
        /// Raised when pointer is up anywhere
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerUpEvent = new BroadcastInteractionEvent();

        /// <summary>
        /// Raised when pointer press happens anywhere
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerPressEvent = new BroadcastInteractionEvent();

        /// <summary>
        /// Raised when pointer drag starts anywhere
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerDragStartEvent = new BroadcastInteractionEvent();

        /// <summary>
        /// Raised when pointer drags anywhere
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerDragEvent = new BroadcastInteractionEvent();

        /// <summary>
        /// Raised when pointer drag ends anywhere
        /// </summary>
        public static readonly BroadcastInteractionEvent GlobalPointerDragEndEvent = new BroadcastInteractionEvent();
    }

    /// <summary>
    /// These are object interaction events. They can be sent to specific objects
    /// I use them as keys at this moment.
    /// </summary>
    public static class ObjectInteractionEvents
    {
        /// <summary>
        /// Called when user presses on object
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerPressEvent = new ObjectInteractionEvent();

        /// <summary>
        /// Called when user moves pointer over object
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerMoveEvent = new ObjectInteractionEvent();

        /// <summary>
        /// Called when user starts dragging object
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerDragStartEvent = new ObjectInteractionEvent();

        /// <summary>
        /// Called when user drag
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerDragEvent = new ObjectInteractionEvent();

        /// <summary>
        /// Called when user stops drag
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerDragEndEvent = new ObjectInteractionEvent();

        /// <summary>
        /// Called when pointer is down on object
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerGrabStartEvent = new ObjectInteractionEvent();

        /// <summary>
        /// Called when pointer is up and user stopped grabbing object
        /// </summary>
        public static readonly ObjectInteractionEvent ObjectPointerGrabEndEvent = new ObjectInteractionEvent();
    }
}
