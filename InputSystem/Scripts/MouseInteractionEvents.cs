namespace UTools.Input
{
    public static partial class MouseInteractionEvents
    {
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
        public static readonly InteractionEvent MouseDragPerformEvent = new();

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
        public static readonly InteractionEvent MouseLeftDragPerformEvent = new();

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
        public static readonly InteractionEvent MouseRightDragPerformEvent = new();

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
        public static readonly InteractionEvent MouseMiddleDragPerformEvent = new();

        /// <summary>
        /// Raised when user starts dragging object with middle mouse button
        /// </summary>
        public static readonly InteractionEvent MouseMiddleDragStartEvent = new();

        /// <summary>
        /// Raised when user stops dragging object with middle mouse button
        /// </summary>
        public static readonly InteractionEvent MouseMiddleDragEndEvent = new();
    }
}