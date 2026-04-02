using Infrastructure.StateMachines.Fsm;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal class PlayAnimation : MonsterActionComponent
    {
        // Front
        public record AnimationPlayInfo
        (
            float DelayBeforePlay = 0f,

            //종료 시간 미설정 시 -1
            float DelayAfterPlay = 0f
        );

        public float DelayBeforePlay { get; set; } = 0f;
        public float DelayAfterPlay { get; set; } = 0f;
        public MonsterAnimationPlayInfo PlayInfo { get; set; }

        // Internal
        private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;
        private AnimationPlayInfo _currentAnimationPlayInfo;

        private Work _work;
        private float _deltaTime;
        private float _elapsedTime;
        private float _playingFinishedTime;
        private object _playToken;


        // Content
        public PlayAnimation(MonsterAnimationPlayInfo playInfo)
        {
            PlayInfo = playInfo;

            _work = new Work()
                .OnExited(() => AnimationPlayer.Stop())
                .AddChild(new Work("BeforePlay")
                    .OnUpdated(() =>
                    {
                        if (_elapsedTime >= _currentAnimationPlayInfo.DelayBeforePlay)
                            _work.SetNext("Play");
                    }),
                    isPrimary: true
                )
                .AddChild(new Work("Play")
                    .OnEntered(() => AnimationPlayer.Play(
                        PlayInfo with { Callback = succeed =>
                        {
                            if (!Active) return;
                            PlayInfo.Callback?.Invoke(succeed);

                            if (!succeed)
                                return;

                            if (_currentAnimationPlayInfo.DelayAfterPlay < 0)
                            {
                                _work.Exit();
                                return;
                            }

                            _work.SetNext("AfterPlay");
                        }},
                        autoRun: false)
                    )
                    .OnUpdated(() => AnimationPlayer.Run(_deltaTime))
                    .OnExited(() => _playingFinishedTime = _elapsedTime)
                )
                .AddChild(new Work("AfterPlay")
                    .OnUpdated(() =>
                    {
                        if (_elapsedTime >= _playingFinishedTime + _currentAnimationPlayInfo.DelayAfterPlay)
                            Interrupt(InterruptType.Completed);
                    })
                );
        }

        protected override void OnEnter(object input)
        {
            _currentAnimationPlayInfo
                = input as AnimationPlayInfo
                ?? new(DelayBeforePlay, DelayAfterPlay);

            _elapsedTime = 0f;
            _work.Enter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _deltaTime = deltaTime;
            _elapsedTime += deltaTime;

            _work.Update();
        }

        protected override void OnInterrupt(InterruptType _)
        {
            _work.Exit();
        }
    }
}
