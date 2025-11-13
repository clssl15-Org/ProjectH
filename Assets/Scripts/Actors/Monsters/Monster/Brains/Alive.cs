using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal class Alive : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        public Alive()
        {
            AbortPolicies = AbortPolicies.Self;
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;
        }

        public override bool CheckCondition()
        {
            if (Owner.HP <= 0)
            {
                Owner.HP = 0;
                return false;
            }

            return true;
        }
    }
}
