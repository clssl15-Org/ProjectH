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
        public bool Enabled => _enabled;

        // Internal
        private Animation _animation;
        private readonly Dictionary<EnableEventType, Action> _events = new();

        private bool _enabled = true;
        private IDisposable _timer;

        private bool _isDisposed = false;


        // Content
        public EnableWithAnimation(
            Animation animation,
            bool isEnabled = true)
        {
            _animation = animation;
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

            var state = _animation[_animation.clip.name];
            state.speed = 1f;

            if (state.time >= state.length)
                state.time = 0f;

            _events[EnableEventType.Enabling]?.Invoke();

            _animation.Play();
            _timer = new Timer(state.length - state.time, succeed =>
            {
                if (succeed)
                    _events[EnableEventType.Enabled]?.Invoke();
            });
        }

        public void SetToEnabled()
        {
            if (_enabled) return;
            _enabled = true;

            _timer?.Dispose();

            var state = _animation[_animation.clip.name];
            state.time = state.length;

            _events[EnableEventType.Enabling]?.Invoke();
            _events[EnableEventType.Enabled]?.Invoke();
        }

        public void Disable()
        {
            if (_isDisposed)
                return;

            if (!_enabled) return;
            _enabled = false;

            _timer?.Dispose();

            var state = _animation[_animation.clip.name];
            state.speed = -1f;

            if (state.time <= 0f)
                state.time = state.length;

            _events[EnableEventType.Disabling]?.Invoke();

            _animation.Play();
            _timer = new Timer(state.time, succeed =>
            {
                if (succeed)
                    _events[EnableEventType.Disabled]?.Invoke();
            });
        }

        public void SetToDisabled()
        {
            if (!_enabled) return;
            _enabled = false;

            _timer?.Dispose();

            var state = _animation[_animation.clip.name];
            state.time = 0f;

            _events[EnableEventType.Disabling]?.Invoke();
            _events[EnableEventType.Disabled]?.Invoke();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _timer?.Dispose();
            _events.Clear();
        }
    }
}
