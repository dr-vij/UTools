using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace UTools.Input
{
    /// <summary>
    /// Runs a native modal window, such as a system file dialog, that blocks the player loop while it is open.
    /// </summary>
    /// <remarks>
    /// The window takes the pointer and the keyboard away from the application, but on some platforms (macOS, the
    /// editor included) the application keeps its focus. The Input System then queues everything the user does in
    /// the window and delivers it, with its old time stamps, in the first update after the window closes: a click on
    /// the window's "Open" button arrives as a click on whatever lies under that spot in the scene.
    ///
    /// On a real focus loss the Input System handles this itself: it disables the pointer and the keyboard while the
    /// application is in the background and syncs them when it comes back (<see cref="InputSettings.backgroundBehavior"/>).
    /// This does the same around the window. Pointers and keyboards are disabled before it opens, in the runtime as
    /// well as in the Input System: the events of such a device are dropped on arrival, before they change any state
    /// or reach any listener (a device disabled in the Input System only, with keepSendingEvents, still has its events
    /// processed). They stay disabled through the first input update after the window, the one that flushes the
    /// queue, and are enabled after it: the Input System syncs each device or resets it to "all released"; the
    /// pointer keeps its position.
    ///
    /// Only for windows that block the calling thread: an asynchronous dialog (browser, mobile) does not stall the
    /// player loop, and the application gets its focus events there.
    /// </remarks>
    public static class NativeModal
    {
        private const InputUpdateType PlayerUpdates = InputUpdateType.Dynamic | InputUpdateType.Fixed | InputUpdateType.Manual;

        private static readonly List<InputDevice> s_Disabled = new();
        private static int s_OpenWindows;

        public static T Run<T>(Func<T> window)
        {
            s_OpenWindows++;
            DisableInput();
            try
            {
                return window();
            }
            finally
            {
                s_OpenWindows--;
                InputSystem.onAfterUpdate -= OnAfterUpdate;
                InputSystem.onAfterUpdate += OnAfterUpdate;
            }
        }

        private static void DisableInput()
        {
            foreach (var device in InputSystem.devices)
            {
                if (!device.enabled || device is not (Pointer or Keyboard))
                    continue;

                InputSystem.DisableDevice(device);
                s_Disabled.Add(device);
            }
        }

        private static void OnAfterUpdate()
        {
            // In play mode the editor runs its own input updates too; the queued pointer and keyboard events go to the player's.
            var update = InputState.currentUpdateType;
            var flushed = (update & PlayerUpdates) != 0 || (update == InputUpdateType.Editor && !Application.isPlaying);
            if (s_OpenWindows > 0 || !flushed)
                return;

            InputSystem.onAfterUpdate -= OnAfterUpdate;
            foreach (var device in s_Disabled)
            {
                if (device.added)
                    InputSystem.EnableDevice(device);
            }

            s_Disabled.Clear();
        }
    }
}
