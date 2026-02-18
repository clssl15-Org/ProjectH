using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class Ghost
    {
        private class GhostExplosiveAttackAction : MonsterActionComponent
        {
            // Front
            public float TeleportDistance { get; set; } = 1.5f;

            // Internal
            private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;

            private Work _work;
            private Vector3 _originalPosition;

            private readonly string[] phases = new[]
            {
                "Approaching",
                "Approached",
                "Attack",
                "Retreating",
                "Retreated"
            };

            //private readonly Exception AnimationFailure
            //    = new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");


            // Content
            public GhostExplosiveAttackAction()
            {
                _work = new Work()
                    .OnExited(() => AnimationPlayer.Stop())
                    .AddChild(new Work(phases[0])
                        .OnEntered(() => _originalPosition = MonsterAction.Owner.transform.position)
                        .OnEntered(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo("Teleportation", "Teleportation_In", Callback: succeed =>
                            {
                                //if (!succeed) throw AnimationFailure;
                                _work.SetNext(phases[1]);
                            }))), true)
                    .AddChild(new Work(phases[1])
                        .OnEntered(() =>
                        {
                            var direction = MonsterAction.Owner.Direction.ToVector3();
                            MonsterAction.Owner.transform.position = _originalPosition + direction * TeleportDistance;
                        })
                        .OnEntered(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo("Teleportation", "Teleportation_Out", Callback: succeed =>
                            {
                                //if (!succeed) throw AnimationFailure;
                                _work.SetNext(phases[2]);
                            }))))
                    .AddChild(new Work(phases[2])
                        .OnEntered(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo("Attack_2", Callback: succeed =>
                            {
                                //if (!succeed) throw AnimationFailure;
                                _work.SetNext(phases[3]);
                            }))))
                    .AddChild(new Work(phases[3])
                        .OnEntered(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo("Teleportation", "Teleportation_In", Callback: succeed =>
                            {
                                //if (!succeed) throw AnimationFailure;
                                _work.SetNext(phases[4]);
                            }))))
                    .AddChild(new Work(phases[4])
                        .OnEntered(() => MonsterAction.Owner.transform.position = _originalPosition)
                        .OnEntered(() => AnimationPlayer.Play(
                            new MonsterAnimationPlayInfo("Teleportation", "Teleportation_Out", Callback: succeed =>
                            {
                                //if (!succeed) throw AnimationFailure;
                                Interrupt(InterruptType.Completed);
                            }))));
            }

            protected override void OnEnter(object _)
            {
                _work.Enter();
            }

            protected override void OnUpdate(float _)
            {
                _work.Update();
            }

            protected override void OnInterrupt(InterruptType reason)
            {
                _work.Exit();
            }
        }
    }
}
