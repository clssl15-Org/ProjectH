using System;
using UnityEngine;

namespace UniEngine
{
    public class Timer : IDisposable
    {
        // Front
        public float RemainingTime => _remainingTime;
        public bool IsRunning => !_isDisposed;
        public float Factor { get; set; } = 1f;

        // Internal
        float _remainingTime;
        Action<bool> _callback;
        IDisposable _handle;

        private bool _isDisposed = false;


        // Content
        public Timer(float time, Action<bool> callback = null)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(time), time, $"{nameof(time)}은(는) 0 이상이어야 합니다.");

            _remainingTime = time;
            _callback = callback;
            _handle = Loco.Subscribe(Update);
        }

        private void Update()
        {
            _remainingTime -= Time.deltaTime * Factor;
            if (_remainingTime > 0f) return;

            _remainingTime = 0f;

            CallbackAndClear(true);
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            CallbackAndClear(false);

            _handle?.Dispose();
            _handle = null;
        }

        private void CallbackAndClear(bool succeed)
        {
            if (_callback == null)
                return;

            var callback = _callback;
            _callback = null;

            callback.Invoke(succeed);
        }
    }
}