using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class ValidPlatform : BTNode<Monster, MonsterBlackboard>
    {
        public ValidPlatform()
        {
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;
        }
    }
}
