using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public class DisposableObject : IDisposableObject
    {
        public bool IsDisposed { get; private set; }

        public event Action DisposeEvent;

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                OnDispose();
                DisposeEvent?.Invoke();
                DisposeEvent = null;
            }
        }

        protected virtual void OnDispose()
        {
        }
    }
}