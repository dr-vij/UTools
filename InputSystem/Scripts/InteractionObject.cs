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
    public class InteractionObject : MonoBehaviour
    {
        private Dictionary<InteractionEvent, List<InteractionSubscribtion>> mAllSubscribtions = new Dictionary<InteractionEvent, List<InteractionSubscribtion>>();

        public bool HasEventSubscribtion(InteractionEvent evt)
        {
            return mAllSubscribtions.TryGetValue(evt, out var subscribtions) && subscribtions.Count != 0;
        }

        public void Subscribe<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler) where TEventArgs : InteractionEventArgs
        {
            if (!mAllSubscribtions.TryGetValue(evt, out var subscribtions))
            {
                subscribtions = new List<InteractionSubscribtion>();
                mAllSubscribtions.Add(evt, subscribtions);
            }
            subscribtions.Add(new InteractionSubscribtion(handler));
        }

        public void Unsubscribe<TEventArgs>(InteractionEvent evt, EventHandler<TEventArgs> handler) where TEventArgs : InteractionEventArgs
        {
            if (mAllSubscribtions.TryGetValue(evt, out var subscribtions))
            {
                subscribtions.RemoveAll((subscribtion) => subscribtion.Handler == (Delegate)handler);
            }
        }

        protected virtual void Awake()
        {
            //We check here that only one InteractionObject existst on this gameobject
            var interactionObjects = GetComponentsInChildren<InteractionObject>();
            if (interactionObjects.Where(c => c.transform == transform).Count() != 1)
            {
                Debug.LogError($"Several InteractionObject scripts found at one gameobject {gameObject.name}", gameObject);
            }
        }
    }
}
