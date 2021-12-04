using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public static class DisposeExtensions
    {
        /// <summary>
        /// The disposable will be disposed when action will be Invoked
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="action"></param>
        public static void DisposeWhen(this IDisposable disposable, ref Action action)
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
        public static IDisposable DisposeWhenNotifierDisposed(this IDisposable disposable, IDisposedNotifier disposeNotifier)
        {
            if (disposable == null)
                return new DisposableAction(() => { });

            //Защита от двойного вызова. При первом вызове делегат отпускается и ссылка на объект "disposable" больше не держится
            //This is protection from second call. When first time called the delegate will be freed and the link to disposable  will not be kept anymore
            var disposeAction = new DisposableAction(disposable.Dispose);
            disposeNotifier.DisposeEvent += disposeAction.Dispose;
            return new DisposableAction(() => { disposeAction.Dispose(); disposeNotifier.DisposeEvent -= disposeAction.Dispose; });
        }

        /// <summary>
        /// The IDisposableObject will be disposed when IDisposedNotifier is disposed
        /// </summary>
        /// <param name="disposableObject"></param>
        /// <param name="disposeNotifier"></param>
        /// <returns></returns>
        public static IDisposable DisposeIDisposableWhenNotifierDisposed(this IDisposableObject disposableObject, IDisposedNotifier disposeNotifier)
        {
            if (disposableObject == null)
                return new DisposableAction(() => { });

            Action unsubscriber = () => { disposeNotifier.DisposeEvent -= disposableObject.Dispose; };
            disposableObject.DisposeEvent += unsubscriber;

            disposeNotifier.DisposeEvent += disposableObject.Dispose;
            return new DisposableAction(unsubscriber);
        }
    }
}