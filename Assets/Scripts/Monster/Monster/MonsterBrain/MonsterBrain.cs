using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal abstract class MonsterBrain : BTNode<IMonster, MonsterBlackboard>
    {
        public MonsterBrain(IMonster owner = null, string name = null) : base(owner, name)
        {
            HierarchyMode = HierarchyMode.Selector;
        }
    }
}
