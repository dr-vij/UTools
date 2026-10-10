using System;

namespace UTools.Input
{
    /// <summary>
    /// One handler of an interaction event on an interaction object, and how it takes part in handling the event.
    /// </summary>
    public abstract class InteractionSubscription
    {
        public readonly bool HandleEvents;
        public readonly bool IgnoreHandled;

        //The pointers the handler is for; null is any.
        private readonly PointerButton? m_Button;
        private readonly PointerKind? m_Kind;

        protected InteractionSubscription(bool handleEvents, bool ignoreHandled, PointerButton? button, PointerKind? kind)
        {
            HandleEvents = handleEvents;
            IgnoreHandled = ignoreHandled;
            m_Button = button;
            m_Kind = kind;
        }

        /// <summary>
        /// Whether the handler is for these args: they are of its type and come from a pointer of its button and kind.
        /// Args without a pointer, of an event run by hand, pass by their type alone.
        /// </summary>
        public bool Accepts(InteractionEventArgs args)
        {
            if (!IsOfHandlerType(args))
                return false;
            if (args is not PointerInteractionEventArgs { Pointer: { } pointer })
                return true;

            return (m_Button == null || m_Button == pointer.Button) && (m_Kind == null || m_Kind == pointer.Kind);
        }

        public abstract void Invoke(object sender, InteractionEventArgs args);

        protected abstract bool IsOfHandlerType(InteractionEventArgs args);
    }

    public sealed class InteractionSubscription<TEventArgs> : InteractionSubscription where TEventArgs : InteractionEventArgs
    {
        private readonly EventHandler<TEventArgs> m_Handler;

        public InteractionSubscription(EventHandler<TEventArgs> handler, bool handleEvents, bool ignoreHandled, PointerButton? button, PointerKind? kind) :
            base(handleEvents, ignoreHandled, button, kind)
        {
            m_Handler = handler;
        }

        public override void Invoke(object sender, InteractionEventArgs args) => m_Handler(sender, (TEventArgs)args);

        protected override bool IsOfHandlerType(InteractionEventArgs args) => args is TEventArgs;
    }
}
