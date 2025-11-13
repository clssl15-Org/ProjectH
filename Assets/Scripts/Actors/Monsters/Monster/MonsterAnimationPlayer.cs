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
            MonsterActionType actionType,
            string trigger = null,
            float? startTime = null,
            float? endTime = null,
            Action<bool> callback = null)
            : this(actionType.ToString(), trigger, startTime, endTime, callback) { }
    }

    public class MonsterAnimationPlayer : IDisposable
    {
        // Front
        public Animator Animator { get; }
        public float? CurrentAnimationTime { get; private set; }

        // Internal
        private Timer _timer;


        // Content
        public MonsterAnimationPlayer(Animator animator)
        {
            if (!animator)
                throw new ArgumentException(nameof(animator), $"{nameof(animator)}이(가) 유효하지 않습니다.");

            Animator = animator;
        }

        public void Play(MonsterAnimationPlayInfo playInfo)
        {
            try
            {
                // 초기화
                Animator.enabled = true;
                ClearTimer();


                // 애니메니션 클립 가져오기
                if (!Animator.TryFindClip(playInfo.AnimationName, out var clip))
                {
                    throw new InvalidOperationException(
                        $"{Animator.name}의 애니메이터에 '{playInfo.AnimationName}' 클립이 없습니다.");
                }

                if (!string.IsNullOrEmpty(playInfo.TriggerName))
                {
                    // 트리거를 통한 애니메이션 재생
                    if (!Animator.HasParameter(playInfo.TriggerName))
                        throw new InvalidOperationException(
                            $"{Animator.name}의 애니메이터에 '{playInfo.TriggerName}' 트리거가 없습니다.");

                    if (playInfo.StartTime.HasValue || playInfo.EndTime.HasValue)
                        throw new InvalidOperationException(
                            $"현재 트리거 방식 재생에서는 {nameof(playInfo.StartTime)}/{nameof(playInfo.EndTime)} 속성을 사용할 수 없습니다.");

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
                    CurrentAnimationTime = clip.length;

                    _timer = new Timer(
                        playInfo.EndTime.GetValueOrDefault(clip.length) - playInfo.StartTime.GetValueOrDefault(0f),
                        playInfo.Callback);
                }
                else
                    CurrentAnimationTime = null;

                Resume();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Pause()
        {
            if (_timer != null)
                _timer.Factor = 0f;

            Animator.speed = 0f;
        }
        public void Resume()
        {
            if (_timer != null)
                _timer.Factor = 1f;

            Animator.speed = 1f;
        }

        public void Stop()
        {
            ClearTimer();
            Animator.enabled = false;
        }

        private void ClearTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }


        public void Dispose() => ClearTimer();
    }
}
