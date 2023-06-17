using UnityEngine;

namespace UTools.Input
{
    public interface IGestureAnalyzer
    {
        public bool IsSoloGesture { get; }
        public bool CanBeRemoved { get; }
        public InteractionObjectBase InteractionObjectBase { get; }
        public Camera InteractionCamera { get; }
        public int GestureId { get; }
        public void UpdateAnalyzer();
    }
}