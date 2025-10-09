using System;
using UniEngine.StateMachines.FSM;

namespace MonsterActions
{
    public class PlayAnimation : MonsterActionComponent
    {
        // Front
        public float DelayBeforePlay { get; set; } = 0f;
        public float DelayAfterPlay { get; set; } = 0f;
        public PlayInfo PlayInfo { get; set; }

        // Internal
        private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;

        private Work _work;
        private float _elapsedTime;
        private float _playingFinishedTime;


        // Content
        public PlayAnimation(PlayInfo playInfo)
        {
            PlayInfo = playInfo;

            _work = new Work()
                    .AddExitedAction(AnimationPlayer.Stop)
                .AddChild(new Work("BeforePlay")
                    .AddUpdatedAction(() =>
                    {
                        if (_elapsedTime >= DelayBeforePlay)
                            _work.SetNext("Play");
                    }))
                .AddChild(new Work("Play")
                    .AddEnteredAction(() => AnimationPlayer.Play(
                        PlayInfo with { Callback = succeed =>
                        {
                            PlayInfo.Callback?.Invoke(succeed);

                            if (!succeed) throw new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");
                            _work.SetNext("AfterPlay");
                        }}))
                    .AddExitedAction(() => _playingFinishedTime = _elapsedTime))
                .AddChild(new Work("AfterPlay")
                    .AddUpdatedAction(() =>
                    {
                        if (_elapsedTime >= _playingFinishedTime + DelayAfterPlay)
                            _work.Exit();
                    }));
        }

        public override void Enter(object input = null)
        {
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
