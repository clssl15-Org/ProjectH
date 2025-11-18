using System;
using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.FSM;
using Unity.VisualScripting;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class Belia
    {
        private class BeliaCurvedAreaAttackAction : MonsterActionComponent
        {
            public float EffectLength { get; set; }

            private readonly Exception AnimationFailure
                = new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");

            private GameObject _curveEffectPrefab;
            private Vector2 _curveEffectWorldPosition;

            private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;
            private Work _work;

            public BeliaCurvedAreaAttackAction(
                GameObject curveEffectPrefab,
                Vector2 curveEffectWorldPosition,
                float effectLength = 1)
            {
                _curveEffectPrefab = curveEffectPrefab;
                _curveEffectWorldPosition = curveEffectWorldPosition;
                EffectLength = effectLength;
                
                GameObject effectInstance = default;
                float effectRemainingTime = default;
                Material effectMat = default;

                _work = new Work()
                    .SetExitedAction(() => AnimationPlayer.Stop())
                    .AddChild(new Work("PreAction")
                        .SetEnteredAction(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo(
                                "CurvedAreaAttackStart",
                                Callback: succeed =>
                                {
                                    if (!succeed) throw AnimationFailure;
                                    _work.SetNext("MainAction");
                                }))
                        ), true
                    )
                    .AddChild(new Work("MainAction")
                        .SetEnteredAction(() =>
                        {
                            effectInstance = Instantiate(_curveEffectPrefab);
                            effectInstance.transform.localScale = Vector3.one;
                            effectInstance.transform.position = _curveEffectWorldPosition;
                            effectInstance.SetActive(true);

                            effectMat = effectInstance.GetComponent<SpriteRenderer>().material;
                            effectRemainingTime = EffectLength;
                        })
                        .AddUpdatedAction(() =>
                        {
                            effectRemainingTime -= Time.deltaTime;
                            effectMat.color = effectMat.color.WithAlpha(effectRemainingTime / EffectLength);

                            if (effectRemainingTime <= 0)
                                _work.SetNext("PostAction");
                        })
                        .SetExitedAction(() => Destroy(effectInstance))
                    )
                    .AddChild(new Work("PostAction")
                        .SetEnteredAction(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo(
                                "CurvedAreaAttackEnd",
                                Callback: succeed =>
                                {
                                    if (!succeed) throw AnimationFailure;
                                    Interrupt(InterruptType.Completed);
                                })
                            )
                        )
                    );
            }

            protected override void OnEnter(object _)
            {
                _work.Enter();
            }

            protected override void OnUpdate(float _)
            {
                _work.Update();
            }

            protected override void OnInterrupt(InterruptType _)
            {
                _work.Exit();
            }
        }
    }
}
