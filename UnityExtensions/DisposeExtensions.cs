using System;
using System.Collections.Generic;

namespace UTools
{
    public class DisposableCombiner : IDisposable
    {
        private readonly List<IDisposable> m_Disposable = new();

        public void AddDisposable(IDisposable disposable) => m_Disposable.Add(disposable);

        public void Dispose()
        {
            foreach (var disposable in m_Disposable)
                disposable.Dispose();
            m_Disposable.Clear();
        }
    }

    public static class DisposeExtensions
    {
        /// <summary>
        /// The disposable will be disposed when action will be Invoked
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="action"></param>
        public static void DisposeOn(this IDisposable disposable, ref Action action)
        {
            if (disposable != null)
                action += disposable.Dispose;
        }

        /// <summary>
        /// The disposable will be disposed when IDisposedNotifier is disposed
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="disposeNotifier"></param>
        /// <returns></returns>
        public static IDisposable DisposeOnDisposed(this IDisposable disposable, IDisposedNotifier disposeNotifier)
        {
            if (disposable != null)
                disposeNotifier.Disposed += disposable.Dispose;

            if (disposable is IDisposedNotifier disposableLifeEndNotifier)
                disposableLifeEndNotifier.Disposed += () => disposeNotifier.Disposed -= disposable.Dispose;

            return disposable;
        }

        /// <summary>
        /// Add IDisposable to DisposableCombiner
        /// </summary>
        /// <param name="disposable">Original IDisposable object to be added</param>
        /// <param name="combiner">DisposableCombiner where Original IDisposable object will be added</param>
        /// <returns>Original IDisposable object</returns>
        public static IDisposable CombineTo(this IDisposable disposable, DisposableCombiner combiner)
        {
            combiner.AddDisposable(disposable);
            return disposable;
        }

        /// <summary>
        /// Subscribe to LifeEnd action
        /// </summary>
        /// <param name="disposeNotifier"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IDisposable AddActionToDisposeEvent(this IDisposedNotifier disposeNotifier, Action handler)
        {
            disposeNotifier.Disposed += handler;
            return new DisposeAction(() => disposeNotifier.Disposed -= handler);
        }
    }
}