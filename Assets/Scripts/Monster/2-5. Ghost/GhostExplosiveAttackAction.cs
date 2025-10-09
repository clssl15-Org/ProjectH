using System;
using MonsterActions;
using UniEngine.StateMachines.FSM;
using UnityEngine;

public partial class Ghost
{
    public class GhostExplosiveAttackAction : MonsterActionComponent
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
                .SetExitedAction(AnimationPlayer.Stop)
                .AddChild(new Work(phases[0])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new PlayInfo("Teleportation", "Teleportation_In", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[1]);
                        }))))
                .AddChild(new Work(phases[1])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new PlayInfo("Teleportation", "Teleportation_Out", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[2]);
                        }))))
                .AddChild(new Work(phases[1])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new PlayInfo("Attack_2", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[2]);
                        }))))
                .AddChild(new Work(phases[1])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new PlayInfo("Teleportation", "Teleportation_In", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[2]);
                        }))))
                .AddChild(new Work(phases[1])
                    .SetEnteredAction(() => AnimationPlayer.Play(
                        new PlayInfo("Teleportation", "Teleportation_Out", Callback: succeed =>
                        {
                            if (!succeed) throw AnimationFailure;
                            _work.SetNext(phases[2]);
                        }))));
        }

        public override void Enter(object input = null)
        {
            base.Enter(input);
            _work.Enter();
        }

        public override void Update(float elapsedTime)
        {
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
