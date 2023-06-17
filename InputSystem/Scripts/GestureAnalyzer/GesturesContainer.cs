using System.Collections.Generic;
using System.Linq;

namespace UTools.Input
{
    public class GesturesContainer
    {
        private readonly Dictionary<InteractionObjectBase, IGestureAnalyzer> m_AllGestures = new();
        private readonly Dictionary<InteractionObjectBase, IPointerGestureAnalyzer> m_PointerGestureAnalyzers = new();
        private readonly Dictionary<InteractionObjectBase, IMouseGestureAnalyzer> m_MouseGestureAnalyzers = new();

        public IEnumerable<IGestureAnalyzer> AllGestures => m_AllGestures.Values;

        public IEnumerable<IPointerGestureAnalyzer> PointerGestureAnalyzers => m_PointerGestureAnalyzers.Values;

        public IEnumerable<IMouseGestureAnalyzer> MouseGestureAnalyzers => m_MouseGestureAnalyzers.Values;

        public bool HasGestures => m_AllGestures.Count > 0;

        public bool HasGestureAnalyzer(InteractionObjectBase interactionObjectBase) => m_AllGestures.ContainsKey(interactionObjectBase);

        public bool TryGetGestureAnalyzer(InteractionObjectBase interactionObjectBase, out IGestureAnalyzer gestureAnalyzer) =>
            m_AllGestures.TryGetValue(interactionObjectBase, out gestureAnalyzer);

        public bool TryGetPointerGestureAnalyzer(InteractionObjectBase interactionObjectBase, out IPointerGestureAnalyzer pointerGestureAnalyzer) =>
            m_PointerGestureAnalyzers.TryGetValue(interactionObjectBase, out pointerGestureAnalyzer);

        public void AddGesture(IGestureAnalyzer gestureAnalyzer)
        {
            if (gestureAnalyzer is IPointerGestureAnalyzer pointerGestureAnalyzer)
                m_PointerGestureAnalyzers.Add(gestureAnalyzer.InteractionObjectBase, pointerGestureAnalyzer);
            if (gestureAnalyzer is IMouseGestureAnalyzer mouseGestureAnalyzer)
                m_MouseGestureAnalyzers.Add(gestureAnalyzer.InteractionObjectBase, mouseGestureAnalyzer);

            m_AllGestures.Add(gestureAnalyzer.InteractionObjectBase, gestureAnalyzer);
        }

        public void RemoveGesture(IGestureAnalyzer gestureAnalyzer)
        {
            if (gestureAnalyzer is IPointerGestureAnalyzer)
                m_PointerGestureAnalyzers.Remove(gestureAnalyzer.InteractionObjectBase);
            if (gestureAnalyzer is IMouseGestureAnalyzer)
                m_MouseGestureAnalyzers.Remove(gestureAnalyzer.InteractionObjectBase);
            m_AllGestures.Remove(gestureAnalyzer.InteractionObjectBase);
        }

        public void RemoveUnusedGestures()
        {
            var unusedGestures = m_PointerGestureAnalyzers.Where(pair => pair.Value.CanBeRemoved).ToList();
            foreach (var unusedGesture in unusedGestures)
                RemoveGesture(unusedGesture.Value);
        }
    }
}