using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ViJTools
{
    /// <summary>
    /// Interaction object. it is used for all interaction subscriptions
    /// </summary>
    public class InteractionObject : DisposableMonobehaviour
    {
        private Dictionary<InteractionEvent, List<InteractionSubscribtion>> m_AllSubscribtions = new Dictionary<InteractionEvent, List<InteractionSubscribtion>>();

        [SerializeField] private bool m_IsSoloInteraction = true;

        public bool IsSoloInteraction => m_IsSoloInteraction;

        public PointerGestureAnalizer CreateAnalizer(Camera camera)
        {
            return new PointerGestureAnalizer(this, camera);
        }

        public bool HasEventSubscribtion(InteractionEvent evt)
        {
            return m_AllSubscribtions.TryGetValue(evt, out var subscribtions) && subscribtions.Count != 0;
        }

        public IDisposable Subscribe<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler, bool handleEvent = true, bool ignoreHandled = false) where TEventArgs : InteractionEventArgs
        {
            if (handler == null)
                handler = (obj, args) => { };
            AddSubscribtion(evt, handler, handleEvent, ignoreHandled);
            return new DisposableAction(() => RemoveSubscribtion(evt, handler));
        }

        /// <summary>
        /// Runs event on object if he has such subscribtion and if it can run it (handle condition/subscribtion check)
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="args"></param>
        public void RunEvent(InteractionEvent evt, InteractionEventArgs args)
        {
            if (m_AllSubscribtions.TryGetValue(evt, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    var isHandled = args.IsHandled;
                    if (!isHandled || handler.IgnoreHandled)
                    {
                        if (handler.HandleEvents)
                            args.Handle(this);
                        handler.Handler.DynamicInvoke(this, args);
                    }
                }
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            //We check here that only one InteractionObject existst on this gameobject
            var interactionObjects = GetComponentsInChildren<InteractionObject>();
            if (interactionObjects.Where(c => c.transform == transform).Count() != 1)
            {
                Debug.LogError($"Several InteractionObject scripts found at one gameobject {gameObject.name}", gameObject);
            }
        }

        private void AddSubscribtion<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler, bool handleEvent = true, bool ignoreHandled = false) where TEventArgs : InteractionEventArgs
        {
            if (!m_AllSubscribtions.TryGetValue(evt, out var subscribtions))
            {
                subscribtions = new List<InteractionSubscribtion>();
                m_AllSubscribtions.Add(evt, subscribtions);
            }
            subscribtions.Add(new InteractionSubscribtion(handler, handleEvent, ignoreHandled));
        }

        private void RemoveSubscribtion<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler) where TEventArgs : InteractionEventArgs
        {
            if (m_AllSubscribtions.TryGetValue(evt, out var subscribtions))
                subscribtions.RemoveAll((subscribtion) => subscribtion.Handler == (Delegate)handler);
        }
    }
}
