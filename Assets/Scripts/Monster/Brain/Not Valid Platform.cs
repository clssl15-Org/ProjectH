using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class NotValidPlatform : BTNode<Monster, MonsterBlackboard>
    {
        public NotValidPlatform()
        {
            AbortPolicy = AbortPolicies.LowerPriority;
        }

        protected override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return false;

            if (Owner.PlatformDetector.CheckPlatform(
                Direction.Center, Owner.BelongingPlatform, out _))
            {
                return false;
            }

            return true;
        }

        protected override void OnOpen(object[] _)
        {
            Owner.DoAction(MonsterAction.Idle);
        }

        protected override void OnTick()
        {
            if (Owner.PlatformDetector.TryGetCurrentPlatformId(out var platformId))
            {
                Owner.BelongingPlatform = platformId;
                RetickNow = true;

                Complete();
            }
        }
    }
}
