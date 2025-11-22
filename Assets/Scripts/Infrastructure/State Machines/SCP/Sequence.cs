using System;
using System.Collections.Generic;

namespace Infrastructure.StateMachines.Scp
{
    public class Sequence<TBlackboard> where TBlackboard : class, new()
    {
        // Front
        public TBlackboard Blackboard { get; private set; }
        public event Action Started;
        public event Action<bool> Stopped;

        // Internal
        private readonly List<IClip> _initiators = new();
        private readonly List<PlayCondition> _followers = new();

        private readonly List<IClip> _runnings = new();
        private readonly List<ClipToken> _tokens = new();

        private bool _active = false;
        private bool _succeed;


        // Content
        public void Add(IClip clip)
        {
            ThrowIfActive();
            _initiators.Insert(0, clip);
        }
        public void Add(PlayCondition condition, IClip clip)
        {
            ThrowIfActive();

            condition.Clip = clip;
            _followers.Insert(0, condition);
        }


        // Play
        private void Start()
        {
            if (_active)
                throw new InvalidOperationException(
                    "The sequencs already has been started.");

            Blackboard = new();

            foreach (var clip in _initiators)
            {
                clip.Start(Blackboard, Array.Empty<ClipToken>());
                _runnings.Add(clip);
            }

            _succeed = false;
            _active = true;
            Started?.Invoke();
        }

        public void Update(float deltaTime)
        {
            if (!_active)
                Start();

            if (_runnings.Count == 0)
            {
                _succeed = true;
                Stop();
                return;
            }

            for (int i = _runnings.Count - 1; i >= 0; i--)
            {
                var cur = _runnings[i];
                cur.Update(deltaTime);

                if (cur.TryGetToken(out var token))
                {
                    _runnings.RemoveAt(i);
                    _tokens.Add(token.With(null, true));

                    foreach (var follower in _followers)
                        if (follower.CanPlay(_tokens, out var hits))
                        {
                            _runnings.Insert(0, follower.Clip);
                            follower.Clip.Start(Blackboard, hits);
                        }
                }
            }
        }

        public void Stop()
        {
            if (!_active)
                return;

            _active = false;
            _runnings.Clear();
            _tokens.Clear();

            Stopped?.Invoke(_succeed);
        }

        private void ThrowIfActive()
        {
            if (_active)
                throw new InvalidOperationException(
                    "Cannot modify sequence while active.");
        }
    }

    public class Sequence : Sequence<object> { }
}
