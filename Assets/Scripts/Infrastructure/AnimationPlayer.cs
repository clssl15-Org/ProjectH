using System;
using UnityEngine;

namespace Infrastructure
{
    public class AnimationPlayer : IDisposable
    {
        // Front
        public bool UseAbsoluteTime { get; set; } = false;

        public float Speed { get; set; } = 1f;
        public float Time
        {
            get => AnimState.time;
            set
            {
                var wasDisabled = AnimState.enabled;
                AnimState.enabled = true;

                AnimState.time = Mathf.Clamp(value, 0, Length);
                _animation.Sample();

                if (wasDisabled) AnimState.enabled = false;
            }
        }
        public float Length => AnimState.length;

        // Internal
        private Animation _animation;
        private IDisposable _updater;

        private AnimationState AnimState => _animation[_animation.clip.name];


        // Content
        public AnimationPlayer(Animation animation, bool useAbsoluteTime = false)
        {
            _animation = animation;
            _animation.playAutomatically = false;

            UseAbsoluteTime = useAbsoluteTime;
            AnimState.weight = 1f;
        }

        public void Play()
        {
            Stop();

            if (!_animation) return;
            AnimState.enabled = true;

            _updater = Loco.Subscribe(Update);
            Update();
        }

        public void Stop()
        {
            if (_animation)
                AnimState.enabled = false;

            _updater?.Dispose();
            _updater = null;
        }

        private void Update()
        {
            if (!_animation)
            {
                Stop();
                return;
            }

            Time += Speed * (UseAbsoluteTime
                ? UnityEngine.Time.unscaledDeltaTime
                : UnityEngine.Time.deltaTime);

            if ((Speed > 0 && Time >= Length) || (Speed < 0 && Time <= 0))
                Stop();
        }

        public void Dispose() => _updater?.Dispose();
    }
}
