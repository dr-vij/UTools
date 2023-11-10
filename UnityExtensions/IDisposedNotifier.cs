using System;


namespace UTools
{
    public interface IDisposedNotifier
    {
        public event Action Disposed;
    }
}