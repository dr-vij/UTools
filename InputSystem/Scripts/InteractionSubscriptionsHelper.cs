using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    public static class InteractionObjectHelpers
    {
        #region InteractionObjects and InteractionIgnorer helpers

        /// <summary>
        /// buffer for effective usage of getcomponent
        /// </summary>
        private static List<Transform> m_TransformsBuffer = new List<Transform>(100);

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
        public static bool TryFindInteractionObject(this GameObject unityObject, out InteractionObjectBase result)
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
        public static bool TryFindInteractionObject(this Transform unityTransform, out InteractionObjectBase result) =>
            TryFindInteractionObject(unityTransform.gameObject, out result);

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
        /// Removes interaction ignorers from gameObject if it has them
        /// </summary>
        /// <param name="unityObject"></param>
        public static void UnmakeObjectIgnorer(this GameObject unityObject)
        {
            while (unityObject.TryGetComponent<InteractionIgnorer>(out var ignorer))
                UnityEngine.Object.Destroy(ignorer);
        }

        /// <summary>
        /// Removes interaction ignorers from gameObject if it has them
        /// </summary>
        /// <param name="unityTransform"></param>
        public static void UnmakeObjectIgnorer(this Transform unityTransform) => UnmakeObjectIgnorer(unityTransform.gameObject);

        /// <summary>
        /// Adds Interaction Ignorers to all objects in the tree
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="includeInactive"></param>
        public static void MakeTreeInteractionIgnorer(this GameObject unityObject, bool includeInactive = true)
        {
            unityObject.GetComponentsInChildren(includeInactive, m_TransformsBuffer);
            foreach (var t in m_TransformsBuffer)
                MakeObjectIgnorer(t.gameObject);
        }

        /// <summary>
        /// Adds Interaction Ignorers to all transforms in the tree
        /// </summary>
        /// <param name="unityTransform"></param>
        /// <param name="includeInactive"></param>
        public static void MakeTreeInteractionIgnorer(this Transform unityTransform, bool includeInactive = true) =>
            MakeTreeInteractionIgnorer(unityTransform.gameObject, includeInactive);

        /// <summary>
        /// Removes Interaction Ignorers from all objects in the tree
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="includeInactive"></param>
        public static void UnmakeTreeInteractionIgnorer(this GameObject unityObject, bool includeInactive = true)
        {
            unityObject.GetComponentsInChildren(includeInactive, m_TransformsBuffer);
            foreach (var t in m_TransformsBuffer)
                UnmakeObjectIgnorer(t.gameObject);
        }

        /// <summary>
        /// Removes Interaction Ignorers from all objects in the tree
        /// </summary>
        /// <param name="unityTransform"></param>
        /// <param name="includeInactive"></param>
        public static void UnmakeTreeInteractionIgnorer(this Transform unityTransform, bool includeInactive = true) =>
            UnmakeTreeInteractionIgnorer(unityTransform.gameObject, includeInactive);

        #endregion

        #region MouseSubscriptions

        //The mouse has no events of its own: its buttons are pointers, and these subscribe to the events of the
        //pointers of the mouse with the given button, or with any.

        public static IDisposable SubscribeMouseGrabEvent(
            this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseInteractionEventArgs> grabStartHandler,
            EventHandler<MouseInteractionEventArgs> grabEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return SubscribeMouseGrab(interactionObjectBase, null, grabStartHandler, grabEndHandler, handleEvents, ignoreHandled);
        }

        public static IDisposable SubscribeLeftMouseGrabEvent(
            this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseInteractionEventArgs> grabStartHandler,
            EventHandler<MouseInteractionEventArgs> grabEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return SubscribeMouseGrab(interactionObjectBase, PointerButton.Primary, grabStartHandler, grabEndHandler, handleEvents, ignoreHandled);
        }

        public static IDisposable SubscribeRightMouseGrabEvent(
            this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseInteractionEventArgs> grabStartHandler,
            EventHandler<MouseInteractionEventArgs> grabEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return SubscribeMouseGrab(interactionObjectBase, PointerButton.Secondary, grabStartHandler, grabEndHandler, handleEvents, ignoreHandled);
        }

        public static IDisposable SubscribeMiddleMouseGrabEvent(
            this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseInteractionEventArgs> grabStartHandler,
            EventHandler<MouseInteractionEventArgs> grabEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return SubscribeMouseGrab(interactionObjectBase, PointerButton.Middle, grabStartHandler, grabEndHandler, handleEvents, ignoreHandled);
        }

        public static IDisposable SubscribeMousePressEvent(this InteractionObjectBase interactionObjectBase, EventHandler<MouseInteractionEventArgs> handler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return interactionObjectBase.Subscribe(PointerInteractionEvents.PointerPressEvent, handler, null, PointerKind.Mouse, handleEvents, ignoreHandled);
        }

        public static IDisposable SubscribeLeftMousePressEvent(this InteractionObjectBase interactionObjectBase, EventHandler<MouseInteractionEventArgs> handler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return interactionObjectBase.Subscribe(PointerInteractionEvents.PointerPressEvent, handler, PointerButton.Primary, PointerKind.Mouse, handleEvents,
                ignoreHandled);
        }

        public static IDisposable SubscribeRightMousePressEvent(this InteractionObjectBase interactionObjectBase, EventHandler<MouseInteractionEventArgs> handler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return interactionObjectBase.Subscribe(PointerInteractionEvents.PointerPressEvent, handler, PointerButton.Secondary, PointerKind.Mouse, handleEvents,
                ignoreHandled);
        }

        public static IDisposable SubscribeMiddleMousePressEvent(this InteractionObjectBase interactionObjectBase, EventHandler<MouseInteractionEventArgs> handler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return interactionObjectBase.Subscribe(PointerInteractionEvents.PointerPressEvent, handler, PointerButton.Middle, PointerKind.Mouse, handleEvents,
                ignoreHandled);
        }

        public static IDisposable SubscribeMouseDragEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseDragInteractionEventArgs> mouseDragStartHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragEndHandler
        )
        {
            return SubscribeMouseDrag(interactionObjectBase, null, mouseDragStartHandler, mouseDragHandler, mouseDragEndHandler);
        }

        public static IDisposable SubscribeLeftMouseDragEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseDragInteractionEventArgs> mouseDragStartHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragPerformHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragEndHandler
        )
        {
            return SubscribeMouseDrag(interactionObjectBase, PointerButton.Primary, mouseDragStartHandler, mouseDragPerformHandler, mouseDragEndHandler);
        }

        public static IDisposable SubscribeRightMouseDragEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseDragInteractionEventArgs> mouseDragStartHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragPerformHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragEndHandler
        )
        {
            return SubscribeMouseDrag(interactionObjectBase, PointerButton.Secondary, mouseDragStartHandler, mouseDragPerformHandler, mouseDragEndHandler);
        }

        public static IDisposable SubscribeMiddleMouseDragEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<MouseDragInteractionEventArgs> mouseDragStartHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragPerformHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragEndHandler
        )
        {
            return SubscribeMouseDrag(interactionObjectBase, PointerButton.Middle, mouseDragStartHandler, mouseDragPerformHandler, mouseDragEndHandler);
        }

        private static IDisposable SubscribeMouseGrab(InteractionObjectBase interactionObjectBase, PointerButton? button,
            EventHandler<MouseInteractionEventArgs> grabStartHandler, EventHandler<MouseInteractionEventArgs> grabEndHandler, bool handleEvents,
            bool ignoreHandled)
        {
            var sub1 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerGrabEvent, grabStartHandler, button, PointerKind.Mouse, handleEvents,
                ignoreHandled);
            var sub2 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerReleaseEvent, grabEndHandler, button, PointerKind.Mouse, handleEvents,
                ignoreHandled);
            return new DisposeAction(() =>
            {
                sub1.Dispose();
                sub2.Dispose();
            });
        }

        private static IDisposable SubscribeMouseDrag(InteractionObjectBase interactionObjectBase, PointerButton? button,
            EventHandler<MouseDragInteractionEventArgs> mouseDragStartHandler, EventHandler<MouseDragInteractionEventArgs> mouseDragPerformHandler,
            EventHandler<MouseDragInteractionEventArgs> mouseDragEndHandler)
        {
            var sub1 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerDragStartEvent, mouseDragStartHandler, button, PointerKind.Mouse);
            var sub2 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerDragPerformEvent, mouseDragPerformHandler, button, PointerKind.Mouse);
            var sub3 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerDragEndEvent, mouseDragEndHandler, button, PointerKind.Mouse);
            return new DisposeAction(() =>
            {
                sub1.Dispose();
                sub2.Dispose();
                sub3.Dispose();
            });
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Unified pointer press subscription
        /// </summary>
        /// <param name="interactionObjectBase"></param>
        /// <param name="handler"></param>
        /// <param name="handleEvents"></param>
        /// <param name="ignoreHandled"></param>
        public static IDisposable SubscribePointerPressEvent(this InteractionObjectBase interactionObjectBase, EventHandler<PointerInteractionEventArgs> handler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            return interactionObjectBase.Subscribe(PointerInteractionEvents.PointerPressEvent, handler, handleEvents, ignoreHandled);
        }

        /// <summary>
        /// Unified pointer drag subscription
        /// </summary>
        /// <param name="interactionObjectBase"></param>
        /// <param name="pointerDragStartHandler"></param>
        /// <param name="pointerDragPerformHandler"></param>
        /// <param name="pointerDragEndHandler"></param>
        /// <param name="handleEvents"></param>
        /// <param name="ignoreHandled"></param>
        public static IDisposable SubscribePointerDragEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<PointerDragInteractionEventArgs> pointerDragStartHandler,
            EventHandler<PointerDragInteractionEventArgs> pointerDragPerformHandler,
            EventHandler<PointerDragInteractionEventArgs> pointerDragEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            var sub1 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerDragStartEvent, pointerDragStartHandler, handleEvents, ignoreHandled);
            var sub2 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerDragPerformEvent, pointerDragPerformHandler, handleEvents, ignoreHandled);
            var sub3 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerDragEndEvent, pointerDragEndHandler, handleEvents, ignoreHandled);
            return new DisposeAction(() =>
            {
                sub1.Dispose();
                sub2.Dispose();
                sub3.Dispose();
            });
        }

        /// <summary>
        /// Unified two pointer drag subscription
        /// </summary>
        /// <param name="interactionObjectBase"></param>
        /// <param name="twoPointersDragStartHandler"></param>
        /// <param name="twoPointersDragPerformHandler"></param>
        /// <param name="twoPointersDragEndHandler"></param>
        /// <param name="handleEvents"></param>
        /// <param name="ignoreHandled"></param>
        /// <returns></returns>
        public static IDisposable SubscribeTwoPointersDragEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<TwoPointersDragInteractionEventArgs> twoPointersDragStartHandler,
            EventHandler<TwoPointersDragInteractionEventArgs> twoPointersDragPerformHandler,
            EventHandler<TwoPointersDragInteractionEventArgs> twoPointersDragEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            var sub1 = interactionObjectBase.Subscribe(PointerInteractionEvents.TwoPointersDragStartEvent, twoPointersDragStartHandler, handleEvents, ignoreHandled);
            var sub2 = interactionObjectBase.Subscribe(PointerInteractionEvents.TwoPointersDragPerformEvent, twoPointersDragPerformHandler, handleEvents, ignoreHandled);
            var sub3 = interactionObjectBase.Subscribe(PointerInteractionEvents.TwoPointersDragEndEvent, twoPointersDragEndHandler, handleEvents, ignoreHandled);
            return new DisposeAction(() =>
            {
                sub1.Dispose();
                sub2.Dispose();
                sub3.Dispose();
            });
        }

        /// <summary>
        /// Unified grab/grab end subscription
        /// </summary>
        /// <param name="interactionObjectBase"></param>
        /// <param name="pointerGrabStartHandler"></param>
        /// <param name="pointerGrabEndHandler"></param>
        /// <param name="handleEvents"></param>
        /// <param name="ignoreHandled"></param>
        /// <returns></returns>
        public static IDisposable SubscribePointerGrabEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<PointerInteractionEventArgs> pointerGrabStartHandler,
            EventHandler<PointerInteractionEventArgs> pointerGrabEndHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            var sub1 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerGrabEvent, pointerGrabStartHandler, handleEvents, ignoreHandled);
            var sub2 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerReleaseEvent, pointerGrabEndHandler, handleEvents, ignoreHandled);
            return new DisposeAction(() =>
            {
                sub1.Dispose();
                sub2.Dispose();
            });
        }

        /// <summary>
        /// Pointer hover subscription: the first pointer comes over the object, the last one leaves it
        /// </summary>
        /// <param name="interactionObjectBase"></param>
        /// <param name="pointerEnterHandler"></param>
        /// <param name="pointerExitHandler"></param>
        /// <param name="handleEvents"></param>
        /// <param name="ignoreHandled"></param>
        public static IDisposable SubscribePointerHoverEvent(this InteractionObjectBase interactionObjectBase,
            EventHandler<PointerInteractionEventArgs> pointerEnterHandler,
            EventHandler<PointerInteractionEventArgs> pointerExitHandler,
            bool handleEvents = true, bool ignoreHandled = false)
        {
            var sub1 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerEnterEvent, pointerEnterHandler, handleEvents, ignoreHandled);
            var sub2 = interactionObjectBase.Subscribe(PointerInteractionEvents.PointerExitEvent, pointerExitHandler, handleEvents, ignoreHandled);
            return new DisposeAction(() =>
            {
                sub1.Dispose();
                sub2.Dispose();
            });
        }

        #endregion
    }
}