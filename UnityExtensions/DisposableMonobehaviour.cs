using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools
{
    public class DisposableMonoBehaviour : MonoBehaviour, IDisposableObject
    {
        /// <summary>
        /// Variable can be used if you want to check if object hase been disdosed or destroyed
        /// </summary>
        public bool IsDisposed { get; private set; }

        public event Action Disposed;

        /// <summary>
        ///  This is special workaround for Unity, cause object can be created, then destroyed, then activated and Awake will be called :)
        ///  So I block user from using Awake and OnDestroy.
        ///  Use OnDispose and OnAwake instead
        /// </summary>
        public void Awake()
        {
            if (!IsDisposed)
                OnAwake();
        }

        /// <summary>
        /// This method can be called to Destroying and Disposing object.
        /// The object also will be deleted if you Dispose it. And it will be disposed if you Destroy it.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                OnDispose();
                Disposed?.Invoke();
                Disposed = null;

                if (this != null)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                        Destroy(gameObject);
                    else
                        DestroyImmediate(gameObject);
#else
                    Destroy(gameObject);
#endif
                }
            }
        }

        /// <summary>
        /// Use this method for object initialization
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        /// <summary>
        /// Use this method for object deinitialization
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        private void OnDestroy() => Dispose();
    }
}