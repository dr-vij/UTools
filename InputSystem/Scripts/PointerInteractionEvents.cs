namespace UTools.Input
{
    /// <summary>
    /// The basic interaction event.
    /// </summary>
    public class InteractionEvent
    {
        private static ulong m_Counter;

        private readonly ulong m_Id;

        public InteractionEvent() => m_Id = ++m_Counter;

        public override int GetHashCode() => m_Id.GetHashCode();
    }

    /// <summary>
    /// These are object interaction events. They can be sent to specific objects
    /// I use them as keys at this moment.
    /// </summary>
    public static partial class PointerInteractionEvents
    {
        #region Mouse Interaction Events

        /// <summary>
        /// Raised when user presses on object
        /// </summary>
        public static readonly InteractionEvent MouseGrabEvent = new();

        /// <summary>
        /// Raised when user releases mouse button after pressing on object
        /// </summary>
        public static readonly InteractionEvent MouseReleaseEvent = new();

        /// <summary>
        /// Raised when user released the mouse button without dragging an object
        /// </summary>
        public static readonly InteractionEvent MousePressEvent = new();

        /// <summary>
        /// Raised when user moves mouse after dragging object
        /// </summary>
        public static readonly InteractionEvent MouseDragEvent = new();

        /// <summary>
        /// Raised when user starts dragging object
        /// </summary>
        public static readonly InteractionEvent MouseDragStartEvent = new();
        
        /// <summary>
        /// Raised when user stops dragging object
        /// </summary>
        public static readonly InteractionEvent MouseDragEndEvent = new();

        /// <summary>
        /// Raised when user presses on object with left mouse button
        /// </summary>
        public static readonly InteractionEvent MouseLeftGrabEvent = new();

        /// <summary>
        /// Raised when user releases left mouse button after pressing on object
        /// </summary>
        public static readonly InteractionEvent MouseLeftReleaseEvent = new();

        /// <summary>
        /// Raised when user released the left mouse button without dragging an object
        /// </summary>
        public static readonly InteractionEvent MouseLeftPressEvent = new();

        /// <summary>
        /// Raised when user moves mouse after dragging object with left mouse button
        /// </summary>
        public static readonly InteractionEvent MouseLeftDragEvent = new();

        /// <summary>
        /// Raised when user starts dragging object with left mouse button
        /// </summary>
        public static readonly InteractionEvent MouseLeftDragStartEvent = new();

        /// <summary>
        /// Raised when user stops dragging object with left mouse button
        /// </summary>
        public static readonly InteractionEvent MouseLeftDragEndEvent = new();

        /// <summary>
        /// Raised when user presses on object with right mouse button
        /// </summary>
        public static readonly InteractionEvent MouseRightGrabEvent = new();

        /// <summary>
        /// Raised when user releases right mouse button after pressing on object
        /// </summary>
        public static readonly InteractionEvent MouseRightReleaseEvent = new();
        
        /// <summary>
        /// Raised when user released the right mouse button without dragging an object
        /// </summary>
        public static readonly InteractionEvent MouseRightPressEvent = new();

        /// <summary>
        /// Raised when user moves mouse after dragging object with right mouse button
        /// </summary>
        public static readonly InteractionEvent MouseRightDragEvent = new();

        /// <summary>
        /// Raised when user starts dragging object with right mouse button
        /// </summary>
        public static readonly InteractionEvent MouseRightDragStartEvent = new();

        /// <summary>
        /// Raised when user stops dragging object with right mouse button
        /// </summary>
        public static readonly InteractionEvent MouseRightDragEndEvent = new();

        /// <summary>
        /// Raised when user presses on object with middle mouse button
        /// </summary>
        public static readonly InteractionEvent MouseMiddleGrabEvent = new();

        /// <summary>
        /// Raised when user releases middle mouse button after pressing on object
        /// </summary>
        public static readonly InteractionEvent MouseMiddleReleaseEvent = new();
        
        /// <summary>
        /// Raised when user released the middle mouse button without dragging an object
        /// </summary>
        public static readonly InteractionEvent MouseMiddlePressEvent = new();

        /// <summary>
        /// Raised when user moves mouse after dragging object with middle mouse button
        /// </summary>
        public static readonly InteractionEvent MouseMiddleDragEvent = new();

        /// <summary>
        /// Raised when user starts dragging object with middle mouse button
        /// </summary>
        public static readonly InteractionEvent MouseMiddleDragStartEvent = new();

        /// <summary>
        /// Raised when user stops dragging object with middle mouse button
        /// </summary>
        public static readonly InteractionEvent MouseMiddleDragEndEvent = new();

        #endregion

        #region Pointer Interaction Events

        /// <summary>
        /// Raised when user presses on object
        /// </summary>
        public static readonly InteractionEvent PointerPressEvent = new();

        /// <summary>
        /// Raised when user moves pointer over object
        /// </summary>
        public static readonly InteractionEvent PointerMoveEvent = new();

        /// <summary>
        /// Raised when user starts dragging object
        /// </summary>
        public static readonly InteractionEvent PointerDragStartEvent = new();

        /// <summary>
        /// Raised when user drag
        /// </summary>
        public static readonly InteractionEvent PointerDragEvent = new();

        /// <summary>
        /// Raised when user stops drag (remove or add pointer)
        /// </summary>
        public static readonly InteractionEvent PointerDragEndEvent = new();

        /// <summary>
        /// Raised when user starts drag with two pointers
        /// </summary>
        public static readonly InteractionEvent TwoPointersDragStartEvent = new();

        /// <summary>
        /// Raised when user drags with two pointers
        /// </summary>
        public static readonly InteractionEvent TwoPointersDragEvent = new();

        /// <summary>
        /// Raised when user stops drag with two pointers (remove or add pointer)
        /// </summary>
        public static readonly InteractionEvent TwoPointersDragEndEvent = new();

        /// <summary>
        /// Called when pointer is down on object
        /// </summary>
        public static readonly InteractionEvent PointerGrabEvent = new();

        /// <summary>
        /// Called when pointer is up and user stopped grabbing object
        /// </summary>
        public static readonly InteractionEvent PointerReleaseEvent = new();

        #endregion
    }
}