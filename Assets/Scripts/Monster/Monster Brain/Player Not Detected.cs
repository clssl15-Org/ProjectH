using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class PlayerNotDetected : BTNode<Monster, MonsterBlackboard>
    {
        public PlayerNotDetected()
        {
            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }
    }
}
