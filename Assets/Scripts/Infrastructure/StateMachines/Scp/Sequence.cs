using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

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
        private bool _succeeded;


        // Content
        public Sequence() { }
        public Sequence(Action started = null, Action<bool> stopped = null)
        {
            Started += started;
            Stopped += stopped;
        }

        public Sequence<TBlackboard> Add(IClip clip)
        {
            ThrowIfActive();
            _initiators.Add(clip);

            return this;
        }

        public Sequence<TBlackboard> Add(CTC condition, IClip clip)
        {
            if (condition == null)
                Add(clip);
            else
                Add(new PlayCondition(condition), clip);

            return this;
        }
        public Sequence<TBlackboard> Add(PlayCondition condition, IClip clip)
        {
            if (condition == null)
            {
                Add(clip);
                return this;
            }

            ThrowIfActive();

            condition.Clip = clip;
            _followers.Add(condition);

            return this;
        }

        public Sequence<TBlackboard> AddAfter(IClip before, IClip clip)
        {
            if (before == null)
                Add(clip);
            else
                Add(new CTC(before), clip);

            return this;
        }


        // Play
        public bool Start()
        {
            if (_active)
            {
                UnityEngine.Debug.LogWarning(
                    "This sequence has already been started.");

                return false;
            }

            Blackboard = new();

            _succeeded = false;
            _active = true;

            try
            {
                foreach (var clip in _initiators)
                {
                    clip.Start(Blackboard, Array.Empty<ClipToken>());
                    _runnings.Insert(0, clip);
                }
            }
#pragma warning disable CS0168 // 비 UNIENGINE 환경 시 변수 미사용 경고
            catch (Exception ex)
#pragma warning restore
            {
                Stop();
#if UNIENGINE
                ExceptionHandler.Report(ex);
                return false;
#else
                throw;
#endif
            }

#if UNIENGINE
            Started?.SafeInvoke();
#else
            Started?.Invoke();
#endif

            return true;
        }

        public void Update(float deltaTime) => Update(deltaTime, out _);
        public bool Update(float deltaTime, out bool succeeded)
        {
            if (!_active && !Start())
            {
                succeeded = false;
                return false;
            }

            if (_runnings.Count == 0)
            {
                _succeeded = succeeded = true;

                Stop();
                return false;
            }

            List<IClip> buffer = null;

            try
            {
                for (int i = _runnings.Count - 1; i >= 0; i--)
                {
                    var cur = _runnings[i];
                    cur.Update(deltaTime);

                    if (cur.TryGetToken(out var token))
                    {
                        _runnings.RemoveAt(i);
                        _tokens.Add(token.With(null, true));

                        foreach (var follower in _followers)
                        {
                            if (follower.Clip.IsPlaying)
                                continue;
                            if (buffer?.Contains(follower.Clip) ?? false)
                                continue;

                            if (follower.CanPlay(token, _tokens, out var hits))
                            {
                                follower.Clip.Start(Blackboard, hits);

                                buffer ??= new();
                                buffer.Insert(0, follower.Clip);
                            }
                        }
                    }
                }

                if (buffer != null)
                    _runnings.InsertRange(0, buffer);

                succeeded = false;
                return true;
            }
#pragma warning disable CS0168 // 비 UNIENGINE 환경 시 변수 미사용 경고
            catch (Exception ex)
#pragma warning restore
            {
                buffer?.ForEach(c => c.Stop());
                Stop();
#if UNIENGINE
                ExceptionHandler.Report(ex);
                return false;
#else
                throw;
#endif
            }
        }

        public void Stop()
        {
            if (!_active) return;
            _active = false;

            _runnings.ForEach(c => c.Stop());
            _runnings.Clear();
            _tokens.Clear();

#if UNIENGINE
            Stopped?.SafeInvoke(_succeeded);
#else
            Stopped?.Invoke(_succeeded);
#endif
        }

        private void ThrowIfActive()
        {
            if (_active)
                throw new InvalidOperationException(
                    "Cannot modify sequence while it is active.");
        }
    }

    public class Sequence : Sequence<object>
    {
        public Sequence() { }
        public Sequence(Action started = null, Action<bool> stopped = null)
            : base(started, stopped) { }
    }
}
