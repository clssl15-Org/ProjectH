using System;
using System.Collections.Generic;

namespace Infrastructure.StateMachines.Scp
{
    public abstract class Clip<TBlackboard> : IClip where TBlackboard : class
    {
        public TBlackboard Blackboard { get; private set; }
        private ClipToken _token;

        public void Start(object blackborad, IEnumerable<ClipToken> befores)
        {
            Blackboard = blackborad as TBlackboard ?? throw new ArgumentException($"{nameof(blackborad)} must be type of {typeof(TBlackboard).Name}");
            OnStart(befores);
        }
        protected virtual void OnStart(IEnumerable<ClipToken> befores) { }

        public void Update(float deltaTime) => OnUpdate(deltaTime);
        protected virtual void OnUpdate(float deltaTime) { }

        public void Stop(object payload = null)
        {
            _token = new(this, payload);
            OnStop();
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

    public abstract class Clip : Clip<object> { }
}
