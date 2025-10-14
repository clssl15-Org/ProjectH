using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class Alive : BTNode<IMonster, MonsterBlackboard>
    {
        public Alive()
        {
            AbortPolicy = AbortPolicies.Self;
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
