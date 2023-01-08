using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    public class InteractionSubscribtion
    {
        public readonly Delegate Handler;
        public readonly bool HandleEvents;
        public readonly bool IgnoreHandled;

        public InteractionSubscribtion(Delegate handler, bool handleEvents = true, bool runIfHandled = false )
        {
            HandleEvents = handleEvents;
            IgnoreHandled = runIfHandled;
            Handler = handler;
        }
    }
}
