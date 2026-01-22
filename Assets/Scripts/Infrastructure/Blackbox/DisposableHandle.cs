using System;

namespace BlackboxSystem
{
    public struct DisposableHandle : IDisposable
    {
        private Blackbox _blackbox;
        private string _scopeMessage;
        private bool _isDisposed;

        internal DisposableHandle(Blackbox blackbox, string message)
        {
            _blackbox = blackbox;
            _scopeMessage = message;
            _isDisposed = false;
        }

        public void Dispose()
        {
            if (_blackbox == null || _isDisposed) return;
            _isDisposed = true;

            _blackbox.CloseScope(_scopeMessage);
        }
    }
}
