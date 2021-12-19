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

        public bool IsHandled  { get; private set; }

        public Camera InteractionCamera { get; private set; }

        public InteractionEventArgs(Camera camera = null)
        {
            InteractionCamera = camera;
        }

        public void SetCamera(Camera camera)
        {
            if (InteractionCamera == null)
                InteractionCamera = camera;
            else
                Debug.LogError("event args camera cannot be changed");
        }

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

        public PointerInteractionEventArgs(Vector2 position, Camera camera) : base(camera) => Position = position;
    }

    /// <summary>
    /// Event args that contain Vector2 Position and previous Vector2 Position
    /// </summary>
    public class PointerDragInteractionEventArgs : PointerInteractionEventArgs
    {
        public Vector2 PrevPosition { get; private set; }

        public PointerDragInteractionEventArgs(Vector2 position, Vector2 prevPosition, Camera camera) : base(position, camera) => PrevPosition = prevPosition;
    }

    public class TwoPointersDragInteractionEventArgs : PointerDragInteractionEventArgs
    {
        public Vector2 PrevPositionPointer2 { get; private set; }
        public Vector2 PositionPointer2 { get; private set; }

        public TwoPointersDragInteractionEventArgs(Vector2 position, Vector2 prevPosition, Vector2 positionPointer2, Vector2 prevPositionPointer2, Camera camera) :
            base(position, prevPosition, camera)
        {
            PositionPointer2 = positionPointer2;
            PrevPositionPointer2 = prevPositionPointer2;
        }
    }
}