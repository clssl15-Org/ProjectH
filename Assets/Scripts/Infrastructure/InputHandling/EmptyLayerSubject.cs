using System;

namespace Infrastructure
{
    public class EmptyLayerSubject : IInputLayerSubject, IDisposable
    {
        public event Action Destroying;
        bool IInputLayerSubject.AllowInput { get; set; }

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Destroying?.Invoke();
        }
    }
}
