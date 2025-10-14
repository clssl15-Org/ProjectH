using System;
using MonsterActions;
using UniEngine.StateMachines.FSM;
using UnityEngine;

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

        private readonly Exception AnimationFailure
            = new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");


        // Content
        public GhostExplosiveAttackAction()
        {
            _work = new Work()
                .SetExitedAction(() => AnimationPlayer.Stop())
                .AddChild(new Work(phases[0])
                    .SetEnteredAction(() => _originalPosition = MonsterAction.Owner.transform.position)
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new MonsterAnimationPlayInfo("Teleportation", "Teleportation_In", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[1]);
                        }))), true)
                .AddChild(new Work(phases[1])
                    .SetEnteredAction(() =>
                    {
                        var direction = MonsterAction.Owner.Direction.ToVector3();
                        MonsterAction.Owner.transform.position = _originalPosition + direction * TeleportDistance;
                    })
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new MonsterAnimationPlayInfo("Teleportation", "Teleportation_Out", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[2]);
                        }))))
                .AddChild(new Work(phases[2])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new MonsterAnimationPlayInfo("Attack_2", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[3]);
                        }))))
                .AddChild(new Work(phases[3])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new MonsterAnimationPlayInfo("Teleportation", "Teleportation_In", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[4]);
                        }))))
                .AddChild(new Work(phases[4])
                    .SetEnteredAction(() => MonsterAction.Owner.transform.position = _originalPosition)
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new MonsterAnimationPlayInfo("Teleportation", "Teleportation_Out", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            Interrupt(InterruptType.Completed);
                        }))));
        }

        protected override void OnEnter(object input = null)
        {
            _work.Enter();
        }

        protected override void OnUpdate(float elapsedTime)
        {
            _work.Update();
        }

        protected override void OnInterrupt(InterruptType reason)
        {
            _work.Exit();
        }
    }
}
