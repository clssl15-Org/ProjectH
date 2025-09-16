using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Engaged : BTNode<Monster, MonsterBlackboard>
    {
        public Engaged()
        {
            LoopType = LoopType.UntilSuccess;
        }
    }
}
