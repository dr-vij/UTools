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
        private readonly Dictionary<InteractionEvent, List<InteractionSubscribtion>> m_AllSubscriptions = new();

        public abstract IGestureAnalyzer CreateAnalyzer(Camera cam);

        public bool HasEventSubscription(InteractionEvent evt)
        {
            return m_AllSubscriptions.TryGetValue(evt, out var subscriptions) && subscriptions.Count != 0;
        }

        public IDisposable Subscribe<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler, bool handleEvent = true, bool ignoreHandled = false)
            where TEventArgs : InteractionEventArgs
        {
            handler ??= (_, _) => { };
            AddSubscription(evt, handler, handleEvent, ignoreHandled);
            return new DisposeAction(() => RemoveSubscription(evt, handler));
        }

        /// <summary>
        /// Runs event on object if he has such subscription and if it can run it (handle condition/subscription check)
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="args"></param>
        public void RunEvent(InteractionEvent evt, InteractionEventArgs args)
        {
            if (m_AllSubscriptions.TryGetValue(evt, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    if (!args.IsHandled || handler.IgnoreHandled)
                    {
                        if (handler.HandleEvents)
                            args.Handle(this);
                        handler.Handler.DynamicInvoke(this, args);
                    }
                }
            }

            if (args.InteractionCamera.TryGetComponent<InteractionObjectBase>(out var cameraInteractionObject) && cameraInteractionObject != this)
            {
                cameraInteractionObject.RunEvent(evt, args);
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

        private void AddSubscription<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler, bool handleEvent = true, bool ignoreHandled = false)
            where TEventArgs : InteractionEventArgs
        {
            if (!m_AllSubscriptions.TryGetValue(evt, out var subscriptions))
            {
                subscriptions = new List<InteractionSubscribtion>();
                m_AllSubscriptions.Add(evt, subscriptions);
            }

            subscriptions.Add(new InteractionSubscribtion(handler, handleEvent, ignoreHandled));
        }

        private void RemoveSubscription<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler) where TEventArgs : InteractionEventArgs
        {
            if (m_AllSubscriptions.TryGetValue(evt, out var subscriptions))
                subscriptions.RemoveAll((subscription) => subscription.Handler == (Delegate)handler);
        }
    }
}