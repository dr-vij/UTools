using System;

namespace UTools
{
    public class LocalizationHelpers
    {
        public IDisposable SubscribeLocalizedString(LocalizedString localizedString, LocalizedString.ChangeHandler handler)
        {
            localizedString.StringChanged += handler;
            return new DisposeAction(() => localizedString.StringChanged -= handler);
        }
    }
}