using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    /// <summary>
    /// The basic interaction event. Maybe it should be generic?
    /// </summary>
    public class InteractionEvent
    {
        public static ulong m_Counter = 0;

        private ulong m_Id;

        public InteractionEvent() => m_Id = ++m_Counter;

        public override int GetHashCode() => m_Id.GetHashCode();
    }

    /// <summary>
    /// These are object interaction events. They can be sent to specific objects
    /// I use them as keys at this moment.
    /// </summary>
    public static class InteractionEvents
    {
        /// <summary>
        /// Raised when user presses on object
        /// </summary>
        public static readonly InteractionEvent PointerPressEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user moves pointer over object
        /// </summary>
        public static readonly InteractionEvent PointerMoveEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user starts dragging object
        /// </summary>
        public static readonly InteractionEvent PointerDragStartEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user drag
        /// </summary>
        public static readonly InteractionEvent PointerDragEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user stops drag (remove or add pointer)
        /// </summary>
        public static readonly InteractionEvent PointerDragEndEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user starts drag with two pointers
        /// </summary>
        public static readonly InteractionEvent TwoPointersDragStartEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user drags with two pointers
        /// </summary>
        public static readonly InteractionEvent TwoPointersDragEvent = new InteractionEvent();

        /// <summary>
        /// Raised when user stops drag with two pointers (remove or add pointer)
        /// </summary>
        public static readonly InteractionEvent TwoPointersDragEndEvent = new InteractionEvent();

        /// <summary>
        /// Called when pointer is down on object
        /// </summary>
        public static readonly InteractionEvent PointerGrabStartEvent = new InteractionEvent();

        /// <summary>
        /// Called when pointer is up and user stopped grabbing object
        /// </summary>
        public static readonly InteractionEvent PointerGrabEndEvent = new InteractionEvent();
    }
}
