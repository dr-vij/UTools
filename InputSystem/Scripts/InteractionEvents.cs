using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    /// <summary>
    /// The basic interaction event. Maybe it should be generic?
    /// </summary>
    public class InteractionEvent
    {
        public static ulong mCounter = 0;

        private ulong mId;

        public InteractionEvent() => mId = ++mCounter;

        public override int GetHashCode() => mId.GetHashCode();
    }

    /// <summary>
    /// These are object interaction events. They can be sent to specific objects
    /// I use them as keys at this moment.
    /// </summary>
    public static class InteractionEvents
    {
        /// <summary>
        /// Called when user presses on object
        /// </summary>
        public static readonly InteractionEvent PointerPressEvent = new InteractionEvent();

        /// <summary>
        /// Called when user moves pointer over object
        /// </summary>
        public static readonly InteractionEvent PointerMoveEvent = new InteractionEvent();

        /// <summary>
        /// Called when user starts dragging object
        /// </summary>
        public static readonly InteractionEvent PointerDragStartEvent = new InteractionEvent();

        /// <summary>
        /// Called when user drag
        /// </summary>
        public static readonly InteractionEvent PointerDragEvent = new InteractionEvent();

        /// <summary>
        /// Called when user stops drag
        /// </summary>
        public static readonly InteractionEvent PointerDragEndEvent = new InteractionEvent();

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
