using System;
using Actors.Monsters.Actions;
using Infrastructure.StateMachines.Fsm;
using static Actors.Monsters.Actions.AttackWithWeapon;

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
            private Action _afterPostAction;

            private Payload _payload;

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
                Action beforePostAction = null,
                Action afterPostAction = null)
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
                _afterPostAction = afterPostAction;

                _work = new Work()
                    .OnExited(() => AnimationPlayer.Stop())
                    .AddChild(new Work("PreAction")
                        .OnEntered(() =>
                        {
                            _beforePreAction?.Invoke();

                            AnimationPlayer.Play(
                                new MonsterAnimationPlayInfo(_animations[0], Callback: succeed =>
                                {
                                    //if (!succeed) throw AnimationFailure;
                                    _work.SetNext("MainAction");
                                }));
                        }),
                        isPrimary: true
                    )
                    .AddChild(new Work("MainAction")
                        .OnEntered(() =>
                        {
                            _beforeMainAction?.Invoke();

                            var attackData = (MonsterAttackData)_payload.MonsterConditionData.Payload;
                            attackData.NotifyEvent(AttackEvent.Started);

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
                        .OnUpdated(() =>
                        {
                            if (_whileMainAction == null)
                                return;

                            var play = _whileMainAction.Invoke(
                                _elapsedTime - _mainActionEnteredTime,
                                AnimationPlayer.CurrentAnimationLength ?? -1);

                            if (!play)
                                _work.SetNext("PostAction");
                        })
                        .OnExited(() =>
                        {
                            var attackData = (MonsterAttackData)_payload.MonsterConditionData.Payload;
                            attackData.NotifyEvent(AttackEvent.Finished);
                        })
                    )
                    .AddChild(new Work("PostAction")
                        .OnEntered(() =>
                        {
                            AnimationPlayer.Play(
                                new MonsterAnimationPlayInfo(_animations[2], Callback: succeed =>
                                {
                                    //if (!succeed) throw AnimationFailure;
                                    Interrupt(InterruptType.Completed);
                                }));

                            _beforePostAction?.Invoke();
                        })
                        .OnExited(() => _afterPostAction?.Invoke())
                    );
            }

            protected override void OnEnter(object input)
            {
                if (input != null)
                {
                    if (input is not Payload payload)
                        throw new ArgumentException(
                            $"{nameof(input)}은(는) null이거나 {nameof(Payload)} 형식이어야 하지만 '{input.GetType().Name}' 형식이 입력되었습니다.",
                            nameof(input));

                    _payload = payload;
                }
                else
                    _payload = null;

                _elapsedTime = 0f;
                _work.Enter();
            }

            protected override void OnUpdate(float deltaTime)
            {
                _elapsedTime += deltaTime;
                _work.Update();
            }

            protected override void OnInterrupt(InterruptType reason)
            {
                _work.Exit();
                _payload = null;
            }
        }
    }
}
