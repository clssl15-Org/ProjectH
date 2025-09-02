using System;
using MonsterActions;
using UniEngine.StateMachines.FSM;
using UnityEngine;

public partial class SpikeSnail : Monster
{
    private class SpikeSnailAttackAction : Work<MonsterActionController>
    {
        // Internal
        private SpikeSnail Owner => (SpikeSnail)Parent.Owner;

        private float playtime;
        private bool launched;
        private bool paused;
        private bool restarted;

        private float targetPlayTime;
        private bool isCompleted;
        private Action<ActionResult> callback;


        // Content
        public SpikeSnailAttackAction() : base(MonsterAction.Attack.ToString()) { }

        protected override void OnEnter(params object[] inputs)
        {
            if (!Owner.Animator.TryFindClip(MonsterAction.Attack.ToString(), out var clip))
                throw new ArgumentException(Owner.Ctx($"애니메이터가 애니메이션 {MonsterAction.Attack.ToString()}을(를) 가지고 있지 않습니다."));

            callback = (Action<ActionResult>)inputs[0];

            launched = false;
            paused = false;
            restarted = false;

            playtime = 0;
            targetPlayTime = clip.length + Owner.waitingTime;

            isCompleted = false;
        }

        protected override void OnUpdate()
        {
            playtime += Time.deltaTime;

            if (!launched && playtime >= Owner.launchTime)
            {
                launched = true;

                Owner.spikeLauncher.LaunchWithDirections(Owner.spikeSpeed,
                    new Vector2[] { new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0) });
            }

            if (!paused && playtime >= (Owner.launchTime + Owner.playtimeBeforeWaiting))
            {
                paused = true;
                Owner.Animator.speed = 0f;
            }

            if (!restarted && playtime >= (Owner.launchTime + Owner.playtimeBeforeWaiting + Owner.waitingTime))
            {
                restarted = true;
                Owner.Animator.speed = 1f;
            }

            if (playtime >= targetPlayTime)
            {
                isCompleted = true;
                Exit();
            }
        }

        protected override void OnExit()
        {
            if (Owner.Animator)
                Owner.Animator.speed = 1f;

            var callback = this.callback;
            this.callback = null;

            callback?.Invoke(new(isCompleted
                ? ActionResult.ResultType.Success
                : ActionResult.ResultType.Interrupted));
        }
    }
}
