using System;

namespace BlackboxSystem
{
    public struct DisposableHandle : IDisposable
    {
        private Blackbox _blackbox;
        private bool _isDisposed;

        internal DisposableHandle(Blackbox blackbox)
        {
            _blackbox = blackbox;
            _isDisposed = false;
        }

        public void Dispose()
        {
            if (_blackbox == null || _isDisposed) return;
            _isDisposed = true;

            _blackbox.PopScope();
        }
    }
}
