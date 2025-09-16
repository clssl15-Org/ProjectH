using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Alive : BTNode<Monster, MonsterBlackboard>
    {
        public Alive()
        {
            SelectionOption = SelectionOptions.Self;
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
