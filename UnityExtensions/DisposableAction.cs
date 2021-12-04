using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ViJTools
{
    public class DisposableAction : DisposableObject
    {
        private Action mAction;

        public DisposableAction(Action action)
        {
            mAction = action;
        }

        protected override void OnDispose()
        {
            Interlocked.Exchange(ref mAction, null)?.Invoke();
        }
    }
}