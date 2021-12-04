using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJTools
{
    public class InteractionSubscribtion
    {
        public readonly Delegate Handler;

        public InteractionSubscribtion(Delegate handler) => Handler = handler;
    }
}
