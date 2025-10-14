using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class PlayerNotDetected : BTNode<IMonster, MonsterBlackboard>
    {
        public PlayerNotDetected()
        {
            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }
    }
}
