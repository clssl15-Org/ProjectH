using System.Collections.Generic;

namespace Infrastructure.StateMachines.Scp
{
    public class Delay<TBlackboard> : Clip<TBlackboard> where TBlackboard : class
    {
        private float _delay;
        private float _remainingTime;

        public Delay(float delay) : this("Delay", delay) { }
        public Delay(string name, float delay) : base(name)
        {
            _delay = delay;
        }

        protected override void OnStart(IEnumerable<ClipToken> _)
        {
            _remainingTime = _delay;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _remainingTime -= deltaTime;

            if (_remainingTime <= 0)
                Stop();
        }
    }

    public class Delay : Delay<object>
    {
        public Delay(float delay) : base(delay) { }
        public Delay(string name, float delay) : base(name, delay) { }
    }
}
