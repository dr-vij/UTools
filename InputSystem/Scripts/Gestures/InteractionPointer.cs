using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    public enum PointerKind
    {
        Mouse,
        Pen,
        Touch,
    }

    public enum PointerButton
    {
        Primary,
        Secondary,
        Middle,
    }

    /// <summary>
    /// An interaction object under a pointer and the point where the pointer met it.
    /// </summary>
    public readonly struct PointerHit
    {
        public readonly InteractionObjectBase Target;
        public readonly Vector3 Point;

        public PointerHit(InteractionObjectBase target, Vector3 point)
        {
            Target = target;
            Point = point;
        }
    }

    /// <summary>
    /// One contact of one input source, from the moment it goes down to the moment it lifts: a mouse button, a finger,
    /// a pen tip. The source that makes it says what lies under it and how far it has travelled; the recognizer knows
    /// neither screens nor rays.
    /// </summary>
    public abstract class InteractionPointer
    {
        protected InteractionPointer(int id, PointerKind kind, PointerButton button, float dragThreshold, IReadOnlyList<PointerHit> hits,
            InteractionObjectBase fallback)
        {
            Id = id;
            Kind = kind;
            Button = button;
            DragThreshold = dragThreshold;
            Hits = hits;
            Fallback = fallback;
        }

        public int Id { get; }

        public PointerKind Kind { get; }

        public PointerButton Button { get; }

        /// <summary>
        /// Where the pointer is in the world and where it points: the ray through its point of the screen for a pointer
        /// of a screen, the ray of a controller, the pose of a hand.
        /// </summary>
        public Pose Pose { get; protected set; }

        /// <summary>
        /// The camera of the screen the pointer is on; null for a pointer that has no screen, and its screen positions
        /// then mean nothing.
        /// </summary>
        public Camera Camera { get; protected set; }

        public Vector2 ScreenPosition { get; protected set; }

        public Vector2 PreviousScreenPosition { get; protected set; }

        /// <summary>
        /// Where the travel is counted from: where the pointer went down, or where it was when another pointer joined
        /// or left it.
        /// </summary>
        public Vector2 TrackStartScreenPosition { get; protected set; }

        /// <summary>
        /// The interaction objects under the pointer where it went down, the nearest first, in the order of its source.
        /// </summary>
        public IReadOnlyList<PointerHit> Hits { get; }

        /// <summary>
        /// Gets what no object under the pointer took: the camera's object for a pointer of a screen. May be null.
        /// </summary>
        public InteractionObjectBase Fallback { get; }

        /// <summary>
        /// How far the pointer has gone from where its travel is counted, and how far makes a drag, in units of its
        /// source: the recognizer only compares the two.
        /// </summary>
        public abstract float Travel { get; }

        public float DragThreshold { get; }

        /// <summary>
        /// Counts the travel anew from where the pointer is now.
        /// </summary>
        public abstract void RebaseTravel();

        //A pointer that stands still while its partner of a pair moves has made no step of its own.
        internal void SettleStep() => PreviousScreenPosition = ScreenPosition;
    }

    /// <summary>
    /// A pointer on the screen of a camera: its travel is the distance in pixels.
    /// </summary>
    public sealed class ScreenPointer : InteractionPointer
    {
        public ScreenPointer(int id, PointerKind kind, PointerButton button, Camera camera, Vector2 position, float dragThreshold,
            IReadOnlyList<PointerHit> hits, InteractionObjectBase fallback) : base(id, kind, button, dragThreshold, hits, fallback)
        {
            Camera = camera;
            ScreenPosition = position;
            PreviousScreenPosition = position;
            TrackStartScreenPosition = position;
            UpdatePose();
        }

        public override float Travel => Vector2.Distance(ScreenPosition, TrackStartScreenPosition);

        public override void RebaseTravel()
        {
            TrackStartScreenPosition = ScreenPosition;
            PreviousScreenPosition = ScreenPosition;
        }

        public void MoveTo(Vector2 position)
        {
            PreviousScreenPosition = ScreenPosition;
            ScreenPosition = position;
            UpdatePose();
        }

        private void UpdatePose()
        {
            if (Camera == null)
                return;

            var ray = Camera.ScreenPointToRay(ScreenPosition, Camera.MonoOrStereoscopicEye.Mono);
            Pose = new Pose(ray.origin, Quaternion.LookRotation(ray.direction));
        }
    }
}
