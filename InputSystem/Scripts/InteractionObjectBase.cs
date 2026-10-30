using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTools.Input
{
    /// <summary>
    /// Interaction object. it is used for all interaction subscriptions
    /// </summary>
    public abstract class InteractionObjectBase : DisposableMonoBehaviour
    {
        //An array is replaced, never changed, on subscribe and unsubscribe: a handler may do either while the event runs.
        private readonly Dictionary<InteractionEvent, InteractionSubscription[]> m_AllSubscriptions = new();

        /// <summary>
        /// The object lies over others: a gesture it does not take is offered to the interaction object beneath it.
        /// Without the mark an untaken gesture goes straight to the fallback receiver of the pointer.
        /// </summary>
        public bool PassesGesturesBeneath { get; set; }

        /// <summary>
        /// The object is a control that does not need the whole of the input to itself: while it holds a pointer, a
        /// source that leads one gesture at a time may still lead pointers to other such objects. Read by the rule a
        /// source admits pointers by, not by the recognizer.
        /// </summary>
        public bool AllowsConcurrentGestures { get; set; }

        /// <summary>
        /// The object is pressed by a touch: a source with fingertips (the hands of a headset) presses it when a tip
        /// comes up to it, without a pinch or a click. A button sets it; a body, a knob or a cable is never poked.
        /// </summary>
        public bool IsPokeable { get; set; }

        /// <summary>
        /// The object takes no drag for itself - locked, or a fixture - and leaves the drags that start on it to what is
        /// beneath or to the pointer's fallback. A source with coarse pointers reads it: what such a pointer meets on the
        /// object within its slop - a control standing on it - is what the pointer means, since the object itself cannot
        /// be taken.
        /// </summary>
        public bool LeavesDrags { get; set; }

        public bool HasEventSubscription(InteractionEvent evt)
        {
            return m_AllSubscriptions.TryGetValue(evt, out var subscriptions) && subscriptions.Length != 0;
        }

        /// <summary>
        /// Subscribes to the event of the pointers with the primary button, of any kind.
        /// </summary>
        public IDisposable Subscribe<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler, bool handleEvent = true, bool ignoreHandled = false)
            where TEventArgs : InteractionEventArgs
        {
            return Subscribe(evt, handler, PointerButton.Primary, null, handleEvent, ignoreHandled);
        }

        /// <summary>
        /// Subscribes to the event of the pointers with the given button and of the given kind; null is any.
        /// </summary>
        public IDisposable Subscribe<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler, PointerButton? button, PointerKind? kind,
            bool handleEvent = true, bool ignoreHandled = false)
            where TEventArgs : InteractionEventArgs
        {
            handler ??= (_, _) => { };
            var subscription = new InteractionSubscription<TEventArgs>(handler, handleEvent, ignoreHandled, button, kind);
            AddSubscription(evt, subscription);
            return new DisposeAction(() => RemoveSubscription(evt, subscription));
        }

        /// <summary>
        /// Runs the event on the handlers of this object: those the args are for, in the order they subscribed, each
        /// unless the event is handled already or the handler ignores that. Where an unhandled event goes next is for
        /// the caller to decide.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="args"></param>
        public void RunEvent(InteractionEvent evt, InteractionEventArgs args)
        {
            if (!m_AllSubscriptions.TryGetValue(evt, out var subscriptions))
                return;

            foreach (var subscription in subscriptions)
            {
                if (!subscription.Accepts(args))
                    continue;

                if (!args.IsHandled || subscription.IgnoreHandled)
                {
                    if (subscription.HandleEvents)
                        args.Handle(this);
                    subscription.Invoke(this, args);
                }
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            //We check here that only one InteractionObject exists on this gameObject
            var interactionObjects = GetComponentsInChildren<InteractionObjectBase>();
            if (interactionObjects.Count(c => c.transform == transform) != 1)
            {
                var obj = gameObject;
                Debug.LogError($"Several InteractionObject scripts found at one gameObject {obj.name}", obj);
            }
        }

        private void AddSubscription(InteractionEvent evt, InteractionSubscription subscription)
        {
            m_AllSubscriptions.TryGetValue(evt, out var subscriptions);
            var count = subscriptions?.Length ?? 0;
            var extended = new InteractionSubscription[count + 1];
            if (count != 0)
                Array.Copy(subscriptions, extended, count);
            extended[count] = subscription;
            m_AllSubscriptions[evt] = extended;
        }

        private void RemoveSubscription(InteractionEvent evt, InteractionSubscription subscription)
        {
            if (!m_AllSubscriptions.TryGetValue(evt, out var subscriptions))
                return;

            var index = Array.IndexOf(subscriptions, subscription);
            if (index < 0)
                return;

            var reduced = new InteractionSubscription[subscriptions.Length - 1];
            Array.Copy(subscriptions, 0, reduced, 0, index);
            Array.Copy(subscriptions, index + 1, reduced, index, reduced.Length - index);
            m_AllSubscriptions[evt] = reduced;
        }
    }
}
