using System;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    /// <summary>
    /// A pinch of a tracked hand, from the moment the fingers close to the moment they part. Its source says what the
    /// hand is on: near something (Near), the pointer stands at the pinch and its Pose is a ray from the pinch along the
    /// normal of the working plane, so the pose meets every plane parallel to it at the hand's own projection; away
    /// from everything (Ray), the Pose is the aim ray of the hand. The travel is counted in the units the hand is
    /// steady in. A pinch within reach travels in metres of the pinch, and a roll of the pinch counts as a move: the
    /// roll threshold is as far as the drag threshold, so a wrist rolled on a knob makes a drag as a moved hand does.
    /// A ray travels in degrees of its turn, not in metres at its far end, where a small shake of the hand is a long way.
    /// </summary>
    public sealed class HandPointer : InteractionPointer
    {
        /// <summary>
        /// The ids of the hands, one per hand from here up, HandId(0) to HandId(MaxHands - 1). Negative, as the mouse's are
        /// (Helpers: -1 to -3), and below them, just over the pen's (Helpers.PenInputId, int.MinValue): the touches of the
        /// Input System, numbered up from 1, never come here. A hand makes one pinch at a time; its hover and its pinch
        /// share the id, as the mouse's do.
        /// </summary>
        public const int FirstHandId = int.MinValue + 1;

        /// <summary>How many hands have an id: the two of a person.</summary>
        public const int MaxHands = 2;

        // Metres of travel a degree of roll counts as: the drag threshold over the roll threshold. Zero for a ray and for
        // a pointer without a roll threshold.
        private readonly float m_MetresPerDegree;

        private HandPointer(int id, Pose gripPose, Pose pose, bool isNear, float dragThreshold, float metresPerDegree,
            IReadOnlyList<PointerHit> hits, InteractionObjectBase fallback)
            : base(id, PointerKind.Hand, PointerButton.Primary, dragThreshold, hits, fallback)
        {
            IsNear = isNear;
            m_MetresPerDegree = metresPerDegree;
            GripPose = gripPose;
            Pose = pose;
            RebaseTravel();
        }

        /// <summary>The id of the hand of the index, 0 to MaxHands - 1.</summary>
        public static int HandId(int hand)
        {
            if (hand < 0 || hand >= MaxHands)
                throw new ArgumentOutOfRangeException(nameof(hand), hand, $"a hand is 0 to {MaxHands - 1}");
            return FirstHandId + hand;
        }

        /// <summary>
        /// A pinch within reach of something. It is a drag once the pinch moves farther than
        /// <paramref name="dragThreshold"/>, metres, or rolls farther than <paramref name="rollThreshold"/>, degrees; a
        /// roll threshold of zero or less leaves the roll out. A pinch within reach of the wall but of nothing on it
        /// has no hits and a <paramref name="fallback"/> receiver, as a pointer of the screen on the empty wall has.
        /// </summary>
        public static HandPointer Near(int id, Pose gripPose, Pose pose, float dragThreshold, float rollThreshold,
            IReadOnlyList<PointerHit> hits, InteractionObjectBase fallback = null) =>
            new(id, gripPose, pose, true, dragThreshold, rollThreshold > 0f ? dragThreshold / rollThreshold : 0f, hits, fallback);

        /// <summary>A pinch along the aim ray of the hand. It is a drag once the ray turns farther than <paramref name="dragAngle"/>, degrees.</summary>
        public static HandPointer Ray(int id, Pose gripPose, Pose aimPose, float dragAngle, IReadOnlyList<PointerHit> hits,
            InteractionObjectBase fallback) =>
            new(id, gripPose, aimPose, false, dragAngle, 0f, hits, fallback);

        /// <summary>The pinch: where the fingers meet and how the hand is turned. What is taken into the hand follows it.</summary>
        public Pose GripPose { get; private set; }

        /// <summary>The pinch closed on something within reach, not on the aim ray.</summary>
        public bool IsNear { get; }

        /// <summary>
        /// The pose where the travel is counted from: where the pinch closed, or where it was when another pointer
        /// joined or left it (RebaseTravel).
        /// </summary>
        public Pose TrackStartPose { get; private set; }

        /// <summary>The pinch where the travel is counted from.</summary>
        public Pose TrackStartGripPose { get; private set; }

        /// <summary>Metres of the pinch, its roll counted as metres, within reach; degrees of the ray's turn along the ray.</summary>
        public override float Travel =>
            IsNear
                ? Mathf.Max(Vector3.Distance(GripPose.position, TrackStartGripPose.position),
                    Quaternion.Angle(GripPose.rotation, TrackStartGripPose.rotation) * m_MetresPerDegree)
                : Vector3.Angle(TrackStartPose.forward, Pose.forward);

        public override void RebaseTravel()
        {
            TrackStartPose = Pose;
            TrackStartGripPose = GripPose;
        }

        public void MoveTo(Pose gripPose, Pose pose)
        {
            GripPose = gripPose;
            Pose = pose;
        }
    }
}
