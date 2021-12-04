using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ViJTools
{
    public static class UnityEventExtensions
    {
        public static IDisposable Subscribe(this UnityEvent unityEvent, UnityAction call)
        {
            unityEvent.AddListener(call);
            return new DisposableAction(() => unityEvent?.RemoveListener(call));
        }

        public static IDisposable Subscribe<T0>(this UnityEvent<T0> unityEvent, UnityAction<T0> call)
        {
            unityEvent.AddListener(call);
            return new DisposableAction(() => unityEvent?.RemoveListener(call));
        }

        public static IDisposable Subscribe<T0, T1>(this UnityEvent<T0, T1> unityEvent, UnityAction<T0, T1> call)
        {
            unityEvent.AddListener(call);
            return new DisposableAction(() => unityEvent?.RemoveListener(call));
        }

        public static IDisposable Subscribe<T0, T1, T2>(this UnityEvent<T0, T1, T2> unityEvent, UnityAction<T0, T1, T2> call)
        {
            unityEvent.AddListener(call);
            return new DisposableAction(() => unityEvent?.RemoveListener(call));
        }

        public static IDisposable Subscribe<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> unityEvent, UnityAction<T0, T1, T2, T3> call)
        {
            unityEvent.AddListener(call);
            return new DisposableAction(() => unityEvent?.RemoveListener(call));
        }
    }
}