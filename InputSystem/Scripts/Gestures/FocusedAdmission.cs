using System;
using System.Collections.Generic;

namespace UTools.Input
{
    /// <summary>
    /// The rule a source that leads one gesture at a time admits pointers by. It reads what the recognizer is leading
    /// and decides once, when a pointer goes down.
    ///
    /// A pointer held by an object - its grab handled or its gesture taken - has the input to itself: nothing else
    /// is led until it lifts. The controls (AllowsConcurrentGestures) are the exception: while a control holds a
    /// pointer, other pointers are led too, but only to other controls, and so on until all of them are up. A pointer
    /// nothing holds takes the next one as its partner: the two make a pair.
    /// </summary>
    public sealed class FocusedAdmission
    {
        private enum Admission
        {
            Ignore,
            Track,
            TrackAsControl,
            Join,
        }

        private readonly GestureRecognizer m_Recognizer;

        //The pointers led among the controls only: while one of them is down, so is every newcomer.
        private readonly HashSet<InteractionPointer> m_ControlPointers = new();

        public FocusedAdmission(GestureRecognizer recognizer) => m_Recognizer = recognizer;

        /// <summary>
        /// A pointer is about to go down over the given objects. Returns the pointer, made by
        /// <paramref name="create"/> and led by the recognizer from now on, or null: the pointer is not led for as long
        /// as it is down. A pointer led among the controls is made without the objects that are not controls and
        /// without a fallback receiver.
        /// </summary>
        public InteractionPointer Admit(PointerKind kind, PointerButton button, List<PointerHit> hits, InteractionObjectBase fallback,
            Func<IReadOnlyList<PointerHit>, InteractionObjectBase, InteractionPointer> create)
        {
            var landedOn = hits.Count != 0 ? hits[0].Target : null;
            InteractionPointer pointer;
            switch (Decide(kind, button, hits, out var partner))
            {
                case Admission.Track:
                    pointer = create(hits.ToArray(), fallback);
                    m_Recognizer.Track(pointer);
                    return pointer;
                case Admission.TrackAsControl:
                    pointer = create(hits.ToArray(), null);
                    m_ControlPointers.Add(pointer);
                    //The grab is of the object the pointer landed on, and of no control beneath it.
                    m_Recognizer.Track(pointer, grab: hits[0].Target == landedOn);
                    return pointer;
                case Admission.Join:
                    pointer = create(hits.ToArray(), fallback);
                    m_Recognizer.Join(pointer, partner);
                    return pointer;
                default:
                    return null;
            }
        }

        /// <summary>
        /// The pointer has lifted or is gone.
        /// </summary>
        public void Forget(InteractionPointer pointer) => m_ControlPointers.Remove(pointer);

        private Admission Decide(PointerKind kind, PointerButton button, List<PointerHit> hits, out InteractionPointer partner)
        {
            partner = null;
            var pointers = m_Recognizer.Pointers;
            if (pointers.Count == 0)
                return Admission.Track;

            var isAmongControls = false;
            foreach (var pointer in pointers)
            {
                var holder = m_Recognizer.HeldBy(pointer);
                if (holder != null && !holder.AllowsConcurrentGestures)
                    return Admission.Ignore;

                isAmongControls |= holder != null || m_ControlPointers.Contains(pointer);
            }

            if (isAmongControls)
            {
                hits.RemoveAll(hit => !hit.Target.AllowsConcurrentGestures);
                return hits.Count != 0 ? Admission.TrackAsControl : Admission.Ignore;
            }

            var only = pointers[0];
            var canPair = pointers.Count == 1 && m_Recognizer.CanJoin(only) && button == PointerButton.Primary &&
                          only.Button == PointerButton.Primary && only.Kind == kind;
            if (!canPair)
                return Admission.Ignore;

            partner = only;
            return Admission.Join;
        }
    }
}
