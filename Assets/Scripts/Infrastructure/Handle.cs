using System;

namespace UniEngine
{
    public class Handle : IDisposable
    {
        // Front
        public bool IsDisposed { get; private set; } = false;

        // Internal
        private Action _disposed;

        // Content
        public Handle(Action action) => _disposed = action;

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            _disposed?.Invoke();
            _disposed = null;
        }
    }
}
