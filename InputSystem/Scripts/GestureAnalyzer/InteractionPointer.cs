using System;
using UnityEngine;

namespace UTools.Input
{
    public class InteractionPointer
    {
        public int ID { get; private set; }
        public Vector2 CreationPosition { get; private set; }
        public Vector2 TrackStartPosition { get; private set; }
        public Vector2 CurrentPosition { get; private set; }
        public Vector2 PreviousPosition { get; private set; }

        public event Action<InteractionPointer> OnPointerUpdateEvent;

        public InteractionPointer(int id, Vector2 position)
        {
            CreationPosition = position;
            TrackStartPosition = position;
            CurrentPosition = position;
            PreviousPosition = position;

            ID = id;
        }

        /// <summary>
        /// Sets previous position current
        /// </summary>
        public void ResetLastPositionSilently()
        {
            PreviousPosition = CurrentPosition;
        }

        /// <summary>
        /// Sets TrackStartPosition and PrevPosition from Current position
        /// </summary>
        public void ResetPointerTrackingSilently()
        {
            TrackStartPosition = CurrentPosition;
            PreviousPosition = CurrentPosition;
        }

        /// <summary>
        /// Updates position and raises event
        /// </summary>
        /// <param name="newPosition"></param>
        public void UpdatePositionAndRaise(Vector2 newPosition)
        {
            PreviousPosition = CurrentPosition;
            CurrentPosition = newPosition;
            OnPointerUpdateEvent?.Invoke(this);
        }
    }
}