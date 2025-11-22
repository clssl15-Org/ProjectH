using System;
using Actors.Monsters.Actions;
using Infrastructure.StateMachines.Fsm;

namespace Actors.Monsters
{
    public partial class StagBeetle
    {
        private class ThreePhasedAction : MonsterActionComponent
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
            private Action _beforePostAction;

            //private readonly Exception AnimationFailure
            //    = new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");


            // Content
            public ThreePhasedAction(
                string animationName,
                Func<string, string> getPreActionName,
                Func<string, string> getPostActionName,
                Action beforePreAction = null,
                Action beforeMainAction = null,
                Func<float, float, bool> whileMainAction = null,
                Action beforePostAction = null)
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
                _beforePostAction = beforePostAction;

                _work = new Work()
                    .SetExitedAction(() => AnimationPlayer.Stop())
                    .AddChild(new Work("PreAction")
                        .SetEnteredAction(() =>
                        {
                            _beforePreAction?.Invoke();

                            AnimationPlayer.Play(
                                new MonsterAnimationPlayInfo(_animations[0], Callback: succeed =>
                                {
                                    //if (!succeed) throw AnimationFailure;
                                    _work.SetNext("MainAction");
                                }));
                        }), true)
                    .AddChild(new Work("MainAction")
                        .SetEnteredAction(() =>
                        {
                            _beforeMainAction?.Invoke();
                            _mainActionEnteredTime = _elapsedTime;

                            Action<bool> callback = _whileMainAction == null
                            ? (bool succeed) =>
                            {
                                //if (!succeed) throw AnimationFailure;
                                _work.SetNext("PostAction");
                            }
                            : null;

                            AnimationPlayer.Play(
                                new MonsterAnimationPlayInfo(_animations[1], Callback: callback));
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
                        }))
                    .AddChild(new Work("PostAction")
                        .SetEnteredAction(() =>
                        {
                            AnimationPlayer.Play(
                                new MonsterAnimationPlayInfo(_animations[2], Callback: succeed =>
                                {
                                    //if (!succeed) throw AnimationFailure;
                                    Interrupt(InterruptType.Completed);
                                }));

                            _beforePostAction?.Invoke();
                        }));
            }

            protected override void OnEnter(float _, object __)
            {
                _work.Enter();
            }

            protected override void OnUpdate(float elapsedTime)
            {
                _elapsedTime = elapsedTime;
                _work.Update();
            }

            protected override void OnInterrupt(InterruptType reason)
            {
                _work.Exit();
            }
        }
    }
}
