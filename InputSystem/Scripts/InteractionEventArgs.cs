using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    /// <summary>
    /// The basic interaction event args. They can be handled and that is all at this time
    /// </summary>
    public class InteractionEventArgs : EventArgs
    {
        public InteractionObject HandleObject { get; private set; }

        public bool IsHandled { get; private set; }

        public void Handle(InteractionObject interactionObject)
        {
            if (interactionObject == null)
                throw new ArgumentNullException("Handler object must not be null");

            HandleObject = interactionObject;
            IsHandled = true;
        }
    }

    /// <summary>
    /// Event args that contain Vector2 Position
    /// </summary>
    public class PointerInteractionEventArgs : InteractionEventArgs
    {
        public Vector2 Position { get; private set; }

        public PointerInteractionEventArgs(Vector2 position) => Position = position;
    }

    /// <summary>
    /// Event args that contain Vector2 Position and previous Vector2 Position
    /// </summary>
    public class PointerDragInteractionEventArgs : PointerInteractionEventArgs
    {
        public Vector2 PrevPosition { get; private set; }

        public PointerDragInteractionEventArgs(Vector2 position, Vector2 prevPosition) : base(position) => PrevPosition = prevPosition;
    }
}