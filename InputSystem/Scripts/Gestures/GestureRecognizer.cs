using System;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    /// <summary>
    /// Turns pointers into gestures and gives the gestures to interaction objects.
    ///
    /// Every pointer is led on its own and any number of them at once: a grab when it goes down, a press if it lifts
    /// where it stood, a drag once it has travelled its threshold. A gesture is offered down the path of the pointer -
    /// the objects under it as far as the first that does not pass gestures beneath - and then to its fallback
    /// receiver; the first whose handlers handle the start takes the gesture and alone gets the rest of it. An object
    /// belongs to one pointer at a time: while it holds a grab or a gesture, a pointer of another stops at it.
    ///
    /// Two pointers can be joined into a pair, which makes one gesture of the two. Whether a pointer is led at all,
    /// alone or in a pair, is for its source to decide.
    /// </summary>
    public sealed class GestureRecognizer
    {
        private enum Phase
        {
            //Down, has not travelled its threshold.
            Pending,

            //Its drag is taken.
            Dragging,

            //Nothing more comes of it until it lifts.
            Idle,
        }

        private sealed class Contact
        {
            public InteractionPointer Pointer;
            public readonly List<PointerHit> Path = new();
            public Phase Phase;

            //The receivers the grab went to, which are owed the release, and the one whose handlers handled it.
            public InteractionObjectBase GrabTop;
            public bool IsGrabAtFallback;
            public InteractionObjectBase GrabHolder;

            public InteractionObjectBase DragTaker;
            public Vector3? DragHitPoint;
            public bool IsTapCancelled;

            //Left alone by a pair that led a gesture: its drag may go only to the one that took that gesture.
            public bool IsBound;
            public InteractionObjectBase Leader;

            public Pair Pair;
        }

        private sealed class Pair
        {
            public Contact First;
            public Contact Second;
            public Phase Phase;
            public InteractionObjectBase Taker;

            public Contact Other(Contact contact) => contact == First ? Second : First;
        }

        private readonly List<Contact> m_Contacts = new();
        private readonly List<InteractionPointer> m_Pointers = new();
        private readonly Dictionary<InteractionPointer, InteractionObjectBase> m_Hovered = new();
        private readonly List<PointerHit> m_SharedPath = new();

        /// <summary>
        /// The pointers being led, in the order they went down.
        /// </summary>
        public IReadOnlyList<InteractionPointer> Pointers => m_Pointers;

        public bool IsTracked(InteractionPointer pointer) => Find(pointer) != null;

        public bool IsPaired(InteractionPointer pointer) => Find(pointer)?.Pair != null;

        /// <summary>
        /// The object under the pointer that holds it: the one that handled its grab or took its gesture, alone or in a
        /// pair. Null while nothing holds it, and the fallback receiver does not count.
        /// </summary>
        public InteractionObjectBase HeldBy(InteractionPointer pointer)
        {
            var contact = Find(pointer);
            if (contact == null)
                return null;

            var holder = contact.GrabHolder != null ? contact.GrabHolder : contact.Pair != null ? contact.Pair.Taker : contact.DragTaker;
            return holder != null && holder != pointer.Fallback ? holder : null;
        }

        /// <summary>
        /// Starts leading a pointer that has just gone down, on its own. The grab goes to the object on top of those
        /// under the pointer; a source that left that object out of the pointer's list passes no grab at all.
        /// </summary>
        public void Track(InteractionPointer pointer, bool grab = true)
        {
            var contact = Add(pointer);
            if (grab)
                Grab(contact);
        }

        /// <summary>
        /// Whether a pointer being led can take a partner: it is in no pair and no object holds it. Its drag at its
        /// fallback receiver, if there is one, ends when the partner joins.
        /// </summary>
        public bool CanJoin(InteractionPointer partner)
        {
            var contact = Find(partner);
            return contact != null && contact.Pair == null && HeldBy(partner) == null;
        }

        /// <summary>
        /// Starts leading a pointer that has just gone down as the second of a pair with one already led. The newcomer
        /// gets no grab and neither of the two a press.
        /// </summary>
        public void Join(InteractionPointer pointer, InteractionPointer partner)
        {
            if (!CanJoin(partner))
                throw new InvalidOperationException("The partner is not led, is in a pair already or is held by an object");

            var first = Find(partner);
            if (first.Phase == Phase.Dragging)
                EndDrag(first);

            var second = Add(pointer);
            var pair = new Pair { First = first, Second = second, Phase = Phase.Pending };
            first.Pair = pair;
            second.Pair = pair;
            first.Phase = Phase.Pending;
            first.IsTapCancelled = true;
            second.IsTapCancelled = true;
            first.IsBound = false;
            first.Leader = null;
            partner.RebaseTravel();
            pointer.RebaseTravel();
        }

        /// <summary>
        /// The pointer has moved: its source has already given it its new place.
        /// </summary>
        public void Move(InteractionPointer pointer)
        {
            var contact = Find(pointer);
            if (contact == null)
                return;

            if (contact.Pair != null)
            {
                MovePair(contact);
                return;
            }

            switch (contact.Phase)
            {
                case Phase.Pending:
                    if (pointer.Travel > pointer.DragThreshold)
                        StartDrag(contact);
                    break;
                case Phase.Dragging:
                    PerformDrag(contact);
                    break;
            }
        }

        /// <summary>
        /// The pointer has lifted.
        /// </summary>
        public void Lift(InteractionPointer pointer) => Remove(pointer, isCancelled: false);

        /// <summary>
        /// The pointer is gone without a lift. A gesture under way ends as on a lift, where the pointer last was; a
        /// press is not made.
        /// </summary>
        public void Cancel(InteractionPointer pointer) => Remove(pointer, isCancelled: true);

        /// <summary>
        /// A pointer, down or not, led or not, is over an object, or over none (null). The object gets an enter when the
        /// first pointer comes over it and an exit when the last one leaves; when to tell is for the source to decide.
        /// </summary>
        public void Hover(InteractionPointer pointer, InteractionObjectBase target)
        {
            m_Hovered.TryGetValue(pointer, out var previous);
            if (ReferenceEquals(previous, target))
                return;

            if (target != null)
                m_Hovered[pointer] = target;
            else
                m_Hovered.Remove(pointer);

            if (previous != null && HoverCount(previous) == 0)
                previous.RunEvent(PointerInteractionEvents.PointerExitEvent, PointArgs(pointer));
            if (target != null && HoverCount(target) == 1)
                target.RunEvent(PointerInteractionEvents.PointerEnterEvent, PointArgs(pointer));
        }

        private int HoverCount(InteractionObjectBase target)
        {
            var count = 0;
            foreach (var hovered in m_Hovered.Values)
            {
                if (hovered == target)
                    count++;
            }

            return count;
        }

        #region One pointer

        private void Grab(Contact contact)
        {
            var pointer = contact.Pointer;
            var args = PointArgs(pointer);
            if (contact.Path.Count != 0)
            {
                var top = contact.Path[0];
                //The object under the pointer is another pointer's: nothing comes of this one.
                if (IsHeldByOther(top.Target, contact))
                {
                    contact.Phase = Phase.Idle;
                    contact.IsTapCancelled = true;
                    return;
                }

                args.HitPoint = top.Point;
                top.Target.RunEvent(PointerInteractionEvents.PointerGrabEvent, args);
                contact.GrabTop = top.Target;
                if (args.IsHandled)
                    contact.GrabHolder = top.Target;
            }

            if (args.IsHandled || !CanGoTo(pointer.Fallback, contact))
                return;

            args.HitPoint = null;
            pointer.Fallback.RunEvent(PointerInteractionEvents.PointerGrabEvent, args);
            contact.IsGrabAtFallback = true;
            if (args.IsHandled)
                contact.GrabHolder = pointer.Fallback;
        }

        private void Release(Contact contact)
        {
            var pointer = contact.Pointer;
            var args = PointArgs(pointer);
            if (contact.GrabTop != null)
            {
                args.HitPoint = contact.Path[0].Point;
                contact.GrabTop.RunEvent(PointerInteractionEvents.PointerReleaseEvent, args);
            }

            if (contact.IsGrabAtFallback && pointer.Fallback != null)
            {
                args.HitPoint = null;
                pointer.Fallback.RunEvent(PointerInteractionEvents.PointerReleaseEvent, args);
            }
        }

        private void Press(Contact contact)
        {
            var args = PointArgs(contact.Pointer);
            Offer(contact, contact.Path, contact.Pointer.Fallback, PointerInteractionEvents.PointerPressEvent, args, out _);
        }

        private void StartDrag(Contact contact)
        {
            var pointer = contact.Pointer;
            var args = DragArgs(pointer, pointer.TrackStartScreenPosition);
            InteractionObjectBase taker;
            Vector3? hitPoint;
            if (contact.IsBound)
                taker = OfferTo(contact.Leader, contact, PointerInteractionEvents.PointerDragStartEvent, args, out hitPoint);
            else
                taker = Offer(contact, contact.Path, pointer.Fallback, PointerInteractionEvents.PointerDragStartEvent, args, out hitPoint);

            contact.DragTaker = taker;
            contact.DragHitPoint = hitPoint;
            contact.Phase = taker != null ? Phase.Dragging : Phase.Idle;
        }

        private void PerformDrag(Contact contact)
        {
            //The taker was destroyed under the gesture: the gesture is over, and is not offered anew.
            if (contact.DragTaker == null)
            {
                contact.Phase = Phase.Idle;
                return;
            }

            var args = DragArgs(contact.Pointer, contact.Pointer.PreviousScreenPosition);
            args.HitPoint = contact.DragHitPoint;
            contact.DragTaker.RunEvent(PointerInteractionEvents.PointerDragPerformEvent, args);
        }

        private void EndDrag(Contact contact)
        {
            if (contact.DragTaker != null)
            {
                var args = DragArgs(contact.Pointer, contact.Pointer.PreviousScreenPosition);
                args.HitPoint = contact.DragHitPoint;
                contact.DragTaker.RunEvent(PointerInteractionEvents.PointerDragEndEvent, args);
            }

            contact.DragTaker = null;
            contact.Phase = Phase.Idle;
        }

        private void Remove(InteractionPointer pointer, bool isCancelled)
        {
            var contact = Find(pointer);
            if (contact == null)
                return;

            if (contact.Pair != null)
                LeavePair(contact);
            else if (contact.Phase == Phase.Dragging)
                EndDrag(contact);
            else if (contact.Phase == Phase.Pending && !contact.IsTapCancelled && !isCancelled)
                Press(contact);

            Release(contact);
            m_Contacts.Remove(contact);
            m_Pointers.Remove(pointer);
        }

        #endregion

        #region Pair

        private void MovePair(Contact moved)
        {
            var pair = moved.Pair;
            pair.Other(moved).Pointer.SettleStep();
            switch (pair.Phase)
            {
                case Phase.Pending:
                    var first = pair.First.Pointer;
                    var second = pair.Second.Pointer;
                    if (first.Travel > first.DragThreshold || second.Travel > second.DragThreshold)
                        StartPair(pair);
                    break;
                case Phase.Dragging:
                    if (pair.Taker == null)
                        pair.Phase = Phase.Idle;
                    else
                        pair.Taker.RunEvent(PointerInteractionEvents.TwoPointersDragPerformEvent, PairArgs(pair));
                    break;
            }
        }

        private void StartPair(Pair pair)
        {
            //The objects under both pointers, in the order of the first.
            m_SharedPath.Clear();
            foreach (var hit in pair.First.Path)
            {
                foreach (var other in pair.Second.Path)
                {
                    if (other.Target == hit.Target)
                    {
                        m_SharedPath.Add(hit);
                        break;
                    }
                }
            }

            var fallback = pair.First.Pointer.Fallback == pair.Second.Pointer.Fallback ? pair.First.Pointer.Fallback : null;
            var args = PairArgs(pair);
            pair.Taker = Offer(pair, m_SharedPath, fallback, PointerInteractionEvents.TwoPointersDragStartEvent, args, out _);
            pair.Phase = pair.Taker != null ? Phase.Dragging : Phase.Idle;
        }

        private void LeavePair(Contact leaving)
        {
            var pair = leaving.Pair;
            var staying = pair.Other(leaving);
            var hasLed = pair.Phase == Phase.Dragging;
            if (hasLed && pair.Taker != null)
                pair.Taker.RunEvent(PointerInteractionEvents.TwoPointersDragEndEvent, PairArgs(pair));

            leaving.Pair = null;
            staying.Pair = null;
            staying.Phase = Phase.Pending;
            staying.IsBound = hasLed;
            staying.Leader = hasLed ? pair.Taker : null;
            staying.Pointer.RebaseTravel();
        }

        #endregion

        #region Offering

        /// <summary>
        /// Offers the start of a gesture down a path and then to the fallback receiver. Returns the one that took it,
        /// or null: nobody did, or the offer stopped at an object another pointer holds.
        /// </summary>
        private InteractionObjectBase Offer(object owner, List<PointerHit> path, InteractionObjectBase fallback, InteractionEvent evt,
            PointerInteractionEventArgs args, out Vector3? hitPoint)
        {
            hitPoint = null;
            foreach (var hit in path)
            {
                //Destroyed since the pointer went down.
                if (hit.Target == null)
                    continue;
                if (IsHeldByOther(hit.Target, owner))
                    return null;

                args.HitPoint = hit.Point;
                hit.Target.RunEvent(evt, args);
                if (args.IsHandled)
                {
                    hitPoint = hit.Point;
                    return hit.Target;
                }
            }

            if (!CanGoTo(fallback, owner))
                return null;

            args.HitPoint = null;
            fallback.RunEvent(evt, args);
            return args.IsHandled ? fallback : null;
        }

        //Offers the start of a gesture of a pointer to one receiver only.
        private InteractionObjectBase OfferTo(InteractionObjectBase receiver, Contact contact, InteractionEvent evt, PointerInteractionEventArgs args,
            out Vector3? hitPoint)
        {
            hitPoint = null;
            if (!CanGoTo(receiver, contact))
                return null;

            foreach (var hit in contact.Path)
            {
                if (hit.Target == receiver)
                    hitPoint = hit.Point;
            }

            args.HitPoint = hitPoint;
            receiver.RunEvent(evt, args);
            return args.IsHandled ? receiver : null;
        }

        private bool CanGoTo(InteractionObjectBase receiver, object owner) => receiver != null && !IsHeldByOther(receiver, owner);

        //An object belongs to the pointer, or the pair, whose grab it handled or whose gesture it took.
        private bool IsHeldByOther(InteractionObjectBase receiver, object owner)
        {
            foreach (var contact in m_Contacts)
            {
                var isOwn = contact == owner || (contact.Pair != null && contact.Pair == owner);
                if (isOwn)
                    continue;
                if (contact.GrabHolder == receiver || contact.DragTaker == receiver)
                    return true;
                if (contact.Pair != null && contact.Pair.Taker == receiver)
                    return true;
            }

            return false;
        }

        #endregion

        #region Contacts and args

        private Contact Find(InteractionPointer pointer)
        {
            foreach (var contact in m_Contacts)
            {
                if (contact.Pointer == pointer)
                    return contact;
            }

            return null;
        }

        private Contact Add(InteractionPointer pointer)
        {
            if (Find(pointer) != null)
                throw new InvalidOperationException("The pointer is led already");

            var contact = new Contact { Pointer = pointer, Phase = Phase.Pending };
            //The path ends at the first object that does not pass gestures on to what is beneath it.
            foreach (var hit in pointer.Hits)
            {
                if (hit.Target == null)
                    continue;

                contact.Path.Add(hit);
                if (!hit.Target.PassesGesturesBeneath)
                    break;
            }

            m_Contacts.Add(contact);
            m_Pointers.Add(pointer);
            return contact;
        }

        private static PointerInteractionEventArgs PointArgs(InteractionPointer pointer)
        {
            var args = pointer.Kind == PointerKind.Mouse
                ? new MouseInteractionEventArgs(pointer.ScreenPosition, MouseButtonId(pointer.Button), pointer.Camera)
                : new PointerInteractionEventArgs(pointer.ScreenPosition, pointer.Camera);
            args.Pointer = pointer;
            return args;
        }

        private static PointerDragInteractionEventArgs DragArgs(InteractionPointer pointer, Vector2 from)
        {
            var args = pointer.Kind == PointerKind.Mouse
                ? new MouseDragInteractionEventArgs(pointer.ScreenPosition, from, MouseButtonId(pointer.Button), pointer.Camera)
                : new PointerDragInteractionEventArgs(pointer.ScreenPosition, from, pointer.Camera);
            args.Pointer = pointer;
            return args;
        }

        private static TwoPointersDragInteractionEventArgs PairArgs(Pair pair)
        {
            //The same order whichever of the two moves.
            var (first, second) = pair.First.Pointer.Id < pair.Second.Pointer.Id
                ? (pair.First.Pointer, pair.Second.Pointer)
                : (pair.Second.Pointer, pair.First.Pointer);
            return new TwoPointersDragInteractionEventArgs(first.ScreenPosition, first.PreviousScreenPosition, second.ScreenPosition,
                second.PreviousScreenPosition, first.Camera) { Pointer = first };
        }

        private static int MouseButtonId(PointerButton button)
        {
            return button switch
            {
                PointerButton.Secondary => Helpers.RightMouseInputId,
                PointerButton.Middle => Helpers.MiddleMouseInputId,
                _ => Helpers.LeftMouseInputId,
            };
        }

        #endregion
    }
}
