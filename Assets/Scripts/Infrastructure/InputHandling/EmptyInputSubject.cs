using System;

namespace Infrastructure
{
    public class EmptyInputSubject : IAwakableInputLayerSubject, IDisposable
    {
        public string Name { get; }

        public event Action Destroying;
        public event Action<bool> InputAwakeStateChanged;

        public bool IsTrigger { get; }
        public bool AllowInput { get; set; } = true;

        private bool _isDisposed = false;


        public EmptyInputSubject(string name, bool isTrigger = false)
        {
            Name = name;
            IsTrigger = isTrigger;
        }

        public void SetAwake(bool awake) =>
            InputAwakeStateChanged?.Invoke(awake);

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Destroying?.Invoke();
        }

        public override string ToString() => 
            $"[EmptyInputSubject] {Name}, IsTrigger: {IsTrigger}, AllowInput: {AllowInput}";
    }
}
