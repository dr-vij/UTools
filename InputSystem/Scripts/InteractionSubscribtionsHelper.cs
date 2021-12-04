using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public static class InteractionSubscribtionsHelper
    {
        /// <summary>
        /// Unified pointer press subscription
        /// </summary>
        /// <param name="interactionObject"></param>
        /// <param name="handler"></param>
        public static DisposableAction SubscribePressEvent(this InteractionObject interactionObject, EventHandler<PointerInteractionEventArgs> handler)
        {
            interactionObject.Subscribe(InteractionEvents.InteractionPressEvent, handler);
            return new DisposableAction(() => interactionObject.Unsubscribe(InteractionEvents.InteractionPressEvent, handler));
        }

        /// <summary>
        /// Unified pointer drag subscription
        /// </summary>
        /// <param name="interactionObject"></param>
        /// <param name="pointerDragStartHandler"></param>
        /// <param name="pointerDragHandler"></param>
        /// <param name="pointerDragEndHandler"></param>
        public static DisposableAction SubscribePointerDragEvent(this InteractionObject interactionObject,
            EventHandler<PointerDragInteractionEventArgs> pointerDragStartHandler,
            EventHandler<PointerDragInteractionEventArgs> pointerDragHandler,
            EventHandler<PointerDragInteractionEventArgs> pointerDragEndHandler)
        {
            interactionObject.Subscribe(InteractionEvents.InteractionDragStartEvent, pointerDragStartHandler);
            interactionObject.Subscribe(InteractionEvents.InteractionDragEvent, pointerDragHandler);
            interactionObject.Subscribe(InteractionEvents.InteractionDragEndEvent, pointerDragEndHandler);

            return new DisposableAction(() =>
            {
                interactionObject.Unsubscribe(InteractionEvents.InteractionDragStartEvent, pointerDragStartHandler);
                interactionObject.Unsubscribe(InteractionEvents.InteractionDragEvent, pointerDragHandler);
                interactionObject.Unsubscribe(InteractionEvents.InteractionDragEndEvent, pointerDragEndHandler);
            });
        }

        /// <summary>
        /// Unified pointer move subscription
        /// </summary>
        /// <param name="interactionObject"></param>
        /// <param name="pointerMoveHandler"></param>
        public static DisposableAction SubscribePointerMoveEvent(this InteractionObject interactionObject, EventHandler<PointerInteractionEventArgs> pointerMoveHandler)
        {
            interactionObject.Subscribe(InteractionEvents.InteractionPointerMoveEvent, pointerMoveHandler);
            return new DisposableAction(() => interactionObject.Unsubscribe(InteractionEvents.InteractionPointerMoveEvent, pointerMoveHandler));
        }
    }
}