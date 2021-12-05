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
        private Dictionary<ObjectInteractionEvent, List<InteractionSubscribtion>> mAllSubscribtions = new Dictionary<ObjectInteractionEvent, List<InteractionSubscribtion>>();

        public bool HasEventSubscribtion(ObjectInteractionEvent evt)
        {
            return mAllSubscribtions.TryGetValue(evt, out var subscribtions) && subscribtions.Count != 0;
        }

        public void Subscribe<TEventArgs>(ObjectInteractionEvent evt, EventHandler<TEventArgs> handler) where TEventArgs : InteractionEventArgs
        {
            if (!mAllSubscribtions.TryGetValue(evt, out var subscribtions))
            {
                subscribtions = new List<InteractionSubscribtion>();
                mAllSubscribtions.Add(evt, subscribtions);
            }
            subscribtions.Add(new InteractionSubscribtion(handler));
        }

        public void Unsubscribe<TEventArgs>(ObjectInteractionEvent evt, EventHandler<TEventArgs> handler) where TEventArgs : InteractionEventArgs
        {
            if (mAllSubscribtions.TryGetValue(evt, out var subscribtions))
            {
                subscribtions.RemoveAll((subscribtion) => subscribtion.Handler == (Delegate)handler);
            }
        }

        public void RunEvent(ObjectInteractionEvent evt, InteractionEventArgs args)
        {
            if (mAllSubscribtions.TryGetValue(evt, out var handlers))
            {
                args.Handle(this);
                foreach(var handler in handlers)
                    handler.Handler.DynamicInvoke(this, args);
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
    }
}
