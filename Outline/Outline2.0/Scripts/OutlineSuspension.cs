using System;

namespace UTools.Outline
{
    /// <summary>
    /// Holds the outline pass off for a render that must not carry it - a picture of the scene taken with
    /// Camera.Render while something is selected. Nothing else changes: the selection stays as it is. Kept apart
    /// from the renderer feature so that a caller needs no reference to the render pipeline assembly.
    /// </summary>
    public static class OutlineSuspension
    {
        private static int s_Suspended;

        public static bool IsSuspended => s_Suspended > 0;

        /// <summary>The outline is not drawn until the result is disposed; nests.</summary>
        public static IDisposable Suspend()
        {
            s_Suspended++;
            return new DisposeAction(() => s_Suspended--);
        }
    }
}
