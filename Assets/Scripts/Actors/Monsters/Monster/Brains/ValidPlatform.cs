using Infrastructure;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal class ValidPlatform : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        public ValidPlatform()
        {
            AbortPolicies = AbortPolicies.Self;
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;
        }


        public override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return true;

            return Owner.PlatformDetector.CheckPlatform(
                Direction.Center, Owner.CurrentPlatform, out _);
        }
    }
}
