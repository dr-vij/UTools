using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public static class InteractionObjectHelpers
    {
        #region InteractionObjects and InteractionIgnorer helpers

        /// <summary>
        /// buffer for effective usage of getcomponent
        /// </summary>
        private static List<Transform> mTransformsBuffer = new List<Transform>(100);

        /// <summary>
        /// Checks if Transform has InteractionIgnorer
        /// </summary>
        /// <param name="unityTransform"></param>
        /// <returns></returns>
        public static bool HasInteractionIgnorer(this Transform unityTransform) => HasInteractionIgnorer(unityTransform.gameObject);

        /// <summary>
        /// Checks if Unity object has InteractionIgnorer
        /// </summary>
        /// <param name="unityObject"></param>
        /// <returns></returns>
        public static bool HasInteractionIgnorer(this GameObject unityObject) => unityObject.TryGetComponent<InteractionIgnorer>(out _);

        /// <summary>
        /// Finds first interaction object on unityObject starting from given and up to parents
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryFindInteractionObject(this GameObject unityObject, out InteractionObject result)
        {
            var wasFound = false;
            var currentTransform = unityObject.transform;
            result = null;

            while (currentTransform != null && !wasFound)
            {
                wasFound = currentTransform.TryGetComponent(out result);
                currentTransform = currentTransform.parent;
            }

            return wasFound;
        }

        /// <summary>
        /// Finds first interaction object on unityTransform starting from given and up to parents
        /// </summary>
        /// <param name="unityTransform"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryFindInteractionObject(this Transform unityTransform, out InteractionObject result) => TryFindInteractionObject(unityTransform.gameObject, out result);

        /// <summary>
        /// Adds Interaction Ignorer to gameobject if it does not have it
        /// </summary>
        /// <param name="unityObject"></param>
        public static void MakeObjectIgnorer(this GameObject unityObject)
        {
            if (!unityObject.HasInteractionIgnorer())
                unityObject.AddComponent<InteractionIgnorer>();
        }

        /// <summary>
        /// Adds InteractionIgnorer to transform if it does not have it
        /// </summary>
        /// <param name="unityTransform"></param>
        public static void MakeObjectIgnorer(this Transform unityTransform) => MakeObjectIgnorer(unityTransform.gameObject);

        /// <summary>
        /// Removes interaction ignorers from gameobject if it has them
        /// </summary>
        /// <param name="unityObject"></param>
        public static void UnmakeObjectIgnorer(this GameObject unityObject)
        {
            while (unityObject.TryGetComponent<InteractionIgnorer>(out var ignorer))
                UnityEngine.Object.Destroy(ignorer);
        }

        /// <summary>
        /// Removes interaction ignorers from gameobject if it has them
        /// </summary>
        /// <param name="unityTransform"></param>
        public static void UnmakeObjectIgnorer(this Transform unityTransform) => UnmakeObjectIgnorer(unityTransform.gameObject);

        /// <summary>
        /// Adds Interaction Ignorers to all objects in the tree
        /// </summary>
        /// <param name="unityObject"></param>
        public static void MakeTreeInteractionIgnorer(this GameObject unityObject, bool includeInactive = true)
        {
            unityObject.GetComponentsInChildren(includeInactive, mTransformsBuffer);
            foreach (var t in mTransformsBuffer)
                MakeObjectIgnorer(t.gameObject);
        }

        /// <summary>
        /// Adds Interaction Ignorers to all transforms in the tree
        /// </summary>
        /// <param name="unityTransform"></param>
        /// <param name="includeInactive"></param>
        public static void MakeTreeInteractionIgnorer(this Transform unityTransform, bool includeInactive = true) => MakeTreeInteractionIgnorer(unityTransform.gameObject, includeInactive);

        /// <summary>
        /// Removes Interaction Ignorers from all objects in the tree
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="includeInactive"></param>
        public static void UnmakeTreeInteractionIgnorer(this GameObject unityObject, bool includeInactive = true)
        {
            unityObject.GetComponentsInChildren(includeInactive, mTransformsBuffer);
            foreach (var t in mTransformsBuffer)
                UnmakeObjectIgnorer(t.gameObject);
        }

        /// <summary>
        /// Removes Interaction Ignorers from all objects in the tree
        /// </summary>
        /// <param name="unityTransform"></param>
        /// <param name="includeInactive"></param>
        public static void UnmakeTreeInteractionIgnorer(this Transform unityTransform, bool includeInactive = true) => UnmakeTreeInteractionIgnorer(unityTransform.gameObject, includeInactive);

        #endregion

        #region Subscribtions
        /// <summary>
        /// Unified pointer press subscription
        /// </summary>
        /// <param name="interactionObject"></param>
        /// <param name="handler"></param>
        public static DisposableAction SubscribePressEvent(this InteractionObject interactionObject, EventHandler<PointerInteractionEventArgs> handler)
        {
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerPressEvent, handler);
            return new DisposableAction(() => interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerPressEvent, handler));
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
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerDragStartEvent, pointerDragStartHandler);
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerDragEvent, pointerDragHandler);
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerDragEndEvent, pointerDragEndHandler);

            return new DisposableAction(() =>
            {
                interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerDragStartEvent, pointerDragStartHandler);
                interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerDragEvent, pointerDragHandler);
                interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerDragEndEvent, pointerDragEndHandler);
            });
        }

        /// <summary>
        /// Unified grab/grab end subscription
        /// </summary>
        /// <param name="interactionObject"></param>
        /// <param name="pointerGrabStartHandler"></param>
        /// <param name="pointerGrabEndHandler"></param>
        /// <returns></returns>
        public static DisposableAction SubscribePointerGrabEvent(this InteractionObject interactionObject,
            EventHandler<PointerInteractionEventArgs> pointerGrabStartHandler,
            EventHandler<PointerInteractionEventArgs> pointerGrabEndHandler)
        {
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerGrabStartEvent, pointerGrabStartHandler);
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerGrabEndEvent, pointerGrabEndHandler);

            return new DisposableAction(() =>
            {
                interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerGrabStartEvent, pointerGrabStartHandler);
                interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerGrabEndEvent, pointerGrabEndHandler);
            });
        }

        /// <summary>
        /// Unified pointer move subscription
        /// </summary>
        /// <param name="interactionObject"></param>
        /// <param name="pointerMoveHandler"></param>
        public static DisposableAction SubscribePointerMoveEvent(this InteractionObject interactionObject, EventHandler<PointerInteractionEventArgs> pointerMoveHandler)
        {
            interactionObject.Subscribe(ObjectInteractionEvents.ObjectPointerMoveEvent, pointerMoveHandler);
            return new DisposableAction(() => interactionObject.Unsubscribe(ObjectInteractionEvents.ObjectPointerMoveEvent, pointerMoveHandler));
        }
        #endregion
    }
}