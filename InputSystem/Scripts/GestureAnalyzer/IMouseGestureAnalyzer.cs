using UnityEngine;

namespace UTools.Input
{
    public interface IMouseGestureAnalyzer : IGestureAnalyzer
    {
        public void UpdateMousePosition(Vector2 position);
        public void MouseButtonDown(int button);
        public void MouseButtonUp(int button);
    }
}