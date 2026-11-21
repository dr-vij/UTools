using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    public enum PointerKind
    {
        Mouse,
        Pen,
        Touch,
        /// <summary>A tracked hand: a pinch is its contact.</summary>
        Hand,
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

        public virtual Vector2 PreviousScreenPosition { get; protected set; }

        /// <summary>
        /// Where the travel is counted from: where the pointer went down, or where it was when another pointer joined
        /// or left it.
        /// </summary>
        public virtual Vector2 TrackStartScreenPosition { get; protected set; }

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
    /// A pointer on the screen of a camera: its travel is the distance in pixels. Where the travel is counted from and
    /// where the pointer was a step before are kept as lines of the camera, read into the pixels of the screen as it is
    /// when they are read: a pixel means another place once the screen changes its size (a window resized), and a
    /// pointer that holds on to what it pressed while the picture is laid out anew would travel as far as the picture
    /// moved under it, and its press would become a drag.
    /// </summary>
    public sealed class ScreenPointer : InteractionPointer
    {
        private Vector3 m_TrackStart;
        private Vector3 m_Previous;

        public ScreenPointer(int id, PointerKind kind, PointerButton button, Camera camera, Vector2 position, float dragThreshold,
            IReadOnlyList<PointerHit> hits, InteractionObjectBase fallback) : base(id, kind, button, dragThreshold, hits, fallback)
        {
            Camera = camera;
            ScreenPosition = position;
            PreviousScreenPosition = position;
            TrackStartScreenPosition = position;
            UpdatePose();
        }

        public override Vector2 TrackStartScreenPosition
        {
            get => FromCamera(m_TrackStart);
            protected set => m_TrackStart = ToCamera(value);
        }

        public override Vector2 PreviousScreenPosition
        {
            get => FromCamera(m_Previous);
            protected set => m_Previous = ToCamera(value);
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

        // The point of the near plane under the point of the screen, in the camera's own frame - on the camera's line
        // through it, for a perspective camera and an orthographic one alike; the pixels themselves where there is no
        // camera.
        private Vector3 ToCamera(Vector2 position) =>
            Camera == null
                ? position
                : Camera.transform.InverseTransformPoint(Camera.ScreenToWorldPoint(new Vector3(position.x, position.y, Camera.nearClipPlane),
                    Camera.MonoOrStereoscopicEye.Mono));

        private Vector2 FromCamera(Vector3 line) =>
            Camera == null ? line : Camera.WorldToScreenPoint(Camera.transform.TransformPoint(line), Camera.MonoOrStereoscopicEye.Mono);

        private void UpdatePose()
        {
            if (Camera == null)
                return;

            var ray = Camera.ScreenPointToRay(ScreenPosition, Camera.MonoOrStereoscopicEye.Mono);
            Pose = new Pose(ray.origin, Quaternion.LookRotation(ray.direction));
        }
    }
}
