using System;
using System.Collections.Generic;
using UnityEngine;

namespace UTools.Rendering
{
    /// <summary>
    /// A set of renderers marked with a rendering layer while the overlay is on: RenderingLayerOverlayFeature draws the
    /// renderers of that layer once more over the scene with its own material - a highlight that touches no material of
    /// theirs. The feature draws only while some overlay is on. Overlays may share renderers (a device's body and a lever
    /// on it): a renderer keeps the layer while any overlay of that layer holding it is on. Kept apart from the feature,
    /// like OutlineTarget, so that a caller needs no reference to the render pipeline assembly.
    /// </summary>
    public sealed class RenderingLayerOverlay : IDisposable
    {
        private static int s_On;
        // How many overlays that are on hold each renderer with each layer.
        private static readonly Dictionary<(Renderer, uint), int> s_Holders = new();

        private readonly IReadOnlyList<Renderer> m_Renderers;
        private readonly uint m_Mask;
        private bool m_IsOn;

        /// <summary>Whether any overlay is on now.</summary>
        public static bool Any => s_On > 0;

        /// <param name="renderingLayer">The name of a rendering layer of the project (Tags and Layers).</param>
        /// <param name="renderers">The renderers the overlay marks; read each time it goes on or off.</param>
        public RenderingLayerOverlay(string renderingLayer, IReadOnlyList<Renderer> renderers)
        {
            m_Renderers = renderers;
            m_Mask = RenderingLayerMask.GetMask(renderingLayer);
            if (m_Mask == 0)
                Debug.LogError($"No rendering layer '{renderingLayer}' in the project: the overlay marks nothing.");
        }

        public bool IsOn
        {
            get => m_IsOn;
            set
            {
                if (m_IsOn == value)
                    return;
                m_IsOn = value;
                s_On += value ? 1 : -1;
                foreach (var renderer in m_Renderers)
                    if (renderer != null)
                        Hold(renderer, value);
            }
        }

        private void Hold(Renderer renderer, bool on)
        {
            var key = (renderer, m_Mask);
            s_Holders.TryGetValue(key, out var holders);
            holders += on ? 1 : -1;
            if (holders > 0)
                s_Holders[key] = holders;
            else
                s_Holders.Remove(key);
            renderer.renderingLayerMask = holders > 0 ? renderer.renderingLayerMask | m_Mask : renderer.renderingLayerMask & ~m_Mask;
        }

        public void Dispose() => IsOn = false;
    }
}
