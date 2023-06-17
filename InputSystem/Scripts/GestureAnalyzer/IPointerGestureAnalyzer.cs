using System.Collections.Generic;
using UnityEngine;

namespace UTools.Input
{
    public interface IPointerGestureAnalyzer : IGestureAnalyzer
    {
        public int PointersCount { get; }
        IEnumerable<InteractionPointer> Pointers { get; }

        public bool HasPointer(int pointerId);
        public bool TryGetPointer(int pointerId, out InteractionPointer pointer);

        public void CreatePointer(int pointerId, Vector2 position);
        public void RemovePointer(int pointerId, Vector2 position);
    }
}