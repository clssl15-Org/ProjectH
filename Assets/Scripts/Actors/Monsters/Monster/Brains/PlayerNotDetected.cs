using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal class PlayerNotDetected : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        public PlayerNotDetected()
        {
            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }
    }
}
