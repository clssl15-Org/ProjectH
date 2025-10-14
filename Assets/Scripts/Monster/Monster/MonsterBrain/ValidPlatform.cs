using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class ValidPlatform : BTNode<IMonster, MonsterBlackboard>
    {
        public ValidPlatform()
        {
            AbortPolicy = AbortPolicies.Self;
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;
        }


        public override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return true;

            return Owner.PlatformDetector.CheckPlatform(
                Direction.Center, Owner.BelongingPlatform, out _);
        }
    }
}
