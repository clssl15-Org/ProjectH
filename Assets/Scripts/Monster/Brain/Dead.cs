using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Dead : BTNode<Monster, MonsterBlackboard>
    {
        protected override void OnOpen(object[] _)
        {
            Owner.IsDying = true;
            Owner.DoAction(MonsterAction.Dead, Complete);
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.Die();
        }
    }
}
