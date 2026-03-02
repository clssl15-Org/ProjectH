using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    public enum EnableEventType
    {
        Enabling,
        Enabled, 
        Disabling,
        Disabled,
    };

    public class EnableWithAnimation : IDisposable
    {
        // Front
        public bool IsEnabled => _enabled;

        // Internal
        private AnimationPlayer _player;
        private readonly Dictionary<EnableEventType, Action> _events = new();

        private bool _useAbsoluteTime;
        private bool _enabled;
        private IDisposable _timer;

        private bool _isDisposed = false;


        // Content
        public EnableWithAnimation(Animation animation, bool isEnabled, bool useAbsoluteTime = true)
        {
            _player = new(animation, useAbsoluteTime);
            _enabled = isEnabled;

            foreach (EnableEventType eventType in Enum.GetValues(typeof(EnableEventType)))
                _events[eventType] = null;
        }

        public EnableWithAnimation SetAction(EnableEventType eventType, Action action)
        {
            _events[eventType] += action;
            return this;
        }
        public EnableWithAnimation RemoveAction(EnableEventType eventType, Action action)
        {
            _events[eventType] -= action;
            return this;
        }

        public void Enable()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_enabled) return;
            _enabled = true;

            _timer?.Dispose();

            _player.Speed = 1f;
            if (_player.Time >= _player.Length) _player.Time = 0f;

            _events[EnableEventType.Enabling]?.Invoke();

            _player.Play();
            _timer = new Timer(_player.Length - _player.Time, succeed =>
            {
                if (succeed) _events[EnableEventType.Enabled]?.Invoke();
            });
        }

        public void Disable()
        {
            if (_isDisposed)
                return;

            if (!_enabled) return;
            _enabled = false;

            _timer?.Dispose();
            _events[EnableEventType.Disabling]?.Invoke();

            _player.Speed = -1f;
            if (_player.Time <= 0f) _player.Time = _player.Length;

            _player.Play();
            _timer = new Timer(_player.Time, succeed =>
            {
                if (succeed) _events[EnableEventType.Disabled]?.Invoke();
            });
        }

        public void SetToEnabled()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_enabled) return;
            _enabled = true;

            _timer?.Dispose();
            _player.Time = _player.Length;

            _events[EnableEventType.Enabling]?.Invoke();
            _events[EnableEventType.Enabled]?.Invoke();
        }

        public void SetToDisabled()
        {
            if (!_enabled) return;
            _enabled = false;

            _timer?.Dispose();
            _player.Time = 0f;

            _events[EnableEventType.Disabling]?.Invoke();
            _events[EnableEventType.Disabled]?.Invoke();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _timer?.Dispose();
            _player.Dispose();
            _events.Clear();
        }
    }
}
