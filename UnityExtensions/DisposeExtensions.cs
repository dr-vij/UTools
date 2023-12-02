using System;
using System.Collections.Generic;

namespace UTools
{
    /// <summary>
    /// Class that combines multiple IDisposable objects into one, allowing them to be disposed together.
    /// </summary>
    public class DisposableCombiner : IDisposable
    {
        /// <summary>
        /// List of IDisposable objects to be disposed.
        /// </summary>
        private readonly List<IDisposable> m_Disposable = new();

        /// <summary>
        /// Constructor that accepts multiple IDisposable objects.
        /// </summary>
        /// <param name="disposables">Array of IDisposable objects to be added to the list.</param>
        public DisposableCombiner(params IDisposable[] disposables) => m_Disposable.AddRange(disposables);

        /// <summary>
        /// Constructor that accepts a single IDisposable object.
        /// </summary>
        /// <param name="disposable">IDisposable object to be added to the list.</param>
        public DisposableCombiner(IDisposable disposable) => m_Disposable.Add(disposable);

        /// <summary>
        /// Method to add multiple IDisposable objects to the list.
        /// </summary>
        /// <param name="disposables">Array of IDisposable objects to be added to the list.</param>
        public void AddDisposable(params IDisposable[] disposables) => m_Disposable.AddRange(disposables);

        /// <summary>
        /// Method to add a single IDisposable object to the list.
        /// </summary>
        /// <param name="disposable">IDisposable object to be added to the list.</param>
        public void AddDisposable(IDisposable disposable) => m_Disposable.Add(disposable);

        /// <summary>
        /// Method to dispose all IDisposable objects in the list and clear the list.
        /// </summary>
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