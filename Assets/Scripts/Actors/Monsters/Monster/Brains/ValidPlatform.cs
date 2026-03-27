using Infrastructure;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal class ValidPlatform : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        private bool _bypassIfCommitting;

        public ValidPlatform(bool bypassIfCommitting = true)
        {
            AbortPolicies = AbortPolicies.Self;
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;

            _bypassIfCommitting = bypassIfCommitting;
        }

        public override bool CheckCondition()
        {
            if (_bypassIfCommitting && Blackboard.IsCommitting)
                return true;

            return Owner.PlatformDetector.CheckPlatform(
                Direction.Center, Owner.CurrentPlatform, out _);
        }
    }
}
