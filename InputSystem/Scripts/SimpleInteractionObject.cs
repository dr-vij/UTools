using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    public class SimpleInteractionObject : InteractionObjectBase
    {
        public override IGestureAnalyzer CreateAnalyzer(Camera cam) => new SimpleGestureAnalyzer(this, cam, true);
    }
}