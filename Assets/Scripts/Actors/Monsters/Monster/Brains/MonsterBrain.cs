using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal abstract class MonsterBrain : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        public MonsterBrain(IMonsterInternal owner = null, string name = null) : base(owner, name)
        {
            HierarchyMode = HierarchyMode.Selector;
        }
    }
}
