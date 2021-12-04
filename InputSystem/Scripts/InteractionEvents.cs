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
    /// I create events here. They will be propagated to objects
    /// I use them as keys at this moment.
    /// </summary>
    public static class InteractionEvents
    {
        /// <summary>
        /// Called when user presses on object
        /// </summary>
        public static InteractionEvent InteractionPressEvent = new InteractionEvent();

        /// <summary>
        /// Called when user moves pointer over object
        /// </summary>
        public static InteractionEvent InteractionPointerMoveEvent = new InteractionEvent();

        /// <summary>
        /// Called when user starts dragging object
        /// </summary>
        public static InteractionEvent InteractionDragStartEvent = new InteractionEvent();

        /// <summary>
        /// Called when user drag
        /// </summary>
        public static InteractionEvent InteractionDragEvent = new InteractionEvent();

        /// <summary>
        /// Called when user stops drag
        /// </summary>
        public static InteractionEvent InteractionDragEndEvent = new InteractionEvent();
    }
}
