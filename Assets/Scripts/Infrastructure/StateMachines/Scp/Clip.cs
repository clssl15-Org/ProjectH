using System;
using System.Collections.Generic;

namespace Infrastructure.StateMachines.Scp
{
    public class Clip<TBlackboard> : IClip where TBlackboard : class
    {
        public string Name { get; init; } = "Clip";
        public bool IsPlaying { get; private set; } = false;

        public event Action<Clip<TBlackboard>, IEnumerable<ClipToken>> Started;
        public event Action<Clip<TBlackboard>, float> Updated;
        public event Action<Clip<TBlackboard>> Stopped;

        public TBlackboard Blackboard { get; private set; }
        private ClipToken _token;

        public Clip() { }
        public Clip(string name) => Name = name;

        public Clip<TBlackboard> OnStarted(Action<Clip<TBlackboard>, IEnumerable<ClipToken>> started)
        {
            Started += started;
            return this;
        }
        public Clip<TBlackboard> OnUpdated(Action<Clip<TBlackboard>, float> updated)
        {
            Updated += updated;
            return this;
        }
        public Clip<TBlackboard> OnStopped(Action<Clip<TBlackboard>> stopped)
        {
            Stopped += stopped;
            return this;
        }
        public Clip<TBlackboard> AssignTo(out Clip<TBlackboard> self)
        {
            self = this;
            return this;
        }


        public void Start(object blackborad, IEnumerable<ClipToken> befores)
        {
            Blackboard = blackborad as TBlackboard ?? throw new ArgumentException(
                $"{nameof(blackborad)} must be type of {typeof(TBlackboard).Name}");

            IsPlaying = true;
            OnStart(befores);
            Started?.Invoke(this, befores);
        }
        protected virtual void OnStart(IEnumerable<ClipToken> befores) { }

        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
            Updated?.Invoke(this, deltaTime);
        }
        protected virtual void OnUpdate(float deltaTime) { }

        public void Stop(object payload = null)
        {
            IsPlaying = false;
            _token = new(this, payload);

            OnStop();
            Stopped?.Invoke(this);
        }
        protected virtual void OnStop() { }

        public bool TryGetToken(out ClipToken token)
        {
            if (_token == null)
            {
                token = null;
                return false;
            }

            token = _token;
            _token = null;
            return true;
        }
    }

    public class Clip : Clip<object>
    {
        public Clip() { }
        public Clip(string name) => Name = name;
    }
}
