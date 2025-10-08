using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal abstract class MonsterBrain : BTNode<Monster, MonsterBlackboard>
    {
        public MonsterBrain(Monster owner = null, string name = null) : base(owner, name)
        {
            HierarchyMode = HierarchyMode.Selector;
        }
    }
}
