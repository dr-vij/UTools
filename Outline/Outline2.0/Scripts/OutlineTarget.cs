using System;

namespace UTools.Outline
{
    /// <summary>
    /// What the outline is drawn for. The outline runs its full-screen passes only while some target is outlined, so a
    /// frame with nothing selected costs nothing. An object that puts renderers on an outline layer keeps a target
    /// marked outlined for as long as they stay there and disposes it when it goes; a renderer on an outline layer with
    /// no outlined target is not outlined. Kept apart from the renderer feature, like OutlineSuspension, so that a
    /// caller needs no reference to the render pipeline assembly.
    /// </summary>
    public sealed class OutlineTarget : IDisposable
    {
        private static int s_Outlined;

        private bool m_IsOutlined;

        /// <summary>Whether any target is outlined now.</summary>
        public static bool Any => s_Outlined > 0;

        public bool IsOutlined
        {
            get => m_IsOutlined;
            set
            {
                if (m_IsOutlined == value)
                    return;
                m_IsOutlined = value;
                s_Outlined += value ? 1 : -1;
            }
        }

        public void Dispose() => IsOutlined = false;
    }
}
