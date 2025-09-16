using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Engaged : BTNode<Monster, MonsterBlackboard>
    {
        public Engaged()
        {
            LoopType = LoopType.UntilSuccess;
        }

        //protected override void OnTick()
        //{
        //    object _ = null;
        //}
    }
}
