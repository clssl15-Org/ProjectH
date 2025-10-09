using System;
using UniEngine.StateMachines.FSM;

namespace MonsterActions
{
    public class PlayAnimation : MonsterActionComponent
    {
        // Front
        public record AnimationPlayInfo
        (
            float DelayBeforePlay = 0f,
            float DelayAfterPlay = 0f
        );

        public float DelayBeforePlay { get; set; } = 0f;
        public float DelayAfterPlay { get; set; } = 0f;
        public PlayInfo PlayInfo { get; set; }

        // Internal
        private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;
        private AnimationPlayInfo _currentAnimationPlayInfo;

        private Work _work;
        private float _elapsedTime;
        private float _playingFinishedTime;
        private object _playToken;


        // Content
        public PlayAnimation(PlayInfo playInfo)
        {
            PlayInfo = playInfo;

            _work = new Work()
                    .SetExitedAction(() => AnimationPlayer.Stop())
                .AddChild(new Work("BeforePlay")
                    .AddUpdatedAction(() =>
                    {
                        if (_elapsedTime >= _currentAnimationPlayInfo.DelayBeforePlay)
                            _work.SetNext("Play");
                    }), true)
                .AddChild(new Work("Play")
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        PlayInfo with { Callback = succeed =>
                        {
                            if (!Active) return;
                            PlayInfo.Callback?.Invoke(succeed);

                            if (!succeed) return;
                            _work.SetNext("AfterPlay");
                        }}))
                    .SetExitedAction(() => _playingFinishedTime = _elapsedTime))
                .AddChild(new Work("AfterPlay")
                    .AddUpdatedAction(() =>
                    {
                        if (_elapsedTime >= _playingFinishedTime + _currentAnimationPlayInfo.DelayAfterPlay)
                            _work.Exit();
                    }));
        }

        public override void Enter(object input = null)
        {
            if (input != null && input is not AnimationPlayInfo animationPlayInfo)
                throw new ArgumentException(MonsterAction.Owner.Ctx(
                    $"{nameof(input)}은(는) null이거나 {nameof(AnimationPlayInfo)} 형식이어야 하지만 " +
                    $"'{input.GetType().Name}' 형식이 입력되었습니다."),
                    nameof(input));


            _currentAnimationPlayInfo
                = input as AnimationPlayInfo
                ?? new(DelayBeforePlay, DelayAfterPlay);

            base.Enter(input);
            _work.Enter();
        }

        public override void Update(float elapsedTime)
        {
            _elapsedTime = elapsedTime;
            _work.Update();
        }

        public override void Interrupt(InterruptType reason)
        {
            if (!Active)
                return;

            _work.Exit();
            base.Interrupt(reason);
        }
    }
}
