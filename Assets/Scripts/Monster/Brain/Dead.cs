using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Dead : BTNode<Monster, MonsterBlackboard>
    {
        protected override void OnOpen()
        {
            if (!Owner.TryDoAction(MonsterAction.Dead, Complete))
                Complete();
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.Die();
        }
    }
}
