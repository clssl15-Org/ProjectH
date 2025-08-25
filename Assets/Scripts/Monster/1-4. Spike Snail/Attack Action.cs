using MonsterActions;

public partial class SpikeSnail : Monster
{
    private class SpikeSnailAttackAction : MonsteActionState
    {
        // Internal
        private float playtime;
        private bool launched;
        private bool paused;
        private bool restarted;


        // Content
        public SpikeSnailAttackAction() : base(MonsterAction.Attack.ToString()) { }

        protected override void OnEnter(params object[] inputs)
        {
            launched = false;
            paused = false;
            restarted = false;

            base.OnEnter(inputs);

            if (!RemainingTime.HasValue)
                throw new System.InvalidOperationException("가시달팽이의 Attack 행동은 종료 시간이 존재해야 합니다.");

            RemainingTime += ((SpikeSnail)Owner).waitingTime;
            playtime = RemainingTime.Value;
        }
        
        protected override void OnUpdate()
        {
            base.OnUpdate();

            var owner = (SpikeSnail)Owner;
            var currentPlaytime = playtime - RemainingTime;

            if (!launched && currentPlaytime >= owner.launchTime)
            {
                launched = true;
                owner.spikeLauncher.Launch();
            }

            if (!paused && currentPlaytime >= (owner.launchTime + owner.playAfterlaunchTime))
            {
                paused = true;
                Owner.Animator.speed = 0f;
            }

            if (!restarted && currentPlaytime >= (owner.launchTime + owner.playAfterlaunchTime + owner.waitingTime))
            {
                restarted = true;
                Owner.Animator.speed = 1f;
            }
        }

        protected override void OnExit()
        {
            if (Owner.Animator)
                Owner.Animator.speed = 1f;
        }
    }
}
