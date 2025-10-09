using System;
using MonsterActions;
using UniEngine.StateMachines.FSM;

public partial class StagBeetle
{
    public class ThreePhasedAction : MonsterActionComponent
    {
        // Internal
        private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;

        private Work _work;
        private float _elapsedTime;
        private float _mainActionEnteredTime;

        private string[] _animations;
        private Action _beforePreAction;
        private Action _beforeMainAction;
        private Func<float, float, bool> _whileMainAction;
        private Action _afterMainAction;


        // Content
        public ThreePhasedAction(
            string animationName,
            Func<string, string> getPreActionName,
            Func<string, string> getPostActionName,
            Action beforePreAction = null,
            Action beforeMainAction = null,
            Func<float, float, bool> whileMainAction = null,
            Action afterMainAction = null)
        {
            _animations = new string[]
            {
                getPreActionName(animationName),
                animationName,
                getPostActionName(animationName)
            };

            _beforePreAction = beforePreAction;
            _beforeMainAction = beforeMainAction;
            _whileMainAction = whileMainAction;
            _afterMainAction = afterMainAction;

            _work = new Work()
                    .AddExitedAction(AnimationPlayer.Stop)
                .AddChild(new Work("PreAction")
                    .AddEnteredAction(() =>
                    {
                        _beforePreAction?.Invoke();

                        AnimationPlayer.Play(
                            new PlayInfo(_animations[0], Callback: succeed =>
                            {
                                if (!succeed) throw new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");
                                _work.SetNext("MainAction");
                            }));
                    })
                .AddChild(new Work("MainAction")
                    .AddEnteredAction(() =>
                    {
                        _beforeMainAction?.Invoke();
                        _mainActionEnteredTime = _elapsedTime;

                        AnimationPlayer.Play(
                            new PlayInfo(_animations[0], Callback: succeed =>
                            {
                                if (!succeed) throw new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");
                                _work.SetNext("MainAction");
                            }));
                    })
                    .AddUpdatedAction(() =>
                    {
                        if (_whileMainAction == null)
                            return;

                        var play = _whileMainAction.Invoke(
                            _elapsedTime - _mainActionEnteredTime,
                            AnimationPlayer.CurrentAnimationTime ?? -1);

                        if (!play)
                            _work.SetNext("PostAction");
                    })
                    .AddExitedAction(() => _afterMainAction?.Invoke()))
                .AddChild(new Work("PostAction")
                    .AddEnteredAction(() => AnimationPlayer.Play(
                        new PlayInfo(_animations[0], Callback: succeed =>
                        {
                            if (!succeed) throw new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");
                            _work.Exit();
                        })))));
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
