using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public class InteractionPointer
    {
        public int ID { get; private set; }
        public Vector2 CreationPosition { get; private set; }

        public Vector2 TrackStartPosition { get; private set; }
        public Vector2 CurrentPosition { get; private set; }
        public Vector2 PrevPosition { get; private set; }

        public event Action<InteractionPointer> PointerUpdateEvent;

        public InteractionPointer(int id, Vector2 position)
        {
            CreationPosition = position;

            TrackStartPosition = position;
            CurrentPosition = position;
            PrevPosition = position;

            ID = id;
        }

        /// <summary>
        /// Sets previous position current
        /// </summary>
        public void ResetLastPosition()
        {
            PrevPosition = CurrentPosition;
        }

        /// <summary>
        /// Sets TrackStartPosition Prev position Current position
        /// </summary>
        public void ResetPointerTracking()
        {
            TrackStartPosition = CurrentPosition;
            PrevPosition = CurrentPosition;
        }

        /// <summary>
        /// Updates position and raises event
        /// </summary>
        /// <param name="newPosition"></param>
        public void UpdateDataAndRaise(Vector2 newPosition)
        {
            PrevPosition = CurrentPosition;
            CurrentPosition = newPosition;

            PointerUpdateEvent?.Invoke(this);
        }
    }
}