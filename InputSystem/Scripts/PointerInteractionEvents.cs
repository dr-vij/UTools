namespace UTools.Input
{
    /// <summary>
    /// These are object interaction events. They can be sent to specific objects
    /// I use them as keys at this moment.
    /// </summary>
    public static partial class PointerInteractionEvents
    {
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
        public static readonly InteractionEvent PointerDragPerformEvent = new();

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
        public static readonly InteractionEvent TwoPointersDragPerformEvent = new();

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
    }
}