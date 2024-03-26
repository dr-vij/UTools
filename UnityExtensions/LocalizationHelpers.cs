using System;
using UnityEngine.Localization;

namespace UTools
{
    public static class LocalizationHelpers
    {
        public static IDisposable Subscribe(this LocalizedString localizedString, LocalizedString.ChangeHandler handler)
        {
            localizedString.StringChanged += handler;
            return new DisposeAction(() => localizedString.StringChanged -= handler);
        }
    }
}