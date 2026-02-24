using System;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Actions
{
    public record MonsterAnimationPlayInfo(
        string AnimationName,
        string TriggerName = null,
        float? StartTime = null,
        float? EndTime = null,
        Action<bool> Callback = null)
    {
        public MonsterAnimationPlayInfo(
            MonsterActionType ActionType,
            string Trigger = null,
            float? StartTime = null,
            float? EndTime = null,
            Action<bool> Callback = null)
            : this(ActionType.ToString(), Trigger, StartTime, EndTime, Callback) { }
    }

    public class MonsterAnimationPlayer : IDisposable
    {
        // Front
        public Animator Animator { get; }
        public float? CurrentAnimationLength { get; private set; }

        // Internal
        private Action _played;
        private bool _autoRun;

        private IDisposable _autoRunner;
        private Timer _timer;

        private bool _isPaused;
        private float _speed;


        // Content
        public MonsterAnimationPlayer(Animator animator, Action played = null)
        {
            if (!animator)
                throw new ArgumentException(
                    Ctx($"{nameof(animator)}이(가) 유효하지 않습니다."),
                    nameof(animator));

            Animator = animator;
            _played = played;
        }

        public void Play(MonsterAnimationPlayInfo playInfo, bool autoRun = true)
        {
            try
            {
                // 초기화
                _autoRun = autoRun;
                Animator.enabled = true;

                _isPaused = false;
                _speed = 1f;

                Animator.speed = _speed;
                ClearBindings();


                // 애니메니션 클립 가져오기
                if (!Animator.TryFindClip(playInfo.AnimationName, out var clip))
                {
                    throw new InvalidOperationException(Ctx(
                        $"{Animator.name}의 애니메이터에 '{playInfo.AnimationName}' 클립이 없습니다."));
                }

                if (!string.IsNullOrEmpty(playInfo.TriggerName))
                {
                    // 트리거를 통한 애니메이션 재생
                    if (!Animator.HasParameter(playInfo.TriggerName))
                        throw new InvalidOperationException(Ctx(
                            $"{Animator.name}의 애니메이터에 '{playInfo.TriggerName}' 트리거가 없습니다."));

                    if (playInfo.StartTime.HasValue || playInfo.EndTime.HasValue)
                        throw new InvalidOperationException(Ctx(
                            $"현재 트리거 방식 재생에서는 {nameof(playInfo.StartTime)}/{nameof(playInfo.EndTime)} 속성을 사용할 수 없습니다."));

                    Animator.SetTrigger(playInfo.TriggerName);
                }
                else
                {
                    // 이름을 통한 애니메이션 재생
                    Animator.Play(clip.name, -1, playInfo.StartTime.GetValueOrDefault(0f) / clip.length);
                }


                // 타이머 설정
                if (!clip.isLooping)
                {
                    CurrentAnimationLength = clip.length;

                    _timer = new Timer(
                        playInfo.EndTime.GetValueOrDefault(clip.length) - playInfo.StartTime.GetValueOrDefault(0f),
                        playInfo.Callback);
                }
                else
                    CurrentAnimationLength = null;


                if (autoRun)
                    _autoRunner = Loco.Subscribe(() => RunInternal(Time.deltaTime));

                _played?.Invoke();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Run(float deltaTime)
        {
            if (_autoRun)
                throw new InvalidOperationException(Ctx(
                    $"{nameof(_autoRun)} 속성이 true인 경우 애니메이터를 직접 재생할 수 없습니다."));

            RunInternal(deltaTime);
        }

        private void RunInternal(float deltaTime)
        {
            if (_isPaused || Time.deltaTime <= 0f)
                return;

            _speed = deltaTime / Time.deltaTime;

            if (_timer != null) _timer.Factor = _speed;
            Animator.speed = _speed;
        }

        public void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;

            _speed = Animator.speed;

            if (_timer != null) _timer.Factor = 0f;
            Animator.speed = 0f;
        }
        public void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;

            if (_timer != null) _timer.Factor = _speed;
            Animator.speed = _speed;
        }

        public void Stop()
        {
            ClearBindings();
            if (Animator) Animator.enabled = false;
        }

        private void ClearBindings()
        {
            _autoRunner?.Dispose();
            _autoRunner = null;

            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose() => ClearBindings();

        private string Ctx(string message) => $"[{nameof(MonsterAnimationPlayer)}] {message}";
    }
}
