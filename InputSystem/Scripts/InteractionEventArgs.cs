using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    /// <summary>
    /// The basic interaction event args. They can be handled and that is all at this time
    /// </summary>
    public class InteractionEventArgs : EventArgs
    {
        public InteractionObjectBase HandleObjectBase { get; private set; }

        public bool IsHandled { get; private set; }

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

        public void Handle(InteractionObjectBase interactionObjectBase)
        {
            if (interactionObjectBase == null)
                throw new ArgumentNullException(nameof(interactionObjectBase));

            HandleObjectBase = interactionObjectBase;
            IsHandled = true;
        }
    }

    /// <summary>
    /// Event args that contain Vector2 Position
    /// </summary>
    public class PointerInteractionEventArgs : InteractionEventArgs
    {
        public Vector2 PointerPosition { get; private set; }

        public PointerInteractionEventArgs(Vector2 position, Camera camera) : base(camera) => PointerPosition = position;
    }

    /// <summary>
    /// Event args that contain Vector2 Position and previous Vector2 Position
    /// </summary>
    public class PointerDragInteractionEventArgs : PointerInteractionEventArgs
    {
        public Vector2 PointerPrevPosition { get; private set; }
        
        public Vector2 PointerDelta => PointerPosition - PointerPrevPosition;

        public PointerDragInteractionEventArgs(Vector2 position, Vector2 prevPosition, Camera camera) : base(position, camera) => PointerPrevPosition = prevPosition;
    }
    
    public class MouseInteractionEventArgs : PointerInteractionEventArgs
    {
        public virtual int Button { get; private set; }
        
        public MouseInteractionEventArgs(Vector2 position, int button, Camera camera) : base(position, camera) => Button = button;
    }
    
    public class MouseDragInteractionEventArgs : PointerDragInteractionEventArgs
    {
        public virtual int Button { get; private set; }
        
        public MouseDragInteractionEventArgs(Vector2 position, Vector2 prevPosition, int button, Camera camera) : base(position, prevPosition, camera) => Button = button;
    }

    public class TwoPointersDragInteractionEventArgs : PointerDragInteractionEventArgs
    {
        public Vector2 SecondaryPointerPosition { get; private set; }
        public Vector2 SecondaryPointerPrevPosition { get; private set; }
        public Vector2 PointersCenter => (PointerPosition + SecondaryPointerPosition) * 0.5f;
        public Vector2 PointerCenterPrev => (PointerPrevPosition + SecondaryPointerPrevPosition) * 0.5f;

        public TwoPointersDragInteractionEventArgs(Vector2 position, Vector2 prevPosition, Vector2 positionPointer2, Vector2 prevPositionPointer2, Camera camera) :
            base(position, prevPosition, camera)
        {
            SecondaryPointerPrevPosition = positionPointer2;
            SecondaryPointerPosition = prevPositionPointer2;
        }
    }
}